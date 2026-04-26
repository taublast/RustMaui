# Prerequisites

Everything you need installed before opening this solution.

---

## .NET and MAUI

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then install the MAUI workloads:

```bash
dotnet workload install maui
```

Verify:

```bash
dotnet --version          # 10.x
dotnet workload list      # maui and platform workloads listed
```

---

## Rust

Install Rust via [rustup](https://rustup.rs):

**macOS / Linux**
```bash
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
```

**Windows** — download and run [rustup-init.exe](https://win.rustup.rs)

Verify:

```bash
rustc --version
cargo --version
```

> **Windows note:** Rust on Windows uses the MSVC linker (`link.exe`). You do **not** need full Visual Studio — [Visual Studio Build Tools](https://visualstudio.microsoft.com/downloads/) (free, no IDE) is enough. Install it and select the **Desktop development with C++** workload. VS Code users only need Build Tools. If `cargo build` fails with a linker error after installing, run `rustup default stable-msvc` and rebuild.

---

## Platform targets

Install only the targets for the platforms you intend to build.

### Windows

No extra targets needed. `x86_64-pc-windows-msvc` is installed by default.

### Android

```bash
rustup target add aarch64-linux-android x86_64-linux-android
```

Also install `cargo-ndk`, which handles the NDK linker setup automatically:

```bash
cargo install cargo-ndk
```

Then install the Android NDK (version 27 or later recommended):

- **Via Android Studio:** *Settings → SDK Manager → SDK Tools → NDK (Side by side)*
- **Via command line:**
  ```bash
  sdkmanager --install "ndk;27.2.12479018"
  ```

Finally, set `ANDROID_NDK_HOME` to the NDK directory:

**macOS / Linux** — add to `~/.zshrc` or `~/.bashrc`:
```bash
export ANDROID_NDK_HOME=$ANDROID_SDK_ROOT/ndk/27.2.12479018
```

**Windows** — set as a user environment variable:
```
ANDROID_NDK_HOME = C:\Users\<you>\AppData\Local\Android\Sdk\ndk\27.2.12479018
```

Verify:

```bash
echo $ANDROID_NDK_HOME      # must print a real path
cargo ndk --version
rustup target list --installed | grep android
```

### iOS (macOS only)

Requires Xcode with command-line tools:

```bash
xcode-select --install
```

Add iOS targets:

```bash
# Physical devices
rustup target add aarch64-apple-ios

# Simulators
rustup target add aarch64-apple-ios-sim   # Apple Silicon Mac
rustup target add x86_64-apple-ios        # Intel Mac
```

### macOS / MacCatalyst (macOS only)

```bash
rustup target add aarch64-apple-ios-macabi x86_64-apple-ios-macabi
```

> These targets are Tier 3. If your stable toolchain reports them as unavailable, install nightly and pass `-p:RustCargoToolchainArgs=+nightly` to `dotnet build`.

---

## Quick checklist

| Platform    | Rust target(s)                                               | Extra tools            |
|-------------|--------------------------------------------------------------|------------------------|
| Windows     | `x86_64-pc-windows-msvc` (default)                          | —                      |
| Android     | `aarch64-linux-android` `x86_64-linux-android`              | NDK + `cargo-ndk`      |
| iOS device  | `aarch64-apple-ios`                                         | Xcode                  |
| iOS sim arm64 | `aarch64-apple-ios-sim`                                   | Xcode                  |
| iOS sim x64 | `x86_64-apple-ios`                                          | Xcode                  |
| MacCatalyst | `aarch64-apple-ios-macabi` `x86_64-apple-ios-macabi`        | Xcode                  |

---

## After setup

Restart Visual Studio (or your terminal) after installing Rust or setting environment variables — the IDE captures `PATH` and env vars at launch time.

Open `UseSkiaSharp.sln`, select a target, press **F5**.
