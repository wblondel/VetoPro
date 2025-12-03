using Akavache;
using System.Reactive.Linq; // Required for Akavache's async operations
using System.Threading.Tasks;
using System; // For Exception
using System.Collections.Generic; // For KeyNotFoundException

namespace VetoPro.Services;

/// <summary>
/// Cross-platform implementation of ISecureStorageService using Akavache.EncryptedStorage.
/// Handles secure token storage on Android, iOS, Desktop, and Browser.
/// </summary>
public class SecureStorageService : ISecureStorageService
{
    public async Task SetAsync(string key, string value)
    {
        try
        {
            await CacheDatabase.Secure.InsertObject(key, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to Akavache.Secure: {ex.Message}");
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await CacheDatabase.Secure.GetObject<string>(key);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from Akavache.Secure: {ex.Message}");
            return null;
        }
    }

    public bool Remove(string key)
    {
        try
        {
            // We must block here as the interface is synchronous.
            CacheDatabase.Secure.InvalidateObject<string>(key).Wait();
            return true;
        }
        catch (Exception ex)
        {
            // Log this exception
            Console.WriteLine($"Error removing from Akavache.Secure: {ex.Message}");
            return false;
        }
    }
}