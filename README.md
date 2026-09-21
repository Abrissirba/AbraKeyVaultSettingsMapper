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

### Azure credential helper

Azure Functions can be frustrating to diagnose when `DefaultAzureCredential` falls through several credential sources and the active source is misconfigured. In that state the host can spend a long time probing credentials and eventually time out with little or no useful console output.

`AbraAzureCredentialFactory` is included to make that behavior more predictable:

- in local development it prefers `AzureCliCredential` when no managed identity endpoint or client ID is configured
- in Azure it prefers `ManagedIdentityCredential` as soon as the host exposes a managed identity endpoint
- it falls back to `DefaultAzureCredential` when managed identity is not available

This reduces the "silent timeout" case during startup and gives consumers a single place to control which environment variable names are used.

`AddAbraAzureKeyVault` wraps the common `if (!string.IsNullOrWhiteSpace(keyVaultName))` startup check, builds the vault URI, chooses the credential, and applies `AbraAzureKeyVaultAppSettingsMapper` for you.

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
using AbraKeyVaultSettingsMapper;

var builder = WebApplication.CreateBuilder(args);
builder.AddAbraAzureKeyVault(builder.Configuration["Config:KeyVaultName"]);
```

### Multiple Key Vault Mappings

```csharp
using AbraKeyVaultSettingsMapper;

var builder = WebApplication.CreateBuilder(args);
builder.AddAbraAzureKeyVault(builder.Configuration["Config:SharedKeyVaultName"], "Shared");
builder.AddAbraAzureKeyVault(builder.Configuration["Config:AppKeyVaultName"], "App");
```

### Customizing environment variable names

If your host or deployment conventions use different variable names, pass `AbraAzureCredentialFactoryOptions`:

```csharp
using AbraKeyVaultSettingsMapper;

var builder = WebApplication.CreateBuilder(args);

var options = new AbraAzureCredentialFactoryOptions
{
    AzureClientIdVariableName = "MY_AZURE_CLIENT_ID"
};
options.ManagedIdentityEndpointVariableNames.Clear();
options.ManagedIdentityEndpointVariableNames.Add("MY_IDENTITY_ENDPOINT");

builder.AddAbraAzureKeyVault(
    builder.Configuration["Config:KeyVaultName"],
    credentialFactoryOptions: options);
```

By default the factory uses:

- `AZURE_CLIENT_ID`
- `IDENTITY_ENDPOINT`
- `MSI_ENDPOINT`

## Supported placeholder formats

- `AzureKeyVaultRef:Shared:OpenAiKey`
- `AzureKeyVaultRef:App:DatabaseConnectionString`
- `AzureKeyVaultRef:SomeSecretName`

The optional middle segment lets you scope references per logical vault name without forcing the actual secret name to match the final configuration path. When you omit that segment, the mapper treats the reference as a single-vault mapping.
