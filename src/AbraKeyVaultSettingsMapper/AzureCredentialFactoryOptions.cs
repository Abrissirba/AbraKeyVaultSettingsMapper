namespace AbraKeyVaultSettingsMapper;

/// <summary>
/// Configures how <see cref="AzureCredentialFactory" /> inspects environment variables before creating credentials.
/// </summary>
public sealed class AzureCredentialFactoryOptions
{
    /// <summary>
    /// Gets or sets the environment variable name that contains the Azure client ID.
    /// </summary>
    public string AzureClientIdVariableName { get; set; } = AzureCredentialFactoryDefaults.AzureClientIdVariableName;

    /// <summary>
    /// Gets the environment variable names that indicate managed identity is available on the host.
    /// </summary>
    public IList<string> ManagedIdentityEndpointVariableNames { get; } =
    [
        AzureCredentialFactoryDefaults.IdentityEndpointVariableName,
        AzureCredentialFactoryDefaults.MsiEndpointVariableName
    ];
}
