# RustMaui

`RustMaui` is a .NET tool that reuses the shared MauiRust scaffold for two entry points:

- `rustmaui new` creates a new .NET MAUI + Rust app from the shared scaffold
- `rustmaui init` retrofits an existing MAUI app by adding the generator package reference and starter Rust crate files

## Install

```bash
dotnet tool install --global RustMaui
```

## Commands

Create a new app:

```bash
rustmaui new MyApp
```

Initialize an existing app:

```bash
rustmaui init path/to/MyApp.csproj
```

The tool leaves existing files alone and only creates missing Rust boilerplate.