# Community.MauiRust

Community.MauiRust brings .NET MAUI and Rust together through two NuGet packages: a build-time generator package and a `dotnet new` template package.

Use it to scaffold a MAUI app with a Rust native library already wired into the normal .NET build, with generated bindings and native build targets handled for you.

## Packages

### `Community.MauiRust.Generators`
 
 Build-time package that discovers Rust exports, generates `Rust.Generated.cs`, and wires Rust native build targets. 
 
- ships the generator/build package
- packs `build/Community.MauiRust.Generators.targets`
- produces the analyzer/build assets consumed by MAUI apps

 Package docs: [src/Community.MauiRust.Generators/README.md](src/Community.MauiRust.Generators/README.md).


### `Community.MauiRust.Templates`

A `dotnet new` template package that scaffolds a MAUI app already configured to use the generator package. See [package README](src/Community.MauiRust.Templates/README.md).

- ships the `dotnet new maui-rust` template
- carries the scaffold under `src/Community.MauiRust.Templates/content/MauiRust`
- emits a scaffolded app that references `Community.MauiRust.Generators`

Package docs: [src/Community.MauiRust.Templates/README.md](src/Community.MauiRust.Templates/README.md)


## Local commands

Pack both packages:

```powershell
.\dev\pack-all.ps1
```

Pack one package:

```powershell
.\dev\pack.ps1 -Project src/Community.MauiRust.Generators/Community.MauiRust.Generators.csproj
```

Validate the template against locally packed packages:

```powershell
.\dev\validate-template.ps1
```

That validator packs both packages, installs the local template package, generates a temporary app outside the repo tree, adds the local package folder as a NuGet source, and runs a Windows build against the generated app.

See [`nugets.md`](./nugets/nugets.md) for the shared GitHub Actions release workflow and required secrets.