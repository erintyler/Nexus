# Discord OAuth Authentication Setup

This document explains how to set up and use Discord OAuth authentication in the Nexus application.

## Overview

The authentication flow works as follows:

1. User clicks "Login" in the Frontend (BFF - Backend for Frontend)
2. User is redirected to Discord OAuth authorization page
3. After authorization, Discord redirects back with an authorization code
4. The BFF exchanges the code for a Discord access token
5. The BFF sends the Discord access token to the API's `/api/auth/exchange` endpoint
6. The API validates the Discord token and generates a JWT
7. The JWT is stored in the BFF's authentication cookie
8. Subsequent requests from the Frontend include the JWT for API authentication

## Setup Instructions

### 1. Create a Discord Application

1. Go to [Discord Developer Portal](https://discord.com/developers/applications)
2. Click "New Application" and give it a name
3. Go to the "OAuth2" section
4. Copy the "Client ID" and "Client Secret"
5. Add a redirect URI: `https://localhost:7000/signin-discord` (adjust port as needed)

### 2. Configure the Frontend (BFF)

Update `appsettings.Development.json`:

```json
{
  "Discord": {
    "ClientId": "your-discord-client-id-here",
    "ClientSecret": "your-discord-client-secret-here"
  },
  "ApiBaseUrl": "https://localhost:7001"
}
```

### 3. Configure the API

Update `appsettings.Development.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-minimum-32-characters-long",
    "Issuer": "Nexus.Api",
    "Audience": "Nexus.Frontend"
  }
}
```

**Important**: Generate a strong secret key for production. The secret key should be at least 32 characters long.

### 4. Environment Variables (Production)

For production, use environment variables or Azure Key Vault instead of storing secrets in configuration files:

**Frontend:**
- `Discord__ClientId`
- `Discord__ClientSecret`
- `ApiBaseUrl`

**API:**
- `JwtSettings__SecretKey`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`

## Usage

### Frontend Routes

- **`/login`**: Initiates Discord OAuth flow
  - Query parameter: `returnUrl` (optional) - URL to redirect after login
- **`/logout`**: Logs out the user and clears the authentication cookie

### API Endpoints

- **`POST /api/auth/exchange`**: Exchanges Discord access token for JWT
  - Request body: `{ "AccessToken": "discord-access-token" }`
  - Response: `{ "AccessToken": "jwt-token", "TokenType": "Bearer", "ExpiresIn": 3600 }`

## Claims in JWT

The JWT includes the following claims from Discord:

- `sub` (NameIdentifier): Discord user ID
- `name`: Discord username
- `email`: Discord email
- `discord_id`: Discord user ID
- `discord_username`: Discord username
- `discord_discriminator`: Discord discriminator
- `discord_avatar`: Discord avatar hash

## Accessing the JWT in Blazor Components

To access user information in your Blazor components:

```csharp
@inject AuthenticationStateProvider AuthenticationStateProvider

@code {
    private async Task<string?> GetJwtToken()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.Identity?.IsAuthenticated == true)
        {
            return user.FindFirst("jwt_token")?.Value;
        }
        
        return null;
    }
}
```

## Making Authenticated API Calls

When making HTTP requests to the API from the BFF, include the JWT from the cookie:

```csharp
@inject IHttpContextAccessor HttpContextAccessor

@code {
    private async Task CallApiAsync(HttpClient httpClient)
    {
        var jwtToken = HttpContextAccessor.HttpContext?.User.FindFirst("jwt_token")?.Value;
        
        if (!string.IsNullOrEmpty(jwtToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", jwtToken);
        }
        
        var response = await httpClient.GetAsync("https://localhost:7001/api/some-endpoint");
        // Handle response
    }
}
```

## Security Considerations

1. **HTTPS Only**: Always use HTTPS in production
2. **Secure Cookies**: The authentication cookie is configured with:
   - `HttpOnly = true` (prevents JavaScript access)
   - `SecurePolicy = Always` (requires HTTPS)
   - `SameSite = Lax` (CSRF protection)
3. **Secret Key**: Use a strong, randomly generated secret key (minimum 32 characters)
4. **Token Expiration**: JWTs expire after 1 hour by default
5. **Sliding Expiration**: Cookie has sliding expiration enabled

## Troubleshooting

### "Discord:ClientId not configured" error
- Ensure the Discord configuration is present in appsettings.json or environment variables

### "Failed to exchange token" error
- Verify the API is running and accessible from the Frontend
- Check the `ApiBaseUrl` configuration
- Ensure the JWT settings are correctly configured in the API

### 401 Unauthorized on API calls
- Verify the JWT is being sent in the Authorization header
- Check that the JWT secret key matches between token generation and validation
- Ensure the JWT hasn't expired

