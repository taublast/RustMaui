# Community.MauiRust.Templates

`dotnet new` templates for .NET MAUI apps with a Rust native library wired into MSBuild.
The template consumes `Community.MauiRust.Generators` so the generated app gets both the Rust build targets and the automatic `Rust.Generated.cs` bindings.

This package now lives in the combined `Community.MauiRust` repo alongside the generator package.

## For users

Install:

```bash
dotnet new install Community.MauiRust.Templates
```

Update installed templates:

```bash
dotnet new update
```

Create a new app:

```bash
dotnet new maui-rust -n MyApp
```

The generated project includes MAUI app code, Rust crate code, prerequisite scripts, and a package reference to `Community.MauiRust.Generators`.

## Template source layout

Inside the combined `Community.MauiRust` repo the template package lives here:

```text
src/Community.MauiRust.Templates/
├── Community.MauiRust.Templates.csproj
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

The `sourceName = "MauiRust"` setting in `template.json` replaces `MauiRust` across file names, directories, and source content when a user runs `dotnet new maui-rust -n MyApp`.
Package IDs that must stay fixed, such as `Community.MauiRust.Generators`, are emitted through dedicated template tokens instead of raw `sourceName` replacement.

## Local development

Pack both packages:

```powershell
.\eng\pack-all.ps1
```

Pack only the template package:

```powershell
dotnet pack src/Community.MauiRust.Templates/Community.MauiRust.Templates.csproj -c Release -o artifacts/nupkg
```

Install the local template package:

```powershell
dotnet new install .\artifacts\nupkg\Community.MauiRust.Templates.<version>.nupkg
```

Fully validate the generated project from this repo:

```powershell
.\eng\validate-template.ps1
```

That script packs both packages, installs the local template package, adds the local package folder as a NuGet source, generates a temporary app, and builds the generated Windows project.

## Publishing

The combined repo release workflow packs both NuGet packages from one artifact set:

- `Community.MauiRust.Generators`
- `Community.MauiRust.Templates`

See `nugets/nugets.md` in the repo root for the workflow, secrets, and validation sequence.
