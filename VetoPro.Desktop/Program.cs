using System;
using Akavache;
using Akavache.EncryptedSqlite3;
using Avalonia;
using Akavache.SystemTextJson;

namespace VetoPro.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(builder =>
                builder.WithApplicationName("VetoPro")
                    .WithEncryptedSqliteProvider()); // REQUIRED: Explicitly initialize SQLite provider
        
        var dummy = typeof(Akavache.EncryptedSqlite3.EncryptedSqliteBlobCache);
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}