using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VetoPro.Contracts.DTOs.Auth;

namespace VetoPro.Services;

/// <summary>
/// Service to manage all authentication-related API calls.
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Attempts to log in the user via the API.
    /// </summary>
    /// <param name="loginData">The user's email and password.</param>
    /// <returns>An AuthResponseDto if successful, null otherwise.</returns>
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginData)
    {
        try
        {
            // Make a POST request to the api/auth/login endpoint
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                // Read the response (AccessToken, RefreshToken, UserInfo)
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return authResponse;
            }

            // Handle failed login (e.g., 401 Unauthorized) by returning null
            // You could also inspect the status code or read the error message
            return null;
        }
        catch (HttpRequestException ex)
        {
            // Handle network errors, API down, etc.
            Console.WriteLine($"AuthService Login Error: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Attempts to register a new user via the API.
    /// </summary>
    /// <param name="registerData">The new user's details.</param>
    /// <returns>An AuthResponseDto if successful, null otherwise.</returns>
    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerData)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerData);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return authResponse;
            }
            
            // Handle registration failure (e.g., 409 Conflict, 400 Bad Request)
            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"AuthService Register Error: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Attempts to get a new Access Token using a Refresh Token.
    /// </summary>
    /// <param name="refreshToken">The long-lived refresh token.</param>
    /// <returns>A new AuthResponseDto if successful, null otherwise.</returns>
    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var request = new RefreshTokenRequestDto { RefreshToken = refreshToken };
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                return authResponse;
            }
            
            // Handle refresh failure (e.g., 401 Unauthorized if token is revoked)
            return null;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"AuthService Refresh Error: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Tells the API to revoke a refresh token (logging out).
    /// </summary>
    /// <param name="refreshToken">The token to revoke.</param>
    /// <returns>True if the server successfully processed the request.</returns>
    public async Task<bool> LogoutAsync(string refreshToken)
    {
        try
        {
            var request = new RefreshTokenRequestDto { RefreshToken = refreshToken };
            var response = await _httpClient.PostAsJsonAsync("api/auth/revoke-token", request);

            // 204 NoContent is a success
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"AuthService Logout Error: {ex.Message}");
            return false;
        }
    }
}