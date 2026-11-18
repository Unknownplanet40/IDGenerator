using IdGen.App.Services;
using IdGen.App.ViewModels;
using IdGen.App.ViewModels.Admin;
using IdGen.App.ViewModels.Student;
using IdGen.Core.Services;
using IdGen.Data;
using IdGen.Data.Repositories;
using IdGen.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace IdGen.App;

public partial class App : Application
{
    private Window? _window;
    public static Window? MainWindowInstance { get; private set; }

    public IServiceProvider Services { get; }
    public static IServiceProvider CurrentServices => ((App)Current).Services;

    public App()
    {
        LogCrash("1. App constructor entered");

        this.UnhandledException += (s, e) =>
        {
            LogCrash("App.UnhandledException: " + e.Message + "\n" + e.Exception?.ToString());
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            LogCrash("AppDomain.UnhandledException: " + e.ExceptionObject?.ToString());
        };

        try
        {
            LogCrash("2. Before App.InitializeComponent");
            InitializeComponent();
            LogCrash("3. After App.InitializeComponent");

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
            LogCrash("4. After ConfigureServices");

            // Run database migration on startup
            InitializeDatabase();
            LogCrash("5. After InitializeDatabase");
        }
        catch (Exception ex)
        {
            LogCrash("App constructor exception: " + ex.ToString());
            throw;
        }
    }

    public static void LogAppMessage(string msg) => LogCrash(msg);

    private static void LogCrash(string message)
    {
        try
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StudentIdGenerator");
            Directory.CreateDirectory(folder);
            File.AppendAllText(Path.Combine(folder, "crash.log"), $"[{DateTime.UtcNow:O}] {message}\n\n");
        }
        catch { }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Data & SQLite
        services.AddSingleton<ISqliteConnectionFactory>(_ => new SqliteConnectionFactory());

        // Repositories
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<ISectionRepository, SectionRepository>();
        services.AddSingleton<ISettingRepository, SettingRepository>();
        services.AddSingleton<IIdSubmissionRepository, IdSubmissionRepository>();
        services.AddSingleton<IResetRequestRepository, ResetRequestRepository>();
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        services.AddSingleton<ITemplateAssetRepository, TemplateAssetRepository>();
        services.AddSingleton<ITemplateRepository, TemplateRepository>();

        // Domain Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAdminService, AdminService>();
        services.AddSingleton<IStudentService, StudentService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ITemplateService, TemplateService>();

        // App Navigation & Session
        services.AddSingleton<IAppNavigationService, AppNavigationService>();
        services.AddSingleton<IUserSession, UserSession>();
        services.AddSingleton<CardPrintService>();

        // ViewModels
        services.AddTransient<AdminSetupViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<AdminShellViewModel>();
        services.AddTransient<AdminRegistrationsViewModel>();
        services.AddTransient<AdminSectionsViewModel>();
        services.AddTransient<AdminDefaultsViewModel>();
        services.AddTransient<AdminTemplateDesignerViewModel>();
        services.AddTransient<AdminSubmissionsViewModel>();
        services.AddTransient<AdminResetRequestsViewModel>();
        services.AddTransient<AdminUsersViewModel>();

        // Student ViewModels
        services.AddTransient<StudentShellViewModel>();
        services.AddTransient<StudentGeneratorViewModel>();
        services.AddTransient<StudentViewIdViewModel>();
        services.AddTransient<StudentResetIdViewModel>();
        services.AddTransient<StudentAccountViewModel>();
    }

    private void InitializeDatabase()
    {
        var factory = Services.GetRequiredService<ISqliteConnectionFactory>();
        using var conn = factory.CreateConnection();
        DatabaseMigrator.Migrate(conn);
        DatabaseSeeder.SeedAppDefaults(conn);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        LogCrash("6. OnLaunched entered");
        try
        {
            LogCrash("7. Creating MainWindow");
            _window = new MainWindow();
            MainWindowInstance = _window;
            LogCrash("8. Activating MainWindow");
            _window.Activate();
            LogCrash("9. MainWindow activated");
        }
        catch (Exception ex)
        {
            LogCrash("OnLaunched exception: " + ex.ToString());
            throw;
        }
    }
}
