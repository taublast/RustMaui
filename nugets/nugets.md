# NuGet CI/CD

This repo publishes three packages from one manual workflow:

- `RustMaui`
- `RustMaui.Generators`
- `RustMaui.Templates`

Workflow file:

- `.github/workflows/nuget-release.yml`

This is the release flow for the combined repo. All packages are packed from the same workspace and published from one shared artifact set.

## What the workflow does

The workflow is manual and supports two release modes:

- `safe`: pack, validate on Windows/Android/iOS, then publish
- `quick`: pack, then publish without the validation gate

In `safe` mode the workflow has six jobs in total:

1. `Pack NuGet packages`
2. `Validate template on Windows`
3. `Validate template on Android`
4. `Validate template on iOS`
5. `Publish to NuGet.org`
6. `Publish to GitHub Packages`

The pack job creates one artifact named `nuget-packages`. Both publish jobs consume that same artifact.

## Packed project list

- `src/RustMaui.Tool/Community.MauiRust.Tool.csproj`
- `src/RustMaui.Generators/Community.MauiRust.Generators.csproj`
- `src/RustMaui.Templates/Community.MauiRust.Templates.csproj`

## Required secrets

- `NUGET_API_KEY`: NuGet.org API key for package push

GitHub Packages publishing uses the built-in `secrets.GITHUB_TOKEN` and requires the package to be connected to the same repository that runs the workflow.

Both package projects should keep their `RepositoryUrl` pointed at the combined repository before relying on GitHub Packages metadata.

## How to run

1. Open `Actions`.
2. Select `Release NuGet Packages`.
3. Click `Run workflow`.
4. Choose `release_mode`:
	- `safe` to validate before publishing
	- `quick` to skip the validation gate for tiny low-risk changes
5. Choose whether to publish to NuGet.org.
6. Choose whether to publish to GitHub Packages.

Recommended first validation:

1. Run in `safe` mode with both publish toggles disabled.
2. Inspect the `nuget-packages` artifact.
3. Confirm all three `.nupkg` files are present.
4. Confirm Windows, Android, and iOS validation jobs passed.
5. Run again with one registry enabled at a time.

## Maintenance notes

- If package project paths move, update the explicit project list in the workflow.
- Keep the template scaffold pointing at `RustMaui.Generators`; do not let dotnet template name replacement rewrite that package ID.
- Use `safe` mode for generator, template, workflow, or versioning changes; use `quick` only for very small low-risk updates.
- If GitHub Packages returns `403 Forbidden`, first verify the package is linked to this repository.