# AbraKeyVaultSettingsMapper

`AbraKeyVaultSettingsMapper` packages a custom `KeyVaultSecretManager` that maps Azure Key Vault secret names to arbitrary .NET configuration keys.

This lets you keep readable, application-specific paths in `appsettings.json` while referencing secrets with placeholders such as:

```json
{
  "ConnectionStrings": {
    "DbContext": "AzureKeyVaultRef:App:DatabaseConnectionString"
  },
  "OpenAi": {
    "ApiKey": "AzureKeyVaultRef:Shared:OpenAiKey"
  }
}
```

When the Key Vault provider loads secrets, the mapper redirects:

- `DatabaseConnectionString` -> `ConnectionStrings:DbContext`
- `OpenAiKey` -> `OpenAi:ApiKey`

The `keyVaultName` constructor argument is optional. You only need it when you want to distinguish references for multiple logical Key Vault mappings. If your application uses a single Key Vault, you can omit it and reference secrets directly.

## Installation

```bash
dotnet add package AbraKeyVaultSettingsMapper
```

## Usage

### Single Key Vault

If you only use one Key Vault, omit the `keyVaultName` argument and reference secrets without the extra segment:

```json
{
  "ConnectionStrings": {
    "DbContext": "AzureKeyVaultRef:DatabaseConnectionString"
  },
  "OpenAi": {
    "ApiKey": "AzureKeyVaultRef:OpenAiKey"
  }
}
```

```csharp
using Azure.Identity;
using AbraKeyVaultSettingsMapper;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

configuration.AddAzureKeyVault(
    new Uri($"https://{configuration["Config:KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential(),
    new AbraAzureKeyVaultAppSettingsMapper(configuration)
);
```

### Multiple Key Vault Mappings

```csharp
using Azure.Identity;
using AbraKeyVaultSettingsMapper;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

configuration.AddAzureKeyVault(
    new Uri($"https://{configuration["Config:SharedKeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential(),
    new AbraAzureKeyVaultAppSettingsMapper(configuration, "Shared")
);

configuration.AddAzureKeyVault(
    new Uri($"https://{configuration["Config:AppKeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential(),
    new AbraAzureKeyVaultAppSettingsMapper(configuration, "App")
);
```

## Supported placeholder formats

- `AzureKeyVaultRef:Shared:OpenAiKey`
- `AzureKeyVaultRef:App:DatabaseConnectionString`
- `AzureKeyVaultRef:SomeSecretName`

The optional middle segment lets you scope references per logical vault name without forcing the actual secret name to match the final configuration path. When you omit that segment, the mapper treats the reference as a single-vault mapping.


