# WinUI 3 Native Windows App Architecture & Blueprint

A standardized, production-ready blueprint for building modern native Windows applications using **WinUI 3** and the **Windows App SDK**. Designed to match the exact aesthetic, performance, and behavior of official Windows 11 system apps like **Windows Settings** and the **Microsoft Store**.

---

## 1. Core Tech Stack (Default Standard)

| Layer | Technology | Role & Justification |
| :--- | :--- | :--- |
| **Framework** | **WinUI 3 + Windows App SDK 1.6+** | Native UI layer decoupling Windows UI from the OS version. Direct access to Windows 11 Fluent design primitives. |
| **Runtime** | **.NET 8.0 / .NET 9.0 (LTS/STS)** | C# 12/13 with AOT / trimming support, high performance, native Windows interop. |
| **Architecture** | **MVVM (Model-View-ViewModel)** | Industry standard for XAML apps. Clear separation of concerns, testability, and UI data-binding. |
| **MVVM Toolkit** | **CommunityToolkit.Mvvm (8.3+)** | Source-generator based MVVM library (high performance, no reflection overhead). `[ObservableProperty]`, `[RelayCommand]`. |
| **UI Toolkit** | **CommunityToolkit.WinUI.Controls (8.1+)** | Supplies official `SettingsCard`, `SettingsExpander`, segmented controls, and layout helpers identical to Windows Settings. |
| **Dependency Injection**| **Microsoft.Extensions.DependencyInjection** | Standard .NET service provider for IoC, viewmodel resolution, and singleton service lifecycles. |
| **Navigation** | **WinUI NavigationView + Frame Navigation Service** | Standard shell pattern with left-rail navigation, fluid transitions, and page state preservation. |
| **Packaging** | **MSIX (Packaged) or Self-Contained Unpackaged** | Packaged for store/sandboxed distribution; Unpackaged for portable, classic desktop tools with full Win32 API access. |

---

## 2. Prerequisites & Environment Setup

Before writing any code, prepare your developer workstation:

### A. Windows OS Requirements
- **Windows 11 (Build 22621 / 22H2 or later)** is recommended for full native **Mica / Mica Alt** system materials.
- Enable **Developer Mode**:
  - Open **Windows Settings** (`Win + I`) -> **System** -> **For developers**.
  - Toggle **Developer Mode** to **On** (allows sideloading and debugging unpackaged apps).

### B. Visual Studio 2022 (v17.8 or later)
Install or update **Visual Studio 2022 Community / Professional / Enterprise** with the following workloads:
1. **.NET Desktop Development**:
   - Includes .NET 8 / 9 SDK, C# compilers, MSBuild.
2. **Individual Components**:
   - **Windows App SDK C# Templates**
   - **Windows 11 SDK (10.0.22621.0 or 10.0.26100.0)**
