# RustMaui.Generators

Build-time package for [.NET MAUI + Rust](https://github.com/taublast/RustMaui) projects.

Reads your `rust/lib.rs`, finds every `#[no_mangle] pub extern "C" fn`, and generates matching `[LibraryImport]` P/Invoke bindings into `Rust.Generated.cs` — automatically, on every build.

## How it works

Add a Rust export:

```rust
#[no_mangle]
pub extern "C" fn compute(value: f32) -> f32 {
    value * 2.0
}
```

The generator emits:

```csharp
// Rust.Generated.cs — do not edit
[LibraryImport(Lib, EntryPoint = "compute")]
[UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
public static partial float Compute(float value);
```

Call it from C#:

```csharp
var result = Rust.Compute(3.14f);
```

## Override a binding

Declare the same `EntryPoint` in your `Rust.cs` — the generator skips it:

```csharp
// Rust.cs — your file, generator leaves this alone
[LibraryImport(Lib, EntryPoint = "compute")]
public static partial float Compute(float value);
```

## Unknown types

If the generator cannot map a Rust type to C#, it emits a TODO comment instead of broken code:

```csharp
// TODO: bind 'complex_func' — unrecognized type(s), declare manually in Rust.cs
```

## Setup

### Installed via RustMaui.Templates

Zero config. The template package includes the generator and emits a fixed `RustMaui.Generators` package reference in the scaffolded app.

### Install into an existing app from NuGet

You can also add the package to an existing MAUI project with the CLI:

```bash
dotnet add package RustMaui.Generators
```

After that, make sure your project follows the expected layout or set the relevant MSBuild overrides shown below.

### Manual install

```xml
<PackageReference Include="RustMaui.Generators" Version="1.0.0-pre12">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

The package reads `rust/Cargo.toml` to infer the native library name and writes the generated `Lib` constant for you when `Rust.cs` does not already define one.

The package also wires the build-time generation logic from the repo-level `build/RustMaui.Generators.targets` file. `RustCrateDir` defaults to the template layout, but you can override it.

### Optional MSBuild properties

The package works with no required properties in template-generated projects, but it supports these optional overrides in your app `.csproj`:

| Property | Default value | Purpose |
|---|---|---|
| `RustCrateDir` | `../../rust` from the app project directory | Path to the Rust crate root |
| `RustLibName` | Auto-read from `Cargo.toml` `[package].name` and normalized for Rust | Native library name |
| `RustProfile` | `release` | Cargo profile to build with |
| `RustCargoToolchainArgs` | Empty | Extra toolchain selector arguments such as `+nightly` |
| `RustGeneratorSrcDir` | `$(RustCrateDir)` | Override path for `lib.rs` when it is not under the crate root |
| `RustGeneratedFile` | `$(MSBuildProjectDirectory)\Rust.Generated.cs` | Override output path for `Rust.Generated.cs` |

For example, with a custom layout:

```xml
<PropertyGroup>
  <RustCrateDir>..\..\native\mycrate</RustCrateDir>
  <RustGeneratorSrcDir>..\..\rust</RustGeneratorSrcDir>
  <RustLibName>your_crate_name</RustLibName>
  <RustProfile>release</RustProfile>
  <RustGeneratedFile>Generated\RustBindings.g.cs</RustGeneratedFile>
</PropertyGroup>
```

## Supported type mappings

| Rust | C# |
|---|---|
| `i32` / `c_int` | `int` |
| `u32` / `c_uint` | `uint` |
| `f32` / `c_float` | `float` |
| `f64` / `c_double` | `double` |
| `i64` / `u64` | `long` / `ulong` |
| `isize` / `usize` | `nint` / `nuint` |
| `*mut c_void` / `*const c_void` | `IntPtr` |
| `*mut u8` / `*const u8` | `IntPtr` |

Everything else → TODO comment.

## Repo layout

Inside the combined `RustMaui` repo the package lives here:

```text
src/RustMaui.Generators/
build/RustMaui.Generators.targets
build/RustMaui.Generators.props
```

The template package under `src/RustMaui.Templates/` consumes this package.

Both packages are released together from the combined repo via `.github/workflows/nuget-release.yml`.
