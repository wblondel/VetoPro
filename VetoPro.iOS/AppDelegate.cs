using Akavache;
using Akavache.EncryptedSqlite3;
using Foundation;
using UIKit;
using Avalonia;
using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Media;
using Akavache.SystemTextJson;

namespace VetoPro.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var dummy = typeof(Akavache.EncryptedSqlite3.EncryptedSqliteBlobCache);

        Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(akavacheBuilder =>
                akavacheBuilder.WithApplicationName("VetoPro")
                    .WithEncryptedSqliteProvider());
        
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}