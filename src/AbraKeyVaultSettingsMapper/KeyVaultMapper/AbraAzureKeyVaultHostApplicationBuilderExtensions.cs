using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AbraKeyVaultSettingsMapper;

/// <summary>
/// Helper methods for wiring Azure Key Vault into an application builder.
/// </summary>
public static class AbraAzureKeyVaultHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Azure Key Vault configuration when a vault name is provided.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="keyVaultName">The Azure Key Vault DNS name prefix.</param>
    /// <param name="keyVaultReferenceName">
    /// Optional logical vault scope used by <see cref="AbraAzureKeyVaultAppSettingsMapper" />.
    /// </param>
    /// <param name="credentialFactoryOptions">
    /// Optional overrides for how <see cref="AbraAzureCredentialFactory" /> resolves managed identity settings.
    /// </param>
    /// <returns>The same <paramref name="builder" /> instance.</returns>
    public static IHostApplicationBuilder AddAbraAzureKeyVault(
        this IHostApplicationBuilder builder,
        string? keyVaultName,
        string? keyVaultReferenceName = null,
        AbraAzureCredentialFactoryOptions? credentialFactoryOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(keyVaultName))
        {
            return builder;
        }

        var configurationBuilder = (IConfigurationBuilder)builder.Configuration;
        var configurationRoot = (IConfigurationRoot)builder.Configuration;

        configurationBuilder.AddAzureKeyVault(
            new Uri($"https://{keyVaultName}.vault.azure.net/"),
            AbraAzureCredentialFactory.Create(builder.Environment, credentialFactoryOptions),
            new AbraAzureKeyVaultAppSettingsMapper(configurationRoot, keyVaultReferenceName));

        return builder;
    }
}
