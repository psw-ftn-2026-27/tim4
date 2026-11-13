using Xunit;

namespace Host.Tests.DependencyTests;

// No Shared project may know about any module, and Shared.Domain and Shared.Application stay free of frameworks.
public class SharedKernelTests : BaseArchitectureTests
{
    [Theory]
    [MemberData(nameof(SharedProjectNames))]
    public void Shared_project_does_not_depend_on_any_module(string sharedProject)
    {
        var moduleAssemblies = Modules
            .SelectMany(module => Layers.Select(layer => $"{module}.{layer}"))
            .Append("Identity")
            .ToArray();
        AssertNoDependency(sharedProject, moduleAssemblies);
    }

    [Theory]
    [InlineData("Shared.Domain")]
    [InlineData("Shared.Application")]
    public void Shared_domain_and_application_are_framework_free(string sharedProject)
    {
        AssertNoNamespaceDependency(sharedProject, "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");
    }
}
