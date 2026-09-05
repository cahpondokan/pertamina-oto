using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pertamina.Oto.Architecture.Tests;

[TestClass]
public sealed class ContractsProjectDependencyTests
{
    [TestMethod]
    public void Contracts_project_has_no_outbound_references()
    {
        var repositoryRoot = FindRepositoryRoot();
        var contractsProject = Path.Combine(
            repositoryRoot,
            "src",
            "BuildingBlocks",
            "Pertamina.Oto.Contracts",
            "Pertamina.Oto.Contracts.csproj");

        Assert.IsTrue(File.Exists(contractsProject), $"Expected Contracts project at {contractsProject}.");

        ContractsProjectDependencyRule.EnsureCompliant(File.ReadAllText(contractsProject));
    }

    [TestMethod]
    public void Negative_canary_rejects_a_forbidden_module_reference()
    {
        const string invalidProject = """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <ProjectReference Include="..\..\Modules\Pdf\Pertamina.Oto.Pdf.Infrastructure.csproj" />
              </ItemGroup>
            </Project>
            """;

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => ContractsProjectDependencyRule.EnsureCompliant(invalidProject));

        StringAssert.Contains(exception.Message, "ProjectReference");
        StringAssert.Contains(exception.Message, "Pdf.Infrastructure");
    }

    private static string FindRepositoryRoot()
    {
        for (var candidate = new DirectoryInfo(AppContext.BaseDirectory); candidate is not null; candidate = candidate.Parent)
        {
            if (File.Exists(Path.Combine(candidate.FullName, "global.json")) &&
                Directory.Exists(Path.Combine(candidate.FullName, "src")))
            {
                return candidate.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the repository root above test output directory {AppContext.BaseDirectory}.");
    }
}
