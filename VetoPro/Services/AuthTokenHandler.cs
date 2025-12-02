using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceProvider
using VetoPro.Contracts.DTOs.Auth; // For AuthResponseDto

namespace VetoPro.Services;

/// <summary>
/// A DelegatingHandler that automatically attaches the Bearer token to outgoing requests
/// and handles 401 Unauthorized responses by attempting to refresh the token.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly ISecureStorageService _storageService;
    private readonly IServiceProvider _serviceProvider;

    // We use IServiceProvider to lazily resolve AuthService to avoid circular dependencies
    // (HttpClient needs this handler, this handler needs AuthService, AuthService needs HttpClient)
    public AuthTokenHandler(ISecureStorageService storageService, IServiceProvider serviceProvider)
    {
        _storageService = storageService;
        _serviceProvider = serviceProvider;
    }

    // This is a flag to prevent infinite loops if the refresh request itself fails
    private static bool _isRefreshing = false;
    private static readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Get the access token from secure storage
        var accessToken = await _storageService.GetAsync("access_token");

        // 2. Add it to the request header if it exists
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        // 3. Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // 4. Check if the response is 401 Unauthorized (token expired)
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            // Use a semaphore to ensure only one thread tries to refresh the token
            await _refreshSemaphore.WaitAsync(cancellationToken);
            if (_isRefreshing)
            {
                // Another thread is already refreshing, wait for it and retry
                _refreshSemaphore.Release();
                return await base.SendAsync(request, cancellationToken);
            }

            _isRefreshing = true;
            try
            {
                // 5. Try to refresh the token
                var newAuthResponse = await AttemptTokenRefresh();

                if (newAuthResponse != null)
                {
                    // 6. Refresh succeeded: Update the request with the new token
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAuthResponse.AccessToken);
                    
                    // 7. Retry the original request
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    // 8. Refresh failed: Log the user out (by clearing tokens)
                    _storageService.Remove("access_token");
                    _storageService.Remove("refresh_token");
                    // The request will fail, and the UI will see the 401 and redirect to login
                }
            }
            finally
            {
                _isRefreshing = false;
                _refreshSemaphore.Release();
            }
        }

        return response;
    }

    private async Task<AuthResponseDto?> AttemptTokenRefresh()
    {
        // Get the refresh token
        var refreshToken = await _storageService.GetAsync("refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
        {
            return null; // No refresh token, can't refresh
        }

        // We must get a *new* scope and a *new* instance of AuthService
        // to avoid using the same (circular) HttpClient.
        using (var scope = _serviceProvider.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
            var authResponse = await authService.RefreshTokenAsync(refreshToken);

            if (authResponse != null)
            {
                // Save the new tokens
                await _storageService.SetAsync("access_token", authResponse.AccessToken);
                await _storageService.SetAsync("refresh_token", authResponse.RefreshToken);
                return authResponse;
            }
            else
            {
                return null;
            }
        }
    }
}