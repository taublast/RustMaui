# Community.MauiRust.Generators

Build-time package for [.NET MAUI + Rust](https://github.com/taublast/Community.MauiRust) projects.

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

### Installed via Community.MauiRust.Templates

Zero config. The template package includes the generator and emits a fixed `Community.MauiRust.Generators` package reference in the scaffolded app.

### Manual install

```xml
<PackageReference Include="Community.MauiRust.Generators" Version="1.0.0-pre10">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

The package reads `rust/Cargo.toml` to infer the native library name and writes the generated `Lib` constant for you when `Rust.cs` does not already define one.

The package also wires the build-time generation logic from the repo-level `build/Community.MauiRust.Generators.targets` file. `RustCrateDir` defaults to the template layout, but you can override it.

For a custom layout set `RustGeneratorSrcDir` or `RustLibName` in your app project:

```xml
<PropertyGroup>
  <RustGeneratorSrcDir>..\..\rust</RustGeneratorSrcDir>
  <RustLibName>your_crate_name</RustLibName>
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

Inside the combined `Community.MauiRust` repo the package lives here:

```text
src/Community.MauiRust.Generators/
build/Community.MauiRust.Generators.targets
build/Community.MauiRust.Generators.props
```

The template package under `src/Community.MauiRust.Templates/` consumes this package.

Both packages are released together from the combined repo via `.github/workflows/nuget-release.yml`.
