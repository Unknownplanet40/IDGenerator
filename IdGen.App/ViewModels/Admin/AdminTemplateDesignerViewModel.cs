using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IdGen.Core.Models;
using IdGen.Core.Models.Template;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace IdGen.App.ViewModels.Admin;

public partial class AdminTemplateDesignerViewModel : ObservableObject
{
    private readonly ITemplateService _templateService;
    private readonly ISettingsService _settingsService;
    private readonly IAdminService _adminService;
    private readonly ITemplateAssetRepository _templateAssetRepository;

    // Undo / Redo Stacks
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();

    // Template State
    [ObservableProperty] private string _orientation = "Portrait";
    [ObservableProperty] private string _activeSide = "Front"; // "Front" or "Back"
    [ObservableProperty] private double _canvasWidth = TemplateConstants.StandardPortraitWidth;
    [ObservableProperty] private double _canvasHeight = TemplateConstants.StandardPortraitHeight;
    [ObservableProperty] private double _zoom = 0.65;

    public TemplateSideDefinition FrontSide { get; private set; } = new();
    public TemplateSideDefinition BackSide { get; private set; } = new();

    // Observable elements for current view
    public ObservableCollection<TemplateElementDefinition> CurrentElements { get; } = new();

    // Selected Element for Inspector
    [ObservableProperty] private TemplateElementDefinition? _selectedElement;
    [ObservableProperty] private bool _hasSelectedElement;
    [ObservableProperty] private bool _isSelectedElementLocked;

    // Current Background for Inspector
    [ObservableProperty] private string _backgroundColor = "#FFFFFF";
    [ObservableProperty] private string _backgroundFit = "Cover";
    [ObservableProperty] private int? _backgroundAssetId;
    [ObservableProperty] private BitmapImage? _backgroundImage;
    [ObservableProperty] private bool _hasBackgroundImage;
    [ObservableProperty] private string _backgroundDpiWarning = string.Empty;
    [ObservableProperty] private bool _hasBackgroundDpiWarning;

    // School Logo for preview
    [ObservableProperty] private BitmapImage? _schoolLogoImage;

    // State
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private bool _canUndo;
    [ObservableProperty] private bool _canRedo;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _hasStatusMessage;
    [ObservableProperty] private bool _isStatusError;

    // Sample data mapping for dynamic fields preview
    public Dictionary<string, string> SampleData { get; } = new(StringComparer.OrdinalIgnoreCase);

    // Whitelist options for Inspector
    public ObservableCollection<string> WhitelistedFonts { get; } = new(TemplateConstants.WhitelistedFonts);
    public ObservableCollection<string> WhitelistedDynamicFields { get; } = new(TemplateConstants.WhitelistedDynamicFields);
    public ObservableCollection<string> BackgroundFitModes { get; } = new(TemplateConstants.BackgroundFitModes);
    public ObservableCollection<string> TextWeights { get; } = new() { "Normal", "SemiBold", "Bold" };
    public ObservableCollection<string> TextAlignments { get; } = new() { "Left", "Center", "Right" };

