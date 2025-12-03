using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using Akavache;
using Akavache.EncryptedSqlite3;
using Akavache.SystemTextJson;

namespace VetoPro.Android;

[Activity(
    Label = "VetoPro.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        var dummy = typeof(Akavache.EncryptedSqlite3.EncryptedSqliteBlobCache);
        
        Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(builder =>
                builder.WithApplicationName("VetoPro")
                    .WithEncryptedSqliteProvider()); //
        
        base.OnCreate(savedInstanceState);
    }
    
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}