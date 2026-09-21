using Azure.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace AbraKeyVaultSettingsMapper.Tests;

public class AzureCredentialFactoryTests
{
    [Fact]
    public void Create_InDevelopmentWithoutManagedIdentityOrClientId_ReturnsAzureCliCredential()
    {
        using var _ = new EnvironmentVariableScope();

        var credential = AzureCredentialFactory.Create(new TestHostEnvironment(Environments.Development));

        credential.ShouldBeOfType<AzureCliCredential>();
    }

    [Fact]
    public void Create_InProductionWithoutManagedIdentity_ReturnsDefaultAzureCredential()
    {
        using var _ = new EnvironmentVariableScope();

        var credential = AzureCredentialFactory.Create(new TestHostEnvironment(Environments.Production));

        credential.ShouldBeOfType<DefaultAzureCredential>();
    }

    [Fact]
    public void Create_WithManagedIdentityEndpointAndClientId_ReturnsManagedIdentityCredential()
    {
        using var scope = new EnvironmentVariableScope();
        scope.Set(AzureCredentialFactoryDefaults.IdentityEndpointVariableName, "http://localhost/metadata/identity/oauth2/token");
        scope.Set(AzureCredentialFactoryDefaults.AzureClientIdVariableName, "client-id");

        var credential = AzureCredentialFactory.Create(new TestHostEnvironment(Environments.Production));

        credential.ShouldBeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void Create_WithManagedIdentityEndpointWithoutClientId_ReturnsManagedIdentityCredential()
    {
        using var scope = new EnvironmentVariableScope();
        scope.Set(AzureCredentialFactoryDefaults.MsiEndpointVariableName, "http://localhost/metadata/identity/oauth2/token");

        var credential = AzureCredentialFactory.Create(new TestHostEnvironment(Environments.Production));

        credential.ShouldBeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void Create_WithCustomVariableNames_UsesConfiguredNames()
    {
        using var scope = new EnvironmentVariableScope();
        scope.Set("CUSTOM_MANAGED_IDENTITY_ENDPOINT", "http://localhost/metadata/identity/oauth2/token");

        var options = new AzureCredentialFactoryOptions
        {
            AzureClientIdVariableName = "CUSTOM_AZURE_CLIENT_ID"
        };
        options.ManagedIdentityEndpointVariableNames.Clear();
        options.ManagedIdentityEndpointVariableNames.Add("CUSTOM_MANAGED_IDENTITY_ENDPOINT");

        var credential = AzureCredentialFactory.Create(new TestHostEnvironment(Environments.Production), options);

        credential.ShouldBeOfType<ManagedIdentityCredential>();
    }

    [Fact]
    public void Create_WithWhitespaceManagedIdentityVariableName_ThrowsHelpfulException()
    {
        var options = new AzureCredentialFactoryOptions();
        options.ManagedIdentityEndpointVariableNames.Clear();
        options.ManagedIdentityEndpointVariableNames.Add(" ");

        var exception = Should.Throw<ArgumentException>(
            () => AzureCredentialFactory.Create(new TestHostEnvironment(Environments.Production), options));

        exception.ParamName.ShouldBe("options");
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = nameof(AbraKeyVaultSettingsMapper);

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly Dictionary<string, string?> _originalValues = new(StringComparer.Ordinal);

        public EnvironmentVariableScope()
        {
            Track(AzureCredentialFactoryDefaults.AzureClientIdVariableName);
            Track(AzureCredentialFactoryDefaults.IdentityEndpointVariableName);
            Track(AzureCredentialFactoryDefaults.MsiEndpointVariableName);
            Track("CUSTOM_AZURE_CLIENT_ID");
            Track("CUSTOM_MANAGED_IDENTITY_ENDPOINT");

            ClearTrackedVariables();
        }

        public void Set(string name, string value)
        {
            Track(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            foreach (var variable in _originalValues)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
            }
        }

        private void ClearTrackedVariables()
        {
            foreach (var variableName in _originalValues.Keys)
            {
                Environment.SetEnvironmentVariable(variableName, null);
            }
        }

        private void Track(string name)
        {
            if (_originalValues.ContainsKey(name))
            {
                return;
            }

            _originalValues[name] = Environment.GetEnvironmentVariable(name);
        }
    }
}