    public AdminTemplateDesignerViewModel(
        ITemplateService templateService,
        ISettingsService settingsService,
        IAdminService adminService,
        ITemplateAssetRepository templateAssetRepository)
    {
        _templateService = templateService;
        _settingsService = settingsService;
        _adminService = adminService;
        _templateAssetRepository = templateAssetRepository;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            await LoadSampleDataAsync();
            await LoadSchoolLogoAsync();

            var template = await _templateService.GetTemplateAsync();
            Orientation = template.Orientation;
            UpdateCanvasDimensions();

            FrontSide = TemplateValidator.ParseOrDefault(template.FrontJson, isFront: true);
            BackSide = TemplateValidator.ParseOrDefault(template.BackJson, isFront: false);

            _undoStack.Clear();
            _redoStack.Clear();
            UpdateUndoRedoState();

            await SwitchSideAsync("Front");
            IsDirty = false;
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to initialize designer: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSampleDataAsync()
    {
        SampleData.Clear();

        // 1. Settings
        var (school, campus, principal, title, sem, sy) = await _settingsService.GetAllSettingsAsync();
        SampleData["school_name"] = string.IsNullOrWhiteSpace(school) ? "Apex Institute of Science & Technology" : school;
        SampleData["campus_name"] = string.IsNullOrWhiteSpace(campus) ? "Metropolitan Campus" : campus;
        SampleData["principal_name"] = string.IsNullOrWhiteSpace(principal) ? "Dr. Eleanor M. Vance" : principal;
        SampleData["principal_title"] = string.IsNullOrWhiteSpace(title) ? "University Registrar" : title;
        SampleData["semester"] = TemplateConstants.FormatSemester(string.IsNullOrWhiteSpace(sem) ? "First" : sem);
        SampleData["school_year"] = string.IsNullOrWhiteSpace(sy) ? "A.Y. 2026-2027" : $"A.Y. {sy}";

        // 2. Section
        var sections = await _adminService.GetAllSectionsAsync();
        var firstSec = sections.FirstOrDefault();
        SampleData["course"] = firstSec?.Course ?? "BS Information Technology";
        SampleData["year_level"] = firstSec?.YearLevel ?? "3rd Year";
        SampleData["section_name"] = firstSec?.SectionName ?? "Section A";
        SampleData["adviser"] = firstSec?.Adviser ?? "Prof. Michael Chang";

        // 3. Student sample details
        SampleData["student_no"] = "2024-10088";
        SampleData["first_name"] = "Alex";
        SampleData["last_name"] = "Mercer";
        SampleData["middle_name"] = "Vance";
        SampleData["full_name"] = "MERCER, ALEX V.";
        SampleData["gender"] = "Male";
        SampleData["birth_date"] = "October 24, 2004";
        SampleData["age"] = "20";
        SampleData["blood_type"] = "O+";
        SampleData["address"] = "104 Horizon Blvd, Metro City";
        SampleData["guardian_name"] = "Morgan Mercer";
        SampleData["guardian_phone"] = "09171234567";
    }

    private async Task LoadSchoolLogoAsync()
    {
        var logoAsset = await _templateAssetRepository.GetActiveAssetByKindAsync("SchoolLogo");
        if (logoAsset != null && logoAsset.Data.Length > 0)
        {
            SchoolLogoImage = await CreateBitmapFromBytesAsync(logoAsset.Data);
        }
        else
        {
            SchoolLogoImage = null;
        }
    }

    public string ResolveSampleText(TemplateElementDefinition el)
    {
        if (el.Type == "StaticText")
        {
            return el.StaticText ?? string.Empty;
        }

        if (el.Type == "DynamicText" && !string.IsNullOrWhiteSpace(el.FieldKey))
        {
            if (SampleData.TryGetValue(el.FieldKey, out var val))
            {
                if (string.Equals(el.FieldKey, "semester", StringComparison.OrdinalIgnoreCase))
                {
                    return TemplateConstants.FormatSemester(val);
                }
                return val;
            }
            return $"[{el.FieldKey}]";
        }

        return string.Empty;
    }

    private void UpdateCanvasDimensions()
    {
        if (Orientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase))
        {
            CanvasWidth = TemplateConstants.StandardLandscapeWidth;
            CanvasHeight = TemplateConstants.StandardLandscapeHeight;
        }
        else
        {
            CanvasWidth = TemplateConstants.StandardPortraitWidth;
            CanvasHeight = TemplateConstants.StandardPortraitHeight;
        }
    }

    [RelayCommand]
    public async Task SwitchSideAsync(string side)
    {
        ActiveSide = side;
        SelectedElement = null;
        HasSelectedElement = false;

        CurrentElements.Clear();
        var currentSideDef = ActiveSide == "Front" ? FrontSide : BackSide;

        foreach (var el in currentSideDef.Elements)
        {
            CurrentElements.Add(el);
        }

        BackgroundColor = currentSideDef.Background.Color;
        BackgroundFit = currentSideDef.Background.Fit;
        BackgroundAssetId = currentSideDef.Background.AssetId;

        await LoadBackgroundImageAsync(BackgroundAssetId);
    }

    [RelayCommand]
    public void ToggleOrientation()
    {
        TakeSnapshot();
        Orientation = Orientation == "Portrait" ? "Landscape" : "Portrait";
        UpdateCanvasDimensions();
        IsDirty = true;
    }

    public void SelectElement(TemplateElementDefinition? element)
    {
        SelectedElement = element;
        HasSelectedElement = element != null;
        IsSelectedElementLocked = element?.Locked ?? false;
    }

