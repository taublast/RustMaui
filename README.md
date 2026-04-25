# MauiRust.Templates

`dotnet new` templates for .NET MAUI apps with a Rust native library wired into MSBuild.  
Press **F5** — Rust builds automatically alongside C#, on Windows, Android, iOS, and MacCatalyst.

---

## For users — install, create, update

### Install

```bash
dotnet new install MauiRust.Templates
```

### Update to a newer template version

```bash
dotnet new update
```

This updates all installed template packages, including `MauiRust.Templates`.  
Existing projects are **not** affected — only new projects created after the update use the new template.

### Uninstall

```bash
dotnet new uninstall MauiRust.Templates
```

### Create a new project

```bash
dotnet new maui-rust -n MyApp
```

This produces:

```
MyApp/
├── MyApp.sln
├── check-prerequisites.ps1     ← run on Windows to verify build prerequisites
├── check-prerequisites.sh      ← run on macOS/Linux to verify build prerequisites
├── Prerequisites.md            ← full setup guide
├── app/
│   └── MyApp/
│       └── MyApp.csproj        ← MAUI app, MSBuild wires Rust automatically
└── rust/
    └── myapp_native/           ← Rust crate (name derived from project name)
        ├── Cargo.toml
        └── src/lib.rs          ← your Rust FFI entry points
```

Open `MyApp.sln` in Visual Studio, select a target, press **F5**.  
MSBuild runs `cargo build` before the MAUI build. No separate terminal needed.

### Before building — verify prerequisites

Every generated project includes scripts that check what is missing:

```powershell
# Windows
.\check-prerequisites.ps1
```

```bash
# macOS / Linux
chmod +x check-prerequisites.sh && ./check-prerequisites.sh
```

Each script reports what is installed and what is not, with the exact fix command for each missing item. See `Prerequisites.md` in the generated project for the full setup guide.



---

## For developers — edit, test locally, publish

### Repo layout

```
MauiRust.Templates/
├── MauiRust.Templates.csproj   ← NuGet packaging project
├── .github/workflows/
│   └── publish.yml             ← publishes on git tag push
└── content/
    └── MauiRust/               ← template source (sourceName = "MauiRust")
        ├── .template.config/
        │   └── template.json   ← template metadata
        ├── MauiRust.sln
        ├── app/MauiRust/       ← MAUI project
        └── rust/mauirustnativelib_native/  ← Rust crate (renamed on instantiation)
```

The `sourceName = "MauiRust"` setting in `template.json` replaces every occurrence of
`MauiRust` — in file content, file names, and directory names — with the user-supplied
project name. A separate `casing` generator replaces `mauirustnativelib` (the token
embedded in crate and directory names) with the lowercased project name, so
`mauirustnativelib_native` becomes e.g. `myapp_native`.

### Edit the template

All template content lives under `content/MauiRust/`. Edit it like any normal project.

- C# namespace and class names: use `MauiRust` — it will be replaced on instantiation.
- Rust crate name: keep `mauirustnativelib` as the token inside crate/dir names — it is lowercased to the project name on instantiation (e.g. `myapp_native`).
- MAUI project file: `app/MauiRust/MauiRust.csproj`.
- Rust source: `rust/mauirustnativelib_native/src/lib.rs`.

### Test locally without publishing

**Step 1 — pack:**

```bash
dotnet pack -c Release -o ./nupkg
```

**Step 2 — install the local package:**

```bash
dotnet new install ./nupkg/MauiRust.Templates.1.0.0.nupkg
```

**Step 3 — create a test project:**

```bash
dotnet new maui-rust -n TestApp -o /tmp/TestApp
```

Inspect `/tmp/TestApp` to confirm the name substitution is correct.

**Step 4 — uninstall when done:**

```bash
dotnet new uninstall MauiRust.Templates
```

Repeat from Step 1 after each edit. The version in the `.nupkg` filename comes from
`<PackageVersion>` in `MauiRust.Templates.csproj` — bump it to avoid NuGet's local cache
serving an old package.

### Bump the version

Edit `MauiRust.Templates.csproj`:

```xml
<PackageVersion>1.1.0</PackageVersion>
```

Or override on the CLI without editing the file (useful for CI):

```bash
dotnet pack -c Release -p:PackageVersion=1.1.0 -o ./nupkg
```

### Publish to NuGet.org

**One-time setup — add the secret to GitHub:**

| Secret | What it is | Where to get it |
|--------|------------|-----------------|
| `NUGET_API_KEY` | NuGet.org API key scoped to push packages | [NuGet.org](https://www.nuget.org/account/apikeys) → *Create* → scope to `MauiRust.Templates`, select *Push new packages and package versions* |

**Publish by pushing a version tag:**

```bash
git tag v1.0.0
git push origin v1.0.0
```

The `publish.yml` workflow triggers, packs with version `1.0.0` (tag minus the `v`), and
pushes to NuGet.org. The package is live within a few minutes.

**Publish a patch:**

```bash
git tag v1.0.1
git push origin v1.0.1
```

No csproj edit needed — the version is derived entirely from the tag.
