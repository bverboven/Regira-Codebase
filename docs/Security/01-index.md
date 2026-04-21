# Regira Security

Regira Security provides encryption, password hashing, JWT authentication, and API Key authentication for .NET applications.

## Projects

| Project | Package | Purpose |
|---------|---------|---------|
| `Common.Security` | `Regira.Security` | Symmetric encryption and PBKDF2 hashing |
| `Security.Hashing.BCryptNet` | `Regira.Security.Hashing.BCryptNet` | BCrypt password hashing |
| `Security.Authentication` | `Regira.Security.Authentication` | JWT tokens and API Key auth |
| `Security.Authentication.Web` | `Regira.Security.Authentication.Web` | Pre-built auth controllers |

## Installation

```xml
<!-- Core encryption + hashing -->
<PackageReference Include="Regira.Security" Version="5.*" />

<!-- BCrypt password hashing -->
<PackageReference Include="Regira.Security.Hashing.BCryptNet" Version="5.*" />

<!-- JWT + API Key auth -->
<PackageReference Include="Regira.Security.Authentication" Version="5.*" />

<!-- Pre-built Identity controllers -->
<PackageReference Include="Regira.Security.Authentication.Web" Version="5.*" />
```

---

## Encryption

Two `IEncrypter` implementations:

```csharp
public interface IEncrypter
{
    string Encrypt(string plainText, string? key = null);
    string Decrypt(string encryptedText, string? key = null);
}
```

### SymmetricEncrypter

AES-256 with a static key derived from `CryptoOptions.Secret`. Fast; same key always produces the same ciphertext.

```csharp
var enc = new SymmetricEncrypter(new CryptoOptions { Secret = "my-app-key" });
string cipher = enc.Encrypt("sensitive value");
string plain  = enc.Decrypt(cipher);
```

### AesEncrypter

AES with a random salt prepended per encryption. Slower but produces different ciphertext on each call — recommended for stored secrets.

```csharp
var enc = new AesEncrypter(new CryptoOptions { Secret = "my-app-key" });
string cipher = enc.Encrypt("sensitive value");
```

### CryptoOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Secret` | `string?` | built-in salt key | Signing / derivation secret |
| `AlgorithmType` | `string?` | `"SHA512"` | Hash algorithm used for key derivation |
| `Encoding` | `Encoding?` | UTF-8 | Text encoding |

---

## Hashing

Two `IHasher` implementations:

```csharp
public interface IHasher
{
    string Hash(string? plainText);
    bool   Verify(string? plainText, string hashedValue);
}
```

### Hasher (PBKDF2)

Stores a per-hash random salt + PBKDF2 digest (10 000 iterations, SHA-512, 64-byte output). Constant-time verification.

```csharp
var hasher = new Regira.Security.Hashing.Hasher();
string stored = hasher.Hash("myPassword123");
bool ok       = hasher.Verify("myPassword123", stored);   // true
```

### Security.Hashing.BCryptNet — BCrypt Hasher

Enhanced BCrypt (SHA-384 by default). Highest cost factor. Recommended for passwords.

```csharp
var hasher = new Regira.Security.Hashing.BCryptNet.Hasher();
string stored = hasher.Hash("myPassword123");
bool ok       = hasher.Verify("myPassword123", stored);
```

### SimpleHasher

Double-SHA with salt — fast but weaker. Use for non-password data only.

---

## JWT Authentication

### JwtTokenOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Secret` | `string` | *(required)* | HMAC signing key |
| `Algorithm` | `string?` | HmacSha512 | Signing algorithm |
| `Authority` | `string?` | `null` | Token issuer |
| `Audience` | `string?` | `null` | Single audience |
| `Audiences` | `ICollection<string>?` | `null` | Multiple audiences |
| `LifeSpan` | `int` | `7200` | Token lifetime in seconds |
| `NameClaimType` | `string` | `"name"` | Claim used as user name |
| `RoleClaimType` | `string` | `"role"` | Claim used as role |

### ITokenHelper

```csharp
string      Create(IEnumerable<Claim> claims, string? audience = null, int? lifeSpan = null);
Task<bool>  Validate(string token);
```

### DI registration

```csharp
services.AddJwtAuthentication(options =>
{
    options.Secret    = configuration["Jwt:Secret"]!;
    options.Authority = configuration["Jwt:Issuer"];
    options.LifeSpan  = 3600;
});
// Registers ITokenHelper as transient and configures JwtBearer scheme.
```

### ClaimsPrincipal extensions

```csharp
string? userId = User.FindUserId();    // NameIdentifier claim
string? name   = User.FindUserName();  // Name claim
string? email  = User.FindEmail();     // Email claim
```

---

## API Key Authentication

### ApiKeyAuthenticationOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ApiKeyHeaderName` | `string` | `"X-Api-Key"` | Request header name |
| `AuthenticationType` | `string` | `"ApiKey"` | Authentication type string |

### IApiKeyOwnerService

```csharp
Task<ApiKeyOwner?> FindByOwner(string id);
Task<ApiKeyOwner?> FindByKey(string apiKey);
Task<bool>         Validate(string id, string apiKey);
```

### ApiKeyOwner model

| Property | Type | Description |
|----------|------|-------------|
| `OwnerId` | `string` | Owner identifier |
| `Key` | `string` | API key value |
| `Roles` | `ICollection<string>` | Roles assigned to this key |

### DI registration

```csharp
// In-memory keys from code
services.AddApiKeyAuthentication()
        .AddInMemoryApiKeyAuthentication(new[]
        {
            new ApiKeyOwner { OwnerId = "client-a", Key = "key-abc", Roles = ["read"] }
        });

// From appsettings.json
var keys = configuration.GetSection("ApiKeys").ToApiKeyOwners();
services.AddApiKeyAuthentication()
        .AddInMemoryApiKeyAuthentication(keys);
```

`appsettings.json` shape:
```json
"ApiKeys": {
  "client-a": { "Key": "key-abc", "Roles": ["read", "write"] }
}
```

---

## Pre-built Auth Controllers (`Security.Authentication.Web`)

Three abstract base controllers to inherit from. Each requires `[ApiController]` and a `[Route]` attribute on the concrete class.

### AccountControllerBase\<TUser\>

| Endpoint | Method | Description |
|----------|--------|-------------|
| `auth` | POST | Authenticate → returns JWT or lockout info |
| `auth/validate` | POST | Validates current token; returns 200 if user exists |
| `auth/refresh` | POST | Issues a fresh JWT for the current user |
| `auth/personal-data` | GET | Returns given/family names from claims |

### UserControllerBase\<TUser\>

| Endpoint | Method | Description |
|----------|--------|-------------|
| `users` | POST | Create user, send email confirmation |
| `users/confirm-email` | POST | Confirm email with token |

### PasswordControllerBase\<TUser\>

| Endpoint | Method | Description |
|----------|--------|-------------|
| `auth/password` | POST | Change password (authenticated) |
| `auth/password/recover` | POST | Send password reset email |
| `auth/password/reset` | POST | Reset password with token |

---

## Overview

1. **[Index](01-index.md)** — Overview, encryption, hashing, JWT, API key, and controllers
1. [Examples](02-examples.md) — Hash passwords, JWT setup, API key auth, Identity controllers
