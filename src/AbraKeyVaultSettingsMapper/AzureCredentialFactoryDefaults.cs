namespace AbraKeyVaultSettingsMapper;

/// <summary>
/// Default environment variable names used by <see cref="AzureCredentialFactory" />.
/// </summary>
public static class AzureCredentialFactoryDefaults
{
    /// <summary>
    /// Default environment variable used to resolve the Azure client ID.
    /// </summary>
    public const string AzureClientIdVariableName = "AZURE_CLIENT_ID";

    /// <summary>
    /// Default environment variable used by Azure Functions and App Service to expose a managed identity endpoint.
    /// </summary>
    public const string IdentityEndpointVariableName = "IDENTITY_ENDPOINT";

    /// <summary>
    /// Legacy/default environment variable used by some Azure hosts to expose a managed identity endpoint.
    /// </summary>
    public const string MsiEndpointVariableName = "MSI_ENDPOINT";
}
