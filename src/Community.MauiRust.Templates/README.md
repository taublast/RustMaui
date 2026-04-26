# Community.MauiRust.Templates

`dotnet new` templates for .NET MAUI apps with a Rust native library wired into MSBuild.
The template consumes `Community.MauiRust.Generators` so the generated app gets both the Rust build targets and the automatic `Rust.Generated.cs` bindings.

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
        ├── MauiRust.sln
        ├── src/
        ├── app/
        └── rust/
```

The `sourceName = "MauiRust"` setting in `template.json` replaces `MauiRust` across file names, directories, and source content when a user runs `dotnet new maui-rust -n MyApp`.

## Local development

Pack the template package:

```bash
dotnet pack src/Community.MauiRust.Templates/Community.MauiRust.Templates.csproj -c Release -o artifacts/nupkg
```

Install the local template package:

```bash
dotnet new install ./artifacts/nupkg/Community.MauiRust.Templates.1.0.0-pre4.nupkg
```

If you want to fully validate a generated project from this repo, also pack `Community.MauiRust.Generators` and add `artifacts/nupkg` as a local NuGet source before building the generated app.

## Publishing

The combined repo release workflow packs both NuGet packages from one artifact set:

- `Community.MauiRust.Generators`
- `Community.MauiRust.Templates`

See `nugets/nugets.md` in the repo root for the workflow, secrets, and validation sequence.
