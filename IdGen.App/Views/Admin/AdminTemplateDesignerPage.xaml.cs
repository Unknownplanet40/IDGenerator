using System.Collections.Specialized;
using System.ComponentModel;
using IdGen.App.Controls;
using IdGen.App.ViewModels.Admin;
using IdGen.Core.Models.Template;
using IdGen.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

namespace IdGen.App.Views.Admin;

public sealed partial class AdminTemplateDesignerPage : Page
{
    public AdminTemplateDesignerViewModel ViewModel { get; }

    // Fit & Zoom tracking
    private bool _isAutoFit = true;

    // Drag tracking
    private bool _isDragging;
    private TemplateElementDefinition? _draggingElement;
    private FrameworkElement? _draggingWrapper;
    private Point _dragStartPoint;
    private double _initialElementX;
    private double _initialElementY;

    // Resizing engine
    private bool _isResizing;
    private string _resizeDir = "";
    private Point _resizeStartPoint;
    private double _initialResizeX;
    private double _initialResizeY;
    private double _initialResizeW;
    private double _initialResizeH;
    private FrameworkElement? _activeResizeHandle;

    private Grid? _adornerContainer;
    private Border? _adornerBorder;
    private Border? _lockBadge;
    private readonly Dictionary<string, Border> _resizeHandles = new();