    [RelayCommand]
    public void DeselectElement()
    {
        SelectedElement = null;
        HasSelectedElement = false;
        IsSelectedElementLocked = false;
    }

    // --- ELEMENT CREATION COMMANDS ---

    [RelayCommand]
    public void AddRectangle()
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "Rectangle",
            X = 50,
            Y = 50,
            Width = 250,
            Height = 60,
            FillColor = "#1E3A5F",
            FillOpacity = 100,
            BorderColor = "#000000",
            BorderWidth = 0,
            CornerRadius = 8
        };
        AddElementToActiveSide(el);
    }

    [RelayCommand]
    public void AddLine()
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "Line",
            X = 50,
            Y = 50,
            Width = 300,
            Height = 2,
            LineColor = "#000000",
            Thickness = 2
        };
        AddElementToActiveSide(el);
    }

    [RelayCommand]
    public void AddStaticText()
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "StaticText",
            X = 50,
            Y = 50,
            Width = 250,
            Height = 35,
            StaticText = "Sample Label",
            FontFamily = "Segoe UI",
            FontSize = 14,
            FontWeight = "Normal",
            TextColor = "#0F172A",
            TextAlign = "Left"
        };
        AddElementToActiveSide(el);
    }

    [RelayCommand]
    public void AddDynamicField(string fieldKey)
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "DynamicText",
            X = 50,
            Y = 50,
            Width = 300,
            Height = 35,
            FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? "full_name" : fieldKey,
            FontFamily = "Segoe UI",
            FontSize = 14,
            FontWeight = "SemiBold",
            TextColor = "#0F172A",
            TextAlign = "Left"
        };
        AddElementToActiveSide(el);
    }

    [RelayCommand]
    public void AddBarcode()
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "Barcode",
            X = 50,
            Y = 50,
            Width = 400,
            Height = 90,
            BarcodeColor = "#000000",
            ShowHumanReadable = true,
            IncludeQuietZone = true
        };
        AddElementToActiveSide(el);
    }

    [RelayCommand]
    public void AddLogo()
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "Logo",
            X = 50,
            Y = 50,
            Width = 90,
            Height = 90
        };
        AddElementToActiveSide(el);
    }

    [RelayCommand]
    public void AddPhoto()
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "Photo",
            X = 50,
            Y = 50,
            Width = 200,
            Height = 200
        };
        AddElementToActiveSide(el);
    }

    [RelayCommand]
    public void AddSignature()
    {
        TakeSnapshot();
        var el = new TemplateElementDefinition
        {
            Type = "Signature",
            X = 50,
            Y = 50,
            Width = 260,
            Height = 90
        };
        AddElementToActiveSide(el);
    }

    private void AddElementToActiveSide(TemplateElementDefinition el)
    {
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        if (sideDef.Elements.Count >= TemplateConstants.MaxElementsPerSide)
        {
            ShowStatus($"Maximum of {TemplateConstants.MaxElementsPerSide} elements reached for this side.", isError: true);
            return;
        }

        sideDef.Elements.Add(el);
        CurrentElements.Add(el);
        SelectElement(el);
        IsDirty = true;
    }

    // --- Z-ORDER & LAYERING COMMANDS ---

    [RelayCommand]
    public void BringForward()
    {
        if (SelectedElement == null) return;
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        int idx = sideDef.Elements.IndexOf(SelectedElement);
        if (idx >= 0 && idx < sideDef.Elements.Count - 1)
        {
            TakeSnapshot();
            sideDef.Elements.RemoveAt(idx);
            sideDef.Elements.Insert(idx + 1, SelectedElement);

            CurrentElements.RemoveAt(idx);
            CurrentElements.Insert(idx + 1, SelectedElement);
            IsDirty = true;
        }
    }

    [RelayCommand]
    public void SendBackward()
    {
        if (SelectedElement == null) return;
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        int idx = sideDef.Elements.IndexOf(SelectedElement);
        if (idx > 0)
        {
            TakeSnapshot();
            sideDef.Elements.RemoveAt(idx);
            sideDef.Elements.Insert(idx - 1, SelectedElement);

            CurrentElements.RemoveAt(idx);
            CurrentElements.Insert(idx - 1, SelectedElement);
            IsDirty = true;
        }
    }

    [RelayCommand]
    public void BringToFront()
    {
        if (SelectedElement == null) return;
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        int idx = sideDef.Elements.IndexOf(SelectedElement);
        if (idx >= 0 && idx < sideDef.Elements.Count - 1)
        {
            TakeSnapshot();
            sideDef.Elements.RemoveAt(idx);
            sideDef.Elements.Add(SelectedElement);

            CurrentElements.RemoveAt(idx);
            CurrentElements.Add(SelectedElement);
            IsDirty = true;
        }
    }

    [RelayCommand]
    public void SendToBack()
    {
        if (SelectedElement == null) return;
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        int idx = sideDef.Elements.IndexOf(SelectedElement);
        if (idx > 0)
        {
            TakeSnapshot();
            sideDef.Elements.RemoveAt(idx);
            sideDef.Elements.Insert(0, SelectedElement);

            CurrentElements.RemoveAt(idx);
            CurrentElements.Insert(0, SelectedElement);
            IsDirty = true;
        }
    }

    [RelayCommand]
    public void ToggleLock()
    {
        if (SelectedElement == null) return;
        TakeSnapshot();
        SelectedElement.Locked = !SelectedElement.Locked;
        IsSelectedElementLocked = SelectedElement.Locked;
        IsDirty = true;
    }

    [RelayCommand]
    public void DeleteSelectedElement()
    {
        if (SelectedElement == null) return;
        TakeSnapshot();
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        sideDef.Elements.Remove(SelectedElement);
        CurrentElements.Remove(SelectedElement);
        DeselectElement();
        IsDirty = true;
    }

    // --- BACKGROUND CONTROLS ---

    [RelayCommand]
    public async Task UploadBackgroundAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            if (App.MainWindowInstance != null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            using var stream = await file.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.AsStreamForRead().CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            // Validate format and size (max 5MB)
            if (!InputValidators.ValidateImageFile(fileBytes, 5 * 1024 * 1024, out string? mimeType, out string? err))
            {
                ShowStatus(err ?? "Invalid image file format.", isError: true);
                return;
            }

            // Check dimensions & DPI check (max 4000px per side, 300 DPI warning if < 638x1011)
            using var ras = new InMemoryRandomAccessStream();
            await ras.WriteAsync(fileBytes.AsBuffer());
            ras.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(ras);

            uint pixelWidth = decoder.PixelWidth;
            uint pixelHeight = decoder.PixelHeight;

            if (pixelWidth > 4000 || pixelHeight > 4000)
            {
                ShowStatus("Background image dimensions exceed 4000 px limit.", isError: true);
                return;
            }

            TakeSnapshot();

            // Save to TemplateAssets
            var asset = new TemplateAsset
            {
                Kind = $"Background_{ActiveSide}",
                MimeType = mimeType!,
                FileSize = fileBytes.Length,
                Data = fileBytes,
                CreatedAt = DateTime.UtcNow
            };
            int assetId = await _templateAssetRepository.SaveAssetAsync(asset);

            var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
            sideDef.Background.AssetId = assetId;
            BackgroundAssetId = assetId;

            BackgroundImage = await CreateBitmapFromBytesAsync(fileBytes);
            HasBackgroundImage = true;

            // DPI Check: CR80 portrait at 300 DPI is ~638 x 1011 px
            double reqW = Orientation == "Portrait" ? 638 : 1011;
            double reqH = Orientation == "Portrait" ? 1011 : 638;
            if (pixelWidth < reqW || pixelHeight < reqH)
            {
                BackgroundDpiWarning = $"Notice: Image resolution ({pixelWidth}x{pixelHeight}) is below 300 DPI print standard ({reqW}x{reqH}). Output may appear slightly soft when printed.";
                HasBackgroundDpiWarning = true;
            }
            else
            {
                BackgroundDpiWarning = string.Empty;
                HasBackgroundDpiWarning = false;
            }

            IsDirty = true;
            ShowStatus("Background image uploaded successfully.", isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to upload background: {ex.Message}", isError: true);
        }
    }

    [RelayCommand]
    public async Task RemoveBackgroundAsync()
    {
        TakeSnapshot();
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        if (sideDef.Background.AssetId.HasValue)
        {
            await _templateAssetRepository.DeleteAsync(sideDef.Background.AssetId.Value);
        }

        sideDef.Background.AssetId = null;
        BackgroundAssetId = null;
        BackgroundImage = null;
        HasBackgroundImage = false;
        HasBackgroundDpiWarning = false;
        BackgroundDpiWarning = string.Empty;
        IsDirty = true;
        ShowStatus("Background image removed.", isError: false);
    }

    partial void OnBackgroundColorChanged(string value)
    {
        if (TemplateValidator.IsValidHexColor(value))
        {
            var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
            sideDef.Background.Color = value;
            IsDirty = true;
        }
    }

    partial void OnBackgroundFitChanged(string value)
    {
        var sideDef = ActiveSide == "Front" ? FrontSide : BackSide;
        sideDef.Background.Fit = value;
        IsDirty = true;
    }

    private async Task LoadBackgroundImageAsync(int? assetId)
    {
        if (!assetId.HasValue)
        {
            BackgroundImage = null;
            HasBackgroundImage = false;
            HasBackgroundDpiWarning = false;
            return;
        }

        var asset = await _templateAssetRepository.GetByIdAsync(assetId.Value);
        if (asset != null && asset.Data.Length > 0)
        {
            BackgroundImage = await CreateBitmapFromBytesAsync(asset.Data);
            HasBackgroundImage = true;
        }
        else
        {
            BackgroundImage = null;
            HasBackgroundImage = false;
        }
    }

    // --- RESET TO REFERENCE LAYOUT ---

    [RelayCommand]
    public void ResetToReferenceLayout()
    {
        TakeSnapshot();
        if (ActiveSide == "Front")
        {
            FrontSide = _templateService.GetReferenceFront();
        }
        else
        {
            BackSide = _templateService.GetReferenceBack();
        }

        _ = SwitchSideAsync(ActiveSide);
        IsDirty = true;
        ShowStatus($"Reset {ActiveSide} to standard institutional reference layout.", isError: false);
    }

    // --- UNDO / REDO ---

    public void TakeSnapshot()
    {
        string json = SerializeState();
        _undoStack.Push(json);
        _redoStack.Clear();
        UpdateUndoRedoState();
    }

    [RelayCommand]
    public void Undo()
    {
        if (_undoStack.Count == 0) return;

        string current = SerializeState();
        _redoStack.Push(current);

        string prev = _undoStack.Pop();
        RestoreState(prev);
        UpdateUndoRedoState();
    }

    [RelayCommand]
    public void Redo()
    {
        if (_redoStack.Count == 0) return;

        string current = SerializeState();
        _undoStack.Push(current);

        string next = _redoStack.Pop();
        RestoreState(next);
        UpdateUndoRedoState();
    }

    private void UpdateUndoRedoState()
    {
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }

    private string SerializeState()
    {
        var state = new
        {
            Orientation,
            FrontJson = JsonSerializer.Serialize(FrontSide),
            BackJson = JsonSerializer.Serialize(BackSide)
        };
        return JsonSerializer.Serialize(state);
    }

    private void RestoreState(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Orientation = root.GetProperty("Orientation").GetString() ?? "Portrait";
            UpdateCanvasDimensions();

            string frontJson = root.GetProperty("FrontJson").GetString() ?? "";
            string backJson = root.GetProperty("BackJson").GetString() ?? "";

            FrontSide = TemplateValidator.ParseOrDefault(frontJson, isFront: true);
            BackSide = TemplateValidator.ParseOrDefault(backJson, isFront: false);

            _ = SwitchSideAsync(ActiveSide);
            IsDirty = true;
        }
        catch { }
    }

    // --- SAVE ---

    [RelayCommand]
    public async Task SaveTemplateAsync()
    {
        IsLoading = true;
        try
        {
            await _templateService.SaveTemplateAsync(Orientation, FrontSide, BackSide);
            IsDirty = false;
            ShowStatus("ID Card Template saved successfully.", isError: false);
        }
        catch (Exception ex)
        {
            ShowStatus($"Failed to save template: {ex.Message}", isError: true);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsStatusError = isError;
        HasStatusMessage = true;
    }

    private static async Task<BitmapImage?> CreateBitmapFromBytesAsync(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        using var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(bytes.AsBuffer());
        stream.Seek(0);

        var bmp = new BitmapImage();
        await bmp.SetSourceAsync(stream);
        return bmp;
    }
}
