using System.Threading.Tasks;

namespace VetoPro.Services;

/// <summary>
/// Defines a contract for securely storing sensitive data,
/// such as authentication tokens, across application launches.
/// This abstraction allows for different implementations on
/// Desktop (ProtectedData), iOS (KeyChain), and Android (KeyStore).
/// </summary>
public interface ISecureStorageService
{
    /// <summary>
    /// Securely stores a value associated with a key.
    /// </summary>
    /// <param name="key">The key to associate the value with.</param>
    /// <param name="value">The string value to store.</param>
    Task SetAsync(string key, string value);

    /// <summary>
    /// Retrieves a securely stored value for a given key.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>The stored value, or null if the key doesn't exist.</returns>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Removes a key and its associated value from secure storage.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>True if the item was successfully removed.</returns>
    bool Remove(string key);
}