    public AdminTemplateDesignerPage()
    {
        ViewModel = App.CurrentServices.GetRequiredService<AdminTemplateDesignerViewModel>();
        InitializeComponent();

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.CurrentElements.CollectionChanged += CurrentElements_CollectionChanged;

        Loaded += async (s, e) =>
        {
            await ViewModel.InitializeAsync();
            RebuildCanvasChildren();
            UpdateSideButtons();
            UpdateInspectorPanels();
            FitToScreen();
        };
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedElement) || e.PropertyName == nameof(ViewModel.HasSelectedElement))
        {
            UpdateSelectionVisuals();
            UpdateInspectorPanels();
            UpdateAdorner();
        }
        else if (e.PropertyName == nameof(ViewModel.ActiveSide) ||
                 e.PropertyName == nameof(ViewModel.CanvasWidth) ||
                 e.PropertyName == nameof(ViewModel.CanvasHeight))
        {
            RebuildCanvasChildren();
            UpdateSideButtons();
            if (_isAutoFit) FitToScreen();
            UpdateAdorner();
        }
        else if (e.PropertyName == nameof(ViewModel.BackgroundColor))
        {
            UpdateBackgroundColor();
        }
    }

    private void CurrentElements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildCanvasChildren();
    }

    private void UpdateSideButtons()
    {
        if (Application.Current.Resources.TryGetValue("AccentButtonStyle", out object? accentObj) && accentObj is Style accentStyle)
        {
            if (ViewModel.ActiveSide == "Front")
            {
                BtnFrontSide.Style = accentStyle;
                BtnBackSide.ClearValue(Button.StyleProperty);
            }
            else
            {
                BtnFrontSide.ClearValue(Button.StyleProperty);
                BtnBackSide.Style = accentStyle;
            }
        }
    }

    private void UpdateBackgroundColor()
    {
        if (TemplateValidator.IsValidHexColor(ViewModel.BackgroundColor))
        {
            CardBackgroundBorder.Background = ParseHexBrush(ViewModel.BackgroundColor);
        }
    }

    private void RebuildCanvasChildren()
    {
        ElementsCanvas.Children.Clear();
        UpdateBackgroundColor();

        foreach (var el in ViewModel.CurrentElements)
        {
            var wrapper = CreateElementWrapper(el);
            ElementsCanvas.Children.Add(wrapper);
        }

        UpdateSelectionVisuals();
        UpdateAdorner();
    }

    private FrameworkElement CreateElementWrapper(TemplateElementDefinition el)
    {
        var view = new TemplateElementView
        {
            Element = el,
            SampleText = ViewModel.ResolveSampleText(el),
            LogoSource = ViewModel.SchoolLogoImage,
            IsSelected = (el == ViewModel.SelectedElement),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var wrapper = new Grid
        {
            Width = el.Width,
            Height = el.Height,
            Tag = el
        };

        Canvas.SetLeft(wrapper, el.X);
        Canvas.SetTop(wrapper, el.Y);

        wrapper.Children.Add(view);

        // Interactive Pointer Event Handling
        wrapper.PointerPressed += (s, e) =>
        {
            e.Handled = true;
            ViewModel.SelectElement(el);

            if (!el.Locked)
            {
                _isDragging = true;
                _draggingElement = el;
                _draggingWrapper = wrapper;
                _dragStartPoint = e.GetCurrentPoint(ElementsCanvas).Position;
                _initialElementX = el.X;
                _initialElementY = el.Y;
                wrapper.CapturePointer(e.Pointer);
            }
        };

        wrapper.PointerMoved += (s, e) =>
        {
            if (_isDragging && _draggingElement == el && _draggingWrapper == wrapper && !el.Locked)
            {
                var currentPoint = e.GetCurrentPoint(ElementsCanvas).Position;
                double deltaX = currentPoint.X - _dragStartPoint.X;
                double deltaY = currentPoint.Y - _dragStartPoint.Y;

                double newX = Math.Round(_initialElementX + deltaX);
                double newY = Math.Round(_initialElementY + deltaY);

                // Clamp inside canvas boundary
                newX = Math.Clamp(newX, 0, ViewModel.CanvasWidth - el.Width);
                newY = Math.Clamp(newY, 0, ViewModel.CanvasHeight - el.Height);

                el.X = newX;
                el.Y = newY;

                Canvas.SetLeft(wrapper, el.X);
                Canvas.SetTop(wrapper, el.Y);

                if (el == ViewModel.SelectedElement && _adornerContainer != null)
                {
                    Canvas.SetLeft(_adornerContainer, el.X);
                    Canvas.SetTop(_adornerContainer, el.Y);
                    if (PropInputX != null) PropInputX.Value = el.X;
                    if (PropInputY != null) PropInputY.Value = el.Y;
                }
            }
        };

        wrapper.PointerReleased += (s, e) =>
        {
            if (_isDragging && _draggingElement == el)
            {
                _isDragging = false;
                _draggingElement = null;
                _draggingWrapper?.ReleasePointerCapture(e.Pointer);
                _draggingWrapper = null;
                ViewModel.TakeSnapshot();
                ViewModel.IsDirty = true;
                UpdateAdorner();
            }
        };

        return wrapper;
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var child in ElementsCanvas.Children)
        {
            if (child is Grid wrapper && wrapper.Tag is TemplateElementDefinition el)
            {
                if (wrapper.Children.Count > 0 && wrapper.Children[0] is TemplateElementView view)
                {
                    view.IsSelected = (el == ViewModel.SelectedElement);
                }
            }
        }
    }

    #region Selection & Resize Adorner Engine

    private void SetupAdorner()
    {
        AdornerCanvas.Children.Clear();
        _resizeHandles.Clear();

        _adornerContainer = new Grid
        {
            IsHitTestVisible = true
        };

        _adornerBorder = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 37, 99, 235)),
            BorderThickness = new Thickness(2),
            IsHitTestVisible = false,
            CornerRadius = new CornerRadius(2)
        };
        _adornerContainer.Children.Add(_adornerBorder);

        _lockBadge = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, -10, -10, 0),
            Padding = new Thickness(4, 2, 4, 2),
            CornerRadius = new CornerRadius(10),
            Background = (Brush)Application.Current.Resources["SystemFillColorCautionBrush"],
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false,
            Child = new FontIcon { Glyph = "\uE72E", FontSize = 11, Foreground = new SolidColorBrush(Colors.White) }
        };
        _adornerContainer.Children.Add(_lockBadge);

        CreateHandle("NW", InputSystemCursorShape.SizeNorthwestSoutheast);
        CreateHandle("N", InputSystemCursorShape.SizeNorthSouth);
        CreateHandle("NE", InputSystemCursorShape.SizeNortheastSouthwest);
        CreateHandle("E", InputSystemCursorShape.SizeWestEast);
        CreateHandle("SE", InputSystemCursorShape.SizeNorthwestSoutheast);
        CreateHandle("S", InputSystemCursorShape.SizeNorthSouth);
        CreateHandle("SW", InputSystemCursorShape.SizeNortheastSouthwest);
        CreateHandle("W", InputSystemCursorShape.SizeWestEast);

        AdornerCanvas.Children.Add(_adornerContainer);
    }

    private void CreateHandle(string dir, InputSystemCursorShape cursorShape)
    {
        var handle = new Border
        {
            Width = 10,
            Height = 10,
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 37, 99, 235)),
            BorderThickness = new Thickness(1.5),
            CornerRadius = new CornerRadius(2),
            Tag = dir
        };

        handle.PointerEntered += (s, e) =>
        {
            ProtectedCursor = InputSystemCursor.Create(cursorShape);
        };
        handle.PointerExited += (s, e) =>
        {
            if (!_isResizing)
            {
                ProtectedCursor = null;
            }
        };

        handle.PointerPressed += Handle_PointerPressed;
        handle.PointerMoved += Handle_PointerMoved;
        handle.PointerReleased += Handle_PointerReleased;
        handle.PointerCanceled += Handle_PointerCanceled;
        handle.PointerCaptureLost += Handle_PointerCanceled;

        _resizeHandles[dir] = handle;
        _adornerContainer?.Children.Add(handle);
    }

    private void UpdateAdorner()
    {
        var sel = ViewModel.SelectedElement;
        if (sel == null)
        {
            if (_adornerContainer != null) _adornerContainer.Visibility = Visibility.Collapsed;
            return;
        }

        if (_adornerContainer == null)
        {
            SetupAdorner();
        }

        if (_adornerContainer == null) return;
        _adornerContainer.Visibility = Visibility.Visible;

        Canvas.SetLeft(_adornerContainer, sel.X);
        Canvas.SetTop(_adornerContainer, sel.Y);
        _adornerContainer.Width = sel.Width;
        _adornerContainer.Height = sel.Height;

        bool isLocked = sel.Locked;
        if (_lockBadge != null)
        {
            _lockBadge.Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
        }

        double halfW = sel.Width / 2.0;
        double halfH = sel.Height / 2.0;
        const double offset = 5.0;

        foreach (var kvp in _resizeHandles)
        {
            var dir = kvp.Key;
            var handle = kvp.Value;

            if (isLocked)
            {
                handle.Visibility = Visibility.Collapsed;
                continue;
            }
            handle.Visibility = Visibility.Visible;

            double left = 0, top = 0;
            switch (dir)
            {
                case "NW": left = -offset; top = -offset; break;
                case "N": left = halfW - offset; top = -offset; break;
                case "NE": left = sel.Width - offset; top = -offset; break;
                case "E": left = sel.Width - offset; top = halfH - offset; break;
                case "SE": left = sel.Width - offset; top = sel.Height - offset; break;
                case "S": left = halfW - offset; top = sel.Height - offset; break;
                case "SW": left = -offset; top = sel.Height - offset; break;
                case "W": left = -offset; top = halfH - offset; break;
            }

            handle.Margin = new Thickness(left, top, 0, 0);
            handle.HorizontalAlignment = HorizontalAlignment.Left;
            handle.VerticalAlignment = VerticalAlignment.Top;
        }
    }

    private void Handle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var sel = ViewModel.SelectedElement;
        if (sel == null || sel.Locked) return;

        if (sender is FrameworkElement handle && handle.Tag is string dir)
        {
            e.Handled = true;
            _isResizing = true;
            _resizeDir = dir;
            _activeResizeHandle = handle;
            _resizeStartPoint = e.GetCurrentPoint(ElementsCanvas).Position;
            _initialResizeX = sel.X;
            _initialResizeY = sel.Y;
            _initialResizeW = sel.Width;
            _initialResizeH = sel.Height;
            handle.CapturePointer(e.Pointer);
        }
    }

    private void Handle_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isResizing || ViewModel.SelectedElement == null) return;
        var sel = ViewModel.SelectedElement;
        e.Handled = true;

        var curPt = e.GetCurrentPoint(ElementsCanvas).Position;
        double dx = curPt.X - _resizeStartPoint.X;
        double dy = curPt.Y - _resizeStartPoint.Y;

        double newX = _initialResizeX;
        double newY = _initialResizeY;
        double newW = _initialResizeW;
        double newH = _initialResizeH;

        double minW = 20;
        double minH = 10;
        double maxW = ViewModel.CanvasWidth;
        double maxH = ViewModel.CanvasHeight;

        switch (_resizeDir)
        {
            case "E":
                newW = Math.Clamp(_initialResizeW + dx, minW, maxW - newX);
                break;
            case "S":
                newH = Math.Clamp(_initialResizeH + dy, minH, maxH - newY);
                break;
            case "SE":
                newW = Math.Clamp(_initialResizeW + dx, minW, maxW - newX);
                newH = Math.Clamp(_initialResizeH + dy, minH, maxH - newY);
                break;
            case "W":
                newX = Math.Clamp(_initialResizeX + dx, 0, _initialResizeX + _initialResizeW - minW);
                newW = _initialResizeW + (_initialResizeX - newX);
                break;
            case "N":
                newY = Math.Clamp(_initialResizeY + dy, 0, _initialResizeY + _initialResizeH - minH);
                newH = _initialResizeH + (_initialResizeY - newY);
                break;
            case "NW":
                newX = Math.Clamp(_initialResizeX + dx, 0, _initialResizeX + _initialResizeW - minW);
                newW = _initialResizeW + (_initialResizeX - newX);
                newY = Math.Clamp(_initialResizeY + dy, 0, _initialResizeY + _initialResizeH - minH);
                newH = _initialResizeH + (_initialResizeY - newY);
                break;
            case "NE":
                newY = Math.Clamp(_initialResizeY + dy, 0, _initialResizeY + _initialResizeH - minH);
                newH = _initialResizeH + (_initialResizeY - newY);
                newW = Math.Clamp(_initialResizeW + dx, minW, maxW - newX);
                break;
            case "SW":
                newX = Math.Clamp(_initialResizeX + dx, 0, _initialResizeX + _initialResizeW - minW);
                newW = _initialResizeW + (_initialResizeX - newX);
                newH = Math.Clamp(_initialResizeH + dy, minH, maxH - newY);
                break;
        }

        sel.X = Math.Round(newX);
        sel.Y = Math.Round(newY);
        sel.Width = Math.Round(newW);
        sel.Height = Math.Round(newH);

        // Update active element wrapper on ElementsCanvas
        foreach (var child in ElementsCanvas.Children)
        {
            if (child is Grid wrapper && wrapper.Tag == sel)
            {
                Canvas.SetLeft(wrapper, sel.X);
                Canvas.SetTop(wrapper, sel.Y);
                wrapper.Width = sel.Width;
                wrapper.Height = sel.Height;
                if (wrapper.Children.Count > 0 && wrapper.Children[0] is TemplateElementView view)
                {
                    view.RenderElement();
                }
                break;
            }
        }

        UpdateAdorner();

        if (PropInputX != null) PropInputX.Value = sel.X;
        if (PropInputY != null) PropInputY.Value = sel.Y;
        if (PropInputWidth != null) PropInputWidth.Value = sel.Width;
        if (PropInputHeight != null) PropInputHeight.Value = sel.Height;
    }

    private void Handle_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isResizing)
        {
            _isResizing = false;
            ProtectedCursor = null;
            if (_activeResizeHandle != null)
            {
                _activeResizeHandle.ReleasePointerCapture(e.Pointer);
                _activeResizeHandle = null;
            }
            ViewModel.TakeSnapshot();
            ViewModel.IsDirty = true;
        }
    }

    private void Handle_PointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        if (_isResizing)
        {
            _isResizing = false;
            ProtectedCursor = null;
            if (_activeResizeHandle != null)
            {
                _activeResizeHandle.ReleasePointerCapture(e.Pointer);
                _activeResizeHandle = null;
            }
        }
    }

    #endregion

    private void UpdateInspectorPanels()
    {
        var sel = ViewModel.SelectedElement;
        if (sel == null)
        {
            TextPropertiesPanel.Visibility = Visibility.Collapsed;
            RectanglePropertiesPanel.Visibility = Visibility.Collapsed;
            LinePropertiesPanel.Visibility = Visibility.Collapsed;
            BarcodePropertiesPanel.Visibility = Visibility.Collapsed;
            return;
        }

        bool isStaticText = sel.Type == "StaticText";
        bool isDynamicText = sel.Type == "DynamicText";
        bool isRect = sel.Type == "Rectangle";
        bool isLine = sel.Type == "Line";
        bool isBarcode = sel.Type == "Barcode";

        TextPropertiesPanel.Visibility = (isStaticText || isDynamicText) ? Visibility.Visible : Visibility.Collapsed;
        StaticTextInput.Visibility = isStaticText ? Visibility.Visible : Visibility.Collapsed;
        DynamicFieldCombo.Visibility = isDynamicText ? Visibility.Visible : Visibility.Collapsed;

        RectanglePropertiesPanel.Visibility = isRect ? Visibility.Visible : Visibility.Collapsed;
        LinePropertiesPanel.Visibility = isLine ? Visibility.Visible : Visibility.Collapsed;
        BarcodePropertiesPanel.Visibility = isBarcode ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnElementGeometryChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (ViewModel.SelectedElement != null)
        {
            foreach (var child in ElementsCanvas.Children)
            {
                if (child is Grid wrapper && wrapper.Tag == ViewModel.SelectedElement)
                {
                    Canvas.SetLeft(wrapper, ViewModel.SelectedElement.X);
                    Canvas.SetTop(wrapper, ViewModel.SelectedElement.Y);
                    wrapper.Width = ViewModel.SelectedElement.Width;
                    wrapper.Height = ViewModel.SelectedElement.Height;
                    if (wrapper.Children.Count > 0 && wrapper.Children[0] is TemplateElementView view)
                    {
                        view.RenderElement();
                    }
                    break;
                }
            }
            UpdateAdorner();
            ViewModel.IsDirty = true;
        }
    }

    private void OnElementContentChanged(object sender, RoutedEventArgs e)
    {
        RefreshSelectedView();
    }

    private void OnElementContentChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedView();
    }

    private void OnElementContentChanged(object sender, TextChangedEventArgs args)
    {
        RefreshSelectedView();
    }

    private void OnElementContentChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        RefreshSelectedView();
    }

    private void RefreshSelectedView()
    {
        if (ViewModel.SelectedElement != null)
        {
            foreach (var child in ElementsCanvas.Children)
            {
                if (child is Grid wrapper && wrapper.Tag == ViewModel.SelectedElement)
                {
                    if (wrapper.Children.Count > 0 && wrapper.Children[0] is TemplateElementView view)
                    {
                        view.SampleText = ViewModel.ResolveSampleText(ViewModel.SelectedElement);
                        view.RenderElement();
                    }
                    break;
                }
            }
            ViewModel.IsDirty = true;
        }
    }

    private async void OnFrontSideClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.SwitchSideAsync("Front");
    }

    private async void OnBackSideClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.SwitchSideAsync("Back");
    }

    #region Fit to Screen & Zoom Handlers

    private void FitToScreen()
    {
        if (WorkbenchScrollViewer == null) return;
        double actualW = WorkbenchScrollViewer.ActualWidth;
        double actualH = WorkbenchScrollViewer.ActualHeight;

        if (actualW > 100 && actualH > 100 && ViewModel.CanvasWidth > 0 && ViewModel.CanvasHeight > 0)
        {
            // Leave 70px horizontal and 100px vertical margins so the card fits completely with breathing room
            double availW = Math.Max(100, actualW - 70);
            double availH = Math.Max(100, actualH - 100);

            double scaleX = availW / ViewModel.CanvasWidth;
            double scaleY = availH / ViewModel.CanvasHeight;
            double fitZoom = Math.Min(scaleX, scaleY);

            ViewModel.Zoom = Math.Round(Math.Clamp(fitZoom, 0.20, 2.0), 2);
        }
    }

    private void OnZoomFitClicked(object sender, RoutedEventArgs e)
    {
        _isAutoFit = true;
        FitToScreen();
    }

    private void OnZoomInClicked(object sender, RoutedEventArgs e)
    {
        _isAutoFit = false;
        if (ViewModel.Zoom < 1.8)
        {
            ViewModel.Zoom = Math.Round(ViewModel.Zoom + 0.05, 2);
        }
    }

    private void OnZoomOutClicked(object sender, RoutedEventArgs e)
    {
        _isAutoFit = false;
        if (ViewModel.Zoom > 0.25)
        {
            ViewModel.Zoom = Math.Round(ViewModel.Zoom - 0.05, 2);
        }
    }

    private void WorkbenchScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isAutoFit)
        {
            FitToScreen();
        }
    }

    private void Workbench_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var ctrlState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
        if (ctrlState.HasFlag(CoreVirtualKeyStates.Down))
        {
            _isAutoFit = false;
            var delta = e.GetCurrentPoint(WorkbenchScrollViewer).Properties.MouseWheelDelta;
            if (delta > 0 && ViewModel.Zoom < 2.0)
            {
                ViewModel.Zoom = Math.Round(ViewModel.Zoom + 0.05, 2);
            }
            else if (delta < 0 && ViewModel.Zoom > 0.25)
            {
                ViewModel.Zoom = Math.Round(ViewModel.Zoom - 0.05, 2);
            }
            e.Handled = true;
        }
    }

    private void OnToggleToolsClicked(object sender, RoutedEventArgs e)
    {
        ColTools.Width = ColTools.Width.Value > 0 ? new GridLength(0) : new GridLength(220);
        if (_isAutoFit) FitToScreen();
    }

    private void OnToggleInspectorClicked(object sender, RoutedEventArgs e)
    {
        ColInspector.Width = ColInspector.Width.Value > 0 ? new GridLength(0) : new GridLength(320);
        if (_isAutoFit) FitToScreen();
    }

    #endregion

    private void OnInsertDynamicFieldClicked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string key)
        {
            ViewModel.AddDynamicFieldCommand.Execute(key);
        }
    }

    private void Workbench_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.DeselectElement();
    }

    private void CardBackground_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.DeselectElement();
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var sel = ViewModel.SelectedElement;

        // Undo / Redo Shortcuts
        var ctrlState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
        bool isCtrlDown = ctrlState.HasFlag(CoreVirtualKeyStates.Down);

        if (isCtrlDown && e.Key == VirtualKey.Z)
        {
            if (ViewModel.CanUndo) ViewModel.Undo();
            e.Handled = true;
            return;
        }

        if (isCtrlDown && e.Key == VirtualKey.Y)
        {
            if (ViewModel.CanRedo) ViewModel.Redo();
            e.Handled = true;
            return;
        }

        if (sel == null) return;

        // Delete shortcut
        if (e.Key == VirtualKey.Delete && !sel.Locked)
        {
            ViewModel.DeleteSelectedElementCommand.Execute(null);
            e.Handled = true;
            return;
        }

        // Arrow keys nudge (respecting lock)
        if (!sel.Locked)
        {
            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            double step = shiftState.HasFlag(CoreVirtualKeyStates.Down) ? 5.0 : 1.0;

            switch (e.Key)
            {
                case VirtualKey.Left:
                    sel.X = Math.Max(0, sel.X - step);
                    OnElementGeometryChanged(null!, null!);
                    e.Handled = true;
                    break;
                case VirtualKey.Right:
                    sel.X = Math.Min(ViewModel.CanvasWidth - sel.Width, sel.X + step);
                    OnElementGeometryChanged(null!, null!);
                    e.Handled = true;
                    break;
                case VirtualKey.Up:
                    sel.Y = Math.Max(0, sel.Y - step);
                    OnElementGeometryChanged(null!, null!);
                    e.Handled = true;
                    break;
                case VirtualKey.Down:
                    sel.Y = Math.Min(ViewModel.CanvasHeight - sel.Height, sel.Y + step);
                    OnElementGeometryChanged(null!, null!);
                    e.Handled = true;
                    break;
            }
        }
    }

    public string FormatZoom(double zoom) => $"{(int)Math.Round(zoom * 100)}%";
    public double GetScaledWidth(double width, double zoom) => width * zoom;
    public double GetScaledHeight(double height, double zoom) => height * zoom;

    public Stretch GetImageStretch(string fit) => fit switch
    {
        "Stretch" => Stretch.Fill,
        "Contain" => Stretch.Uniform,
        _ => Stretch.UniformToFill
    };

    private static SolidColorBrush ParseHexBrush(string hex)
    {
        string clean = hex.Trim().TrimStart('#');
        byte r = 255, g = 255, b = 255;
        if (clean.Length == 6)
        {
            r = Convert.ToByte(clean.Substring(0, 2), 16);
            g = Convert.ToByte(clean.Substring(2, 2), 16);
            b = Convert.ToByte(clean.Substring(4, 2), 16);
        }
        return new SolidColorBrush(Color.FromArgb(255, r, g, b));
    }
}
