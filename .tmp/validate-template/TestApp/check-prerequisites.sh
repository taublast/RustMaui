#!/usr/bin/env bash
# check-prerequisites.sh — reports missing tools needed to build TestApp.
# Run from any directory. Does NOT install anything.

ok=0
fail=0

pass() { echo "  ✓  $1"; ok=$((ok+1)); }
miss() { echo "  ✗  $1"; echo "     → $2"; fail=$((fail+1)); }
section() { echo ""; echo "── $1"; }

# ── .NET ──────────────────────────────────────────────────────────────────────
section ".NET"

dotnet_ver=$(dotnet --version 2>/dev/null || true)
if echo "$dotnet_ver" | grep -q '^10\.'; then
    pass ".NET 10 SDK  ($dotnet_ver)"
elif [ -n "$dotnet_ver" ]; then
    miss ".NET 10 SDK  (found $dotnet_ver, need 10.x)" \
         "https://dotnet.microsoft.com/download/dotnet/10.0"
else
    miss ".NET 10 SDK  (not found)" \
         "https://dotnet.microsoft.com/download/dotnet/10.0"
fi

if dotnet workload list 2>/dev/null | grep -q 'maui'; then
    pass "MAUI workloads"
else
    miss "MAUI workloads" "dotnet workload install maui"
fi

# ── Rust ──────────────────────────────────────────────────────────────────────
section "Rust"

if command -v cargo >/dev/null 2>&1; then
    pass "cargo  ($(cargo --version 2>/dev/null))"
else
    miss "cargo  (not found)" \
         "curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh"
fi

if command -v rustup >/dev/null 2>&1; then
    pass "rustup  ($(rustup --version 2>/dev/null | head -1))"
else
    miss "rustup  (not found)" \
         "curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh"
fi

# ── Rust targets ──────────────────────────────────────────────────────────────
if command -v rustup >/dev/null 2>&1; then
    section "Rust targets"
    installed=$(rustup target list --installed 2>/dev/null)

    chk() {
        if echo "$installed" | grep -qx "$1"; then
            pass "$1"
        else
            miss "$1  (not installed)" "rustup target add $1"
        fi
    }

    chk "aarch64-linux-android"
    chk "x86_64-linux-android"

    if [ "$(uname)" = "Darwin" ]; then
        chk "aarch64-apple-ios"
        chk "aarch64-apple-ios-sim"
        chk "x86_64-apple-ios"
        chk "aarch64-apple-ios-macabi"
        chk "x86_64-apple-ios-macabi"
    fi
fi

# ── Android ───────────────────────────────────────────────────────────────────
section "Android"

if command -v cargo-ndk >/dev/null 2>&1; then
    pass "cargo-ndk  ($(cargo ndk --version 2>/dev/null))"
else
    miss "cargo-ndk  (not found)" "cargo install cargo-ndk"
fi

if [ -n "${ANDROID_NDK_HOME:-}" ]; then
    if [ -d "$ANDROID_NDK_HOME" ]; then
        pass "ANDROID_NDK_HOME  ($ANDROID_NDK_HOME)"
    else
        miss "ANDROID_NDK_HOME set but directory not found  ($ANDROID_NDK_HOME)" \
             "Install NDK via Android Studio → SDK Manager → SDK Tools → NDK (Side by side)"
    fi
else
    miss "ANDROID_NDK_HOME  (not set)" \
         "Set it to your NDK directory — see Prerequisites.md"
fi

# ── Xcode (macOS only) ────────────────────────────────────────────────────────
if [ "$(uname)" = "Darwin" ]; then
    section "Xcode  (iOS / macCatalyst — macOS only)"

    if xcode_path=$(xcode-select -p 2>/dev/null); then
        pass "Xcode command-line tools  ($xcode_path)"
    else
        miss "Xcode command-line tools  (not found)" "xcode-select --install"
    fi

    if command -v xcodebuild >/dev/null 2>&1; then
        pass "xcodebuild  ($(xcodebuild -version 2>/dev/null | head -1))"
    else
        miss "xcodebuild  (not found)" "Install Xcode from the App Store"
    fi
fi

# ── Summary ───────────────────────────────────────────────────────────────────
echo ""
echo "─────────────────────────────────────────"
if [ "$fail" -eq 0 ]; then
    echo "  ✓  All prerequisites found."
else
    echo "  ✗  $fail item(s) missing — see fix commands above."
fi
echo ""
