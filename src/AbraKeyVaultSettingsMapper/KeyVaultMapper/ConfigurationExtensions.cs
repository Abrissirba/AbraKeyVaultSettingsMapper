using AbraKeyVaultSettingsMapper;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Provides extension methods for working with Azure Key Vault references in configuration.
/// </summary>
public static class AbraAzureKeyVaultHostApplicationConfigurationExtensions
{
    /// <summary>
    /// Checks if the given configuration key is a reference to an Azure Key Vault secret.
    /// </summary>
    /// <param name="configuration">The configuration instance to check.</param>
    /// <param name="key">The configuration key to check.</param>
    /// <returns>True if the configuration key is a reference to an Azure Key Vault secret; otherwise, false.</returns>
    public static bool IsAbraKeyVaultReference(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return value?.IsAbraKeyVaultReference() == true;
    }

    /// <summary>
    /// Checks if the given string value is a reference to an Azure Key Vault secret.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <returns>True if the string value is a reference to an Azure Key Vault secret; otherwise, false.</returns>
    public static bool IsAbraKeyVaultReference(this string value)
    {
        return value?.StartsWith(AbraAzureKeyVaultAppSettingsMapper.ReferencePrefix, StringComparison.Ordinal) == true; 
    }
}