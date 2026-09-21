using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace AbraKeyVaultSettingsMapper.Tests;

public class AbraAzureKeyVaultAppSettingsMapperTests
{
    [Fact]
    public void GetKeyVaultReferences_WithScopedReferences_ReturnsSecretToConfigurationMappings()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DbContext"] = "AzureKeyVaultRef:App:DatabaseConnectionString",
            ["OpenAi:ApiKey"] = "AzureKeyVaultRef:Shared:OpenAiKey",
            ["Logging:LogLevel:Default"] = "Information"
        });

        var mappings = AbraAzureKeyVaultAppSettingsMapper.GetKeyVaultReferences(configuration, "App");

        mappings.ShouldHaveSingleItem();
        mappings["DatabaseConnectionString"].ShouldBe("ConnectionStrings:DbContext");
    }

    [Fact]
    public void GetKeyVaultReferences_WithoutVaultName_UsesBaseReferencePrefix()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DbContext"] = "AzureKeyVaultRef:DatabaseConnectionString",
            ["OpenAi:ApiKey"] = "AzureKeyVaultRef:App:OpenAiKey"
        });

        var mappings = AbraAzureKeyVaultAppSettingsMapper.GetKeyVaultReferences(configuration);

        mappings.ShouldHaveSingleItem();
        mappings["DatabaseConnectionString"].ShouldBe("ConnectionStrings:DbContext");
    }

    [Fact]
    public void Load_WithMappedSecret_ReturnsTrue()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DbContext"] = "AzureKeyVaultRef:App:DatabaseConnectionString"
        });
        var sut = new AbraAzureKeyVaultAppSettingsMapper(configuration, "App");

        var secret = new KeyVaultSecret("DatabaseConnectionString", "test-value");

        sut.Load(secret.Properties).ShouldBeTrue();
    }

    [Fact]
    public void GetKey_WithMappedSecret_ReturnsConfigurationKey()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DbContext"] = "AzureKeyVaultRef:App:DatabaseConnectionString"
        });
        var sut = new AbraAzureKeyVaultAppSettingsMapper(configuration, "App");

        var secret = new KeyVaultSecret("DatabaseConnectionString", "test-value");

        sut.GetKey(secret).ShouldBe("ConnectionStrings:DbContext");
    }

    [Fact]
    public void GetKey_WithUnmappedSecret_ThrowsHelpfulException()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DbContext"] = "AzureKeyVaultRef:App:DatabaseConnectionString"
        });
        var sut = new AbraAzureKeyVaultAppSettingsMapper(configuration, "App");

        var secret = new KeyVaultSecret("OtherSecret", "test-value");

        var exception = Should.Throw<KeyNotFoundException>(() => sut.GetKey(secret));

        exception.Message.ShouldContain("OtherSecret");
    }

    private static IConfigurationRoot BuildConfiguration(Dictionary<string, string?> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
