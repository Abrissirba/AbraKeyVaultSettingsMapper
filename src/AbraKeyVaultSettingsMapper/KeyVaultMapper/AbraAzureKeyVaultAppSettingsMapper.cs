using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace AbraKeyVaultSettingsMapper;

/// <summary>
/// Maps Azure Key Vault secret names to arbitrary configuration keys by scanning configuration
/// values for <c>AzureKeyVaultRef:</c> placeholders.
/// </summary>
public sealed class AbraAzureKeyVaultAppSettingsMapper : KeyVaultSecretManager
{
    /// <summary>
    /// Prefix used in configuration values to indicate that the remainder should be resolved from Key Vault.
    /// </summary>
    public const string ReferencePrefix = "AzureKeyVaultRef:";

    private readonly IReadOnlyDictionary<string, string> _azureKeyVaultKeys;

    /// <summary>
    /// Creates a mapper that redirects Key Vault secrets into the configuration keys that reference them.
    /// </summary>
    /// <param name="configuration">The configuration tree to scan for <c>AzureKeyVaultRef:</c> placeholders.</param>
    /// <param name="keyVaultName">
    /// Optional logical vault scope. When provided, only references starting with
    /// <c>AzureKeyVaultRef:&lt;keyVaultName&gt;:</c> are considered.
    /// </param>
    public AbraAzureKeyVaultAppSettingsMapper(IConfigurationRoot configuration, string? keyVaultName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _azureKeyVaultKeys = GetKeyVaultReferences(configuration, keyVaultName);
    }

    /// <summary>
    /// Determines whether the given secret should be loaded because it is referenced by configuration.
    /// </summary>
    public override bool Load(SecretProperties properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        return _azureKeyVaultKeys.ContainsKey(properties.Name);
    }

    /// <summary>
    /// Returns the destination configuration key for a loaded Key Vault secret.
    /// </summary>
    public override string GetKey(KeyVaultSecret secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        if (_azureKeyVaultKeys.TryGetValue(secret.Name, out var configurationKey))
        {
            return configurationKey;
        }

        throw new KeyNotFoundException($"The Azure Key Vault secret '{secret.Name}' is not mapped to a configuration key.");
    }

    /// <summary>
    /// Scans configuration for Key Vault references and returns a map of secret names to destination configuration keys.
    /// </summary>
    public static Dictionary<string, string> GetKeyVaultReferences(IConfigurationRoot configuration, string? keyVaultName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var valuePrefix = $"{ReferencePrefix}{(string.IsNullOrWhiteSpace(keyVaultName) ? string.Empty : $"{keyVaultName}:")}";
        var keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in configuration.AsEnumerable())
        {
            if (string.IsNullOrEmpty(kvp.Value) || !kvp.Value.StartsWith(valuePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            var secretName = kvp.Value[valuePrefix.Length..];
            if (string.IsNullOrWhiteSpace(secretName))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(keyVaultName) && secretName.Contains(':', StringComparison.Ordinal))
            {
                continue;
            }

            keys[secretName] = kvp.Key;
        }

        return keys;
    }
}
