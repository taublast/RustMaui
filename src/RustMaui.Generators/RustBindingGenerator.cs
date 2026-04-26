using Microsoft.CodeAnalysis;

namespace RustMaui.Generators;

[Generator]
public class RustBindingGenerator : IIncrementalGenerator
{
    // File generation is handled by the MSBuild WriteRustGenerated task in the package .targets file.
    // Rust.Generated.cs is written to the project root before CoreCompile.
    public void Initialize(IncrementalGeneratorInitializationContext context) { }
}
