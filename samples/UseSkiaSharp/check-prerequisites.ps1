# check-prerequisites.ps1 -- reports missing tools needed to build UseSkiaSharp.
# Run from any directory. Does NOT install anything.
#
# Usage:
#   .\check-prerequisites.ps1
#   powershell -ExecutionPolicy Bypass -File check-prerequisites.ps1

$ok   = 0
$fail = 0

function Pass($msg) {
    Write-Host "  [OK]  $msg" -ForegroundColor Green
    $script:ok++
}

function Miss($msg, $fix) {
    Write-Host "  [!!]  $msg" -ForegroundColor Red
    Write-Host "        => $fix" -ForegroundColor Yellow
    $script:fail++
}

function Section($title) {
    Write-Host ""
    Write-Host "-- $title" -ForegroundColor Cyan
}

# -- .NET ---------------------------------------------------------------------
Section ".NET"

$dotnetVer = dotnet --version 2>$null
if ($dotnetVer -match '^10\.') {
    Pass ".NET 10 SDK  ($dotnetVer)"
} elseif ($dotnetVer) {
    Miss ".NET 10 SDK  (found $dotnetVer, need 10.x)" `
         "https://dotnet.microsoft.com/download/dotnet/10.0"
} else {
    Miss ".NET 10 SDK  (not found)" `
         "https://dotnet.microsoft.com/download/dotnet/10.0"
}

$workloads = dotnet workload list 2>$null | Out-String
if ($workloads -match 'maui') {
    Pass "MAUI workloads"
} else {
    Miss "MAUI workloads" "dotnet workload install maui"
}

# -- Rust ---------------------------------------------------------------------
Section "Rust"

$cargo = Get-Command cargo -ErrorAction SilentlyContinue
if ($cargo) {
    Pass "cargo  ($(cargo --version 2>$null))"
} else {
    Miss "cargo  (not found)" `
         "Download rustup-init.exe from https://rustup.rs and run it"
}

$rustup = Get-Command rustup -ErrorAction SilentlyContinue
if ($rustup) {
    Pass "rustup  ($(rustup --version 2>$null))"
} else {
    Miss "rustup  (not found)" `
         "Download rustup-init.exe from https://rustup.rs and run it"
}

# -- Rust targets -------------------------------------------------------------
if ($rustup) {
    Section "Rust targets"
    $installed = rustup target list --installed 2>$null

    function ChkTarget($triple) {
        if ($installed -contains $triple) {
            Pass $triple
        } else {
            Miss "$triple  (not installed)" "rustup target add $triple"
        }
    }

    ChkTarget "x86_64-pc-windows-msvc"
    ChkTarget "aarch64-linux-android"
    ChkTarget "x86_64-linux-android"
}

# -- Android ------------------------------------------------------------------
Section "Android"

$cargoNdk = Get-Command cargo-ndk -ErrorAction SilentlyContinue
if ($cargoNdk) {
    Pass "cargo-ndk  ($(cargo ndk --version 2>$null))"
} else {
    Miss "cargo-ndk  (not found)" "cargo install cargo-ndk"
}

$ndkHome = $env:ANDROID_NDK_HOME
if ($ndkHome) {
    if (Test-Path $ndkHome) {
        Pass "ANDROID_NDK_HOME  ($ndkHome)"
    } else {
        Miss "ANDROID_NDK_HOME set but directory not found  ($ndkHome)" `
             "Install NDK via Android Studio -> SDK Manager -> SDK Tools -> NDK (Side by side)"
    }
} else {
    Miss "ANDROID_NDK_HOME  (not set)" `
         "Set it as a user environment variable -- see Prerequisites.md"
}

# -- MSVC linker --------------------------------------------------------------
# Full Visual Studio is NOT required. VS Build Tools (free, no IDE) is enough.
# Download: https://visualstudio.microsoft.com/downloads/
#   -> "Build Tools for Visual Studio" -> Desktop development with C++
Section "MSVC linker  (Visual Studio or Build Tools)"

$vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vsWhere) {
    $vsPath = & $vsWhere -latest -property installationPath 2>$null
    if ($vsPath) {
        $clPath = Join-Path $vsPath "VC\Tools\MSVC"
        if (Test-Path $clPath) {
            Pass "MSVC tools found  ($vsPath)"
        } else {
            Miss "MSVC tools missing  (VS/Build Tools found but no C++ workload)" `
                 "Run VS Installer -> Modify -> add 'Desktop development with C++'"
        }
    } else {
        Miss "MSVC tools  (installer present but nothing installed)" `
             "Install VS Build Tools + 'Desktop development with C++': https://visualstudio.microsoft.com/downloads/"
    }
} else {
    Miss "MSVC tools  (not found)" `
         "Install VS Build Tools + 'Desktop development with C++': https://visualstudio.microsoft.com/downloads/"
}

# -- Summary ------------------------------------------------------------------
Write-Host ""
Write-Host "---------------------------------------------" -ForegroundColor Cyan
if ($fail -eq 0) {
    Write-Host "  All prerequisites found." -ForegroundColor Green
} else {
    Write-Host "  $fail item(s) missing -- see fix commands above." -ForegroundColor Red
}
Write-Host ""
