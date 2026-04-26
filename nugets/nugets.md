# NuGet CI/CD

This repo publishes two NuGet packages from one manual workflow:

- `Community.MauiRust.Generators`
- `Community.MauiRust.Templates`

Workflow file:

- `.github/workflows/nuget-release.yml`

This is the release flow for the combined repo. Both packages are packed from the same workspace and published from one shared artifact set.

## What the workflow does

The workflow is manual and has three jobs:

1. `Pack NuGet packages`
2. `Publish to NuGet.org`
3. `Publish to GitHub Packages`

The pack job creates one artifact named `nuget-packages`. Both publish jobs consume that same artifact.

## Packed project list

- `src/Community.MauiRust.Generators/Community.MauiRust.Generators.csproj`
- `src/Community.MauiRust.Templates/Community.MauiRust.Templates.csproj`

## Required secrets

- `NUGET_API_KEY`: NuGet.org API key for package push

GitHub Packages publishing uses the built-in `secrets.GITHUB_TOKEN` and requires the package to be connected to the same repository that runs the workflow.

Both package projects should keep their `RepositoryUrl` pointed at the combined repository before relying on GitHub Packages metadata.

## How to run

1. Open `Actions`.
2. Select `Release NuGet Packages`.
3. Click `Run workflow`.
4. Choose whether to publish to NuGet.org.
5. Choose whether to publish to GitHub Packages.

Recommended first validation:

1. Run with both publish toggles disabled.
2. Inspect the `nuget-packages` artifact.
3. Confirm both `.nupkg` files are present.
4. Run again with one registry enabled at a time.

## Maintenance notes

- If package project paths move, update the explicit project list in the workflow.
- Keep the template scaffold pointing at `Community.MauiRust.Generators`; do not let dotnet template name replacement rewrite that package ID.
- If GitHub Packages returns `403 Forbidden`, first verify the package is linked to this repository.