3. **Recommended Visual Studio Extension**:
   - **Template Studio for WinUI (C#)** by Microsoft (Download from *Extensions -> Manage Extensions*). Generates pre-architected, production-grade WinUI 3 solutions.

---

## 3. Project Structure

A clean, scalable project layout:

```text
MyNativeApp/
├── Assets/                        # App icons, splash screens, logos
│   ├── StoreLogo.png
│   ├── Square44x44Logo.png
│   └── WindowIcon.ico
├── Common/                        # Cross-cutting concerns & helpers
│   ├── Converters/                # IValueConverter implementations
│   └── Extensions/                # C# extension methods
├── Contracts/                     # Interfaces for services & navigation
│   ├── INavigationService.cs
│   ├── IPageService.cs
│   ├── IThemeSelectorService.cs
│   └── IUpdateService.cs
├── Controls/                      # Custom user controls
│   └── TitleBarControl.xaml
├── Models/                        # Data models & entity objects
│   └── AppConfig.cs
├── Services/                      # Concrete service implementations
│   ├── NavigationService.cs
│   ├── PageService.cs
│   ├── ThemeSelectorService.cs
│   └── UpdateService.cs
├── ViewModels/                    # Observable ViewModels
│   ├── BaseViewModel.cs
│   ├── ShellViewModel.cs
│   ├── MainViewModel.cs
│   └── SettingsViewModel.cs
├── Views/                         # XAML Views & Pages
│   ├── ShellPage.xaml             # App outer container (TitleBar + NavigationView)
│   ├── MainPage.xaml              # Primary dashboard page
│   └── SettingsPage.xaml          # Native SettingsCard layout
├── App.xaml                       # Application entry, styles, and DI setup
├── App.xaml.cs
├── MainWindow.xaml                # Window host (Backdrop + Content)
├── MainWindow.xaml.cs
└── Package.appxmanifest           # App capabilities, identity, and assets
```

---

## 4. How to Make It Look Like Windows Settings & MS Store

Standard WinUI apps often look empty or generic unless configured to adopt the native shell guidelines. Follow these rules:

### A. Window Backdrop: Mica / Mica Alt
Windows Settings uses **Mica Alt** (a subtle background tint layered behind tabbed/navigation layouts), while Microsoft Store uses standard **Mica**.

#### In `MainWindow.xaml.cs`:
```csharp
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace MyNativeApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 1. Set System Backdrop to MicaAlt (Settings-style) or Mica (Store-style)
        SystemBackdrop = new MicaBackdrop()
        {
            Kind = MicaKind.MicaAlt
        };

        // 2. Extend content into TitleBar for a seamless top edge
        ExtendsContentIntoTitleBar = true;
    }
}
```

---

### B. Custom Seamless Title Bar
Windows Settings and Microsoft Store integrate search boxes, back buttons, and status directly into the title bar area.

#### In `Views/ShellPage.xaml`:
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <!-- Custom Title Bar Grid -->
    <Grid x:Name="AppTitleBar"
          Grid.Row="0"
          Height="48"
          Padding="16,0,0,0">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <Image Source="ms-appx:///Assets/Square44x44Logo.targetsize-24.png"
               Width="16" Height="16" Margin="0,0,12,0" />

        <TextBlock Grid.Column="1"
                   Text="My Native App"
                   VerticalAlignment="Center"
                   Style="{StaticResource CaptionTextBlockStyle}" />

        <!-- Optional center search or custom drag region -->
        <Border Grid.Column="2" Background="Transparent" />
    </Grid>

    <!-- Main Navigation & Content Area -->
    <NavigationView x:Name="NavView"
                    Grid.Row="1"
                    PaneDisplayMode="Left"
                    IsTitleBarAreaPlacementEnabled="False"
                    IsPaneToggleButtonVisible="True"
                    OpenPaneLength="250">
        <Frame x:Name="ContentFrame" />
    </NavigationView>
</Grid>
```

#### Connect Drag Region in `MainWindow.xaml.cs`:
```csharp
public void SetTitleBarElement(UIElement titleBarElement)
{
    // Tells WinUI which XAML element handles window dragging
    SetTitleBar(titleBarElement);
}
```

---

### C. Standard Navigation Layout (Left-Rail)
Configure `NavigationView` to emulate the Windows Settings side menu:
- Set `PaneDisplayMode="Left"` (or `LeftCompact` on smaller windows).
- Place primary pages inside `NavigationView.MenuItems`.
- Place Settings in `NavigationView.FooterMenuItems` (enabled via `IsSettingsVisible="True"`).
- Use **Segoe Fluent Icons** (`\uE...` glyphs) matching standard Windows symbols:
  - System: `\uE770`
  - Apps: `\uE71D`
  - Personalization: `\uE771`
  - Accounts: `\uE77B`
  - Settings: `\uE713`

---

### D. Settings Pages Using CommunityToolkit `SettingsCard`
The signature look of Windows 11 Settings is built using rounded, bordered **Settings Cards** and **Settings Expanders**.

Install NuGet: `CommunityToolkit.WinUI.Controls.SettingsControls`

#### Example `Views/SettingsPage.xaml`:
```xml
<Page x:Class="MyNativeApp.Views.SettingsPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:labs="using:CommunityToolkit.WinUI.Controls">

    <ScrollViewer Padding="36,24,36,36">
        <StackPanel Spacing="4" MaxWidth="1000" HorizontalAlignment="Left">
            
            <!-- Page Header -->
            <TextBlock Text="Settings"
                       Style="{StaticResource TitleTextBlockStyle}"
                       Margin="0,0,0,18" />

            <!-- Group 1: Appearance -->
            <TextBlock Text="Appearance &amp; behavior"
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Margin="0,16,0,8" />

            <!-- Simple Toggle Card -->
            <labs:SettingsCard Header="App Theme"
                               Description="Select which app theme to display"
                               HeaderIcon="{labs:FontIcon Glyph=&#xE790;}">
                <ComboBox SelectedIndex="0">
                    <ComboBoxItem Content="Use system setting" />
                    <ComboBoxItem Content="Light" />
                    <ComboBoxItem Content="Dark" />
                </ComboBox>
            </labs:SettingsCard>

            <labs:SettingsCard Header="Hardware acceleration"
                               Description="Use graphics hardware to speed up rendering"
                               HeaderIcon="{labs:FontIcon Glyph=&#xE7F4;}">
                <ToggleSwitch IsOn="True" OnContent="On" OffContent="Off" />
            </labs:SettingsCard>

            <!-- Expander Card for Sub-settings -->
            <labs:SettingsExpander Header="Advanced options"
                                   Description="Diagnostic logs, cache clearing, and experimental features"
                                   HeaderIcon="{labs:FontIcon Glyph=&#xE713;}">
                <Button Content="Clear Cache" />
                
                <labs:SettingsExpander.Items>
                    <labs:SettingsCard Header="Enable verbose telemetry">
                        <ToggleSwitch IsOn="False" />
                    </labs:SettingsCard>
                    <labs:SettingsCard Header="Storage location"
                                       Description="C:\Users\AppData\Local\MyNativeApp">
                        <Button Content="Browse..." />
                    </labs:SettingsCard>
                </labs:SettingsExpander.Items>
            </labs:SettingsExpander>

            <!-- Group 2: About / Updates -->
            <TextBlock Text="About"
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Margin="0,24,0,8" />

            <labs:SettingsCard Header="My Native App"
                               Description="Version 1.0.0 (x64) • Windows App SDK 1.6"
                               HeaderIcon="{labs:FontIcon Glyph=&#xE946;}">
                <Button Content="Check for updates" Style="{StaticResource AccentButtonStyle}" />
            </labs:SettingsCard>

        </StackPanel>
    </ScrollViewer>
</Page>
```

---

### E. Native Typography & Spacing Hierarchy
Windows 11 apps follow strict font styles and rhythm:
- **Title**: `TitleTextBlockStyle` (28px Semibold)
- **Subtitle**: `SubtitleTextBlockStyle` (20px Semibold)
- **Body**: `BodyTextBlockStyle` (14px Regular)
- **Caption**: `CaptionTextBlockStyle` (12px Regular)
- **Corner Radius**: Standard controls use `CornerRadius="8"`. Small badges use `CornerRadius="4"`.
- **Card Margins**: Use `Spacing="4"` between adjacent SettingsCards to create the connected card stack look.

---

## 5. Dependency Injection & Service Setup

In `App.xaml.cs`, register all your dependencies via `IHost` or `ServiceCollection`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyNativeApp.Contracts;
using MyNativeApp.Services;
using MyNativeApp.ViewModels;
using MyNativeApp.Views;

namespace MyNativeApp;

public partial class App : Application
{
    public IHost Host { get; }

    public static T GetService<T>() where T : class
    {
        if ((Current as App)?.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} is not registered in the DI container.");
        }
        return service;
    }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Core Services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IUpdateService, UpdateService>();

                // ViewModels
                services.AddTransient<ShellViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddTransient<ShellPage>();
                services.AddTransient<MainPage>();
                services.AddTransient<SettingsPage>();
            })
            .Build();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    public static Window MainWindow { get; private set; } = null!;
}
```

---

## 6. Recommended NuGet Packages

Add these to your `.csproj` for the modern stack:

```xml
<ItemGroup>
    <!-- Windows App SDK Core -->
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.*" />
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.26100.1" />

    <!-- MVVM Toolkit -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />

    <!-- Fluent Settings & Native UI Controls -->
    <PackageReference Include="CommunityToolkit.WinUI.Controls.SettingsControls" Version="8.1.240916" />
    <PackageReference Include="CommunityToolkit.WinUI.Controls.Primitives" Version="8.1.240916" />
    <PackageReference Include="CommunityToolkit.WinUI.Animations" Version="8.1.240916" />

    <!-- Dependency Injection & Hosting -->
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
</ItemGroup>
```

---

## 7. Step-by-Step New Project Creation Workflow

1. **Launch Visual Studio 2022**.
2. Select **Create a new project**.
3. Filter by **C#**, **Windows**, and search for **"Template Studio for WinUI"**.
4. Configure Project:
   - Project Name: `MyNativeApp`
   - Project Type: **Navigation View**
   - Framework: **MVVM Toolkit**
   - System Backdrop: **Mica** (or **Mica Alt**)
5. Add Pages:
   - Add **MainPage** (Blank)
   - Add **SettingsPage** (Pre-configures theme selector & version details)
6. Run the project (`F5`) to verify the Windows 11 native frame.
