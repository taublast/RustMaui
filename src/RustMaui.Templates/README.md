# RustMaui.Templates

`dotnet new` templates for .NET MAUI apps with a Rust native library wired into MSBuild.
The template consumes `RustMaui.Generators` so the generated app gets both the Rust build targets and the automatic `Rust.Generated.cs` bindings.

This package now lives in the combined `RustMaui` repo alongside the generator package.
If you want the preferred entry point for both new and existing apps, install the tool package instead: `dotnet tool install --global RustMaui`.

## For users

Install:

```bash
dotnet new install RustMaui.Templates
```

Update installed templates:

```bash
dotnet new update
```

Create a new app:

```bash
dotnet new rustmaui -n MyApp
```

The generated project includes MAUI app code, Rust crate code, prerequisite scripts, and a package reference to `RustMaui.Generators`.

Generated projects follow the shared Apple platform contract from `RustMaui.Generators`:

- iOS device and iOS simulator use static Rust archives imported from `__Internal`
- MacCatalyst uses a bundled dynamic library imported by library name

If you customize the scaffolded Rust code, keep that distinction intact so your app stays aligned with the package behavior.

This package is optional and focused on new-app scaffolding only. For one install path that covers both `new` and `init`, use `RustMaui`.

## Template source layout

Inside the combined `RustMaui` repo the template package lives here:

```text
src/RustMaui.Templates/
├── RustMaui.Templates.csproj
├── README.md
└── content/
    └── MauiRust/
        ├── .template.config/
        ├── check-prerequisites.ps1
        ├── check-prerequisites.sh
        ├── MauiRust.sln
        ├── Prerequisites.md
        ├── app/
        ├── rust/
        └── src/
```

The `sourceName = "MauiRust"` setting in `template.json` replaces `MauiRust` across file names, directories, and source content when a user runs `dotnet new rustmaui -n MyApp`.
Package IDs that must stay fixed, such as `RustMaui.Generators`, are emitted through dedicated template tokens instead of raw `sourceName` replacement.

## Local development

Pack all three packages:

```powershell
.\dev\pack-all.ps1
```

Pack only the template package:

```powershell
dotnet pack src/RustMaui.Templates/RustMaui.Templates.csproj -c Release -o artifacts/nupkg
```

Install the local template package:

```powershell
dotnet new install .\artifacts\nupkg\RustMaui.Templates.<version>.nupkg
```

Fully validate the generated project from this repo:

```powershell
.\dev\validate-template.ps1
```

That script packs both packages, installs the local template package, adds the local package folder as a NuGet source, generates a temporary app, and builds the generated Windows project.

## Publishing

The combined repo release workflow packs all three packages from one artifact set:

- `RustMaui`
- `RustMaui.Generators`
- `RustMaui.Templates`

See `nugets/nugets.md` in the repo root for the workflow, secrets, and validation sequence.
