using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Hosting;

namespace AbraKeyVaultSettingsMapper;

/// <summary>
/// Creates Azure credentials with predictable environment-specific behavior for Key Vault configuration loading.
/// </summary>
public static class AzureCredentialFactory
{
    /// <summary>
    /// Creates a credential using the default environment variable names.
    /// </summary>
    /// <param name="environment">The current host environment.</param>
    public static TokenCredential Create(IHostEnvironment environment)
    {
        return Create(environment, null);
    }

    /// <summary>
    /// Creates a credential using the supplied environment variable configuration.
    /// </summary>
    /// <param name="environment">The current host environment.</param>
    /// <param name="options">Options for resolving managed identity related environment variables.</param>
    public static TokenCredential Create(IHostEnvironment environment, AzureCredentialFactoryOptions? options)
    {
        ArgumentNullException.ThrowIfNull(environment);

        options ??= new AzureCredentialFactoryOptions();
        ValidateOptions(options);

        var azureClientId = GetConfiguredValue(options.AzureClientIdVariableName);
        var isManagedIdentityConfigured = IsManagedIdentityConfigured(options.ManagedIdentityEndpointVariableNames);

        if (environment.IsDevelopment() && !isManagedIdentityConfigured && azureClientId is null)
        {
            return new AzureCliCredential();
        }

        if (isManagedIdentityConfigured)
        {
            var managedIdentityId = azureClientId is null
                ? ManagedIdentityId.SystemAssigned
                : ManagedIdentityId.FromUserAssignedClientId(azureClientId);

            return new ManagedIdentityCredential(managedIdentityId);
        }

        return new DefaultAzureCredential();
    }

    private static void ValidateOptions(AzureCredentialFactoryOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.AzureClientIdVariableName);

        if (options.ManagedIdentityEndpointVariableNames.Count == 0)
        {
            throw new ArgumentException("At least one managed identity endpoint variable name must be configured.", nameof(options));
        }

        if (options.ManagedIdentityEndpointVariableNames.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Managed identity endpoint variable names cannot be null, empty, or whitespace.", nameof(options));
        }
    }

    private static bool IsManagedIdentityConfigured(IEnumerable<string> variableNames)
    {
        return variableNames.Any(static variableName => GetConfiguredValue(variableName) is not null);
    }

    private static string? GetConfiguredValue(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
