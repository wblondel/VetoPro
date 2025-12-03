using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using VetoPro;
using Akavache;
using Akavache.EncryptedSqlite3;
using Akavache.SystemTextJson;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        var dummy = typeof(Akavache.EncryptedSqlite3.EncryptedSqliteBlobCache);
        
        Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(akavacheBuilder =>
                akavacheBuilder.WithApplicationName("VetoPro")
                    .WithEncryptedSqliteProvider()); //
        
        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}