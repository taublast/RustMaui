using System.Runtime.InteropServices;

namespace MauiRust;

// P/Invoke bindings for the Rust native library.
// Add new #[no_mangle] exports in rust/lib.rs, then declare them here.
public static partial class Rust
{
    // "mauirustnativelib_native" maps per-platform:
    //   Windows      : mauirustnativelib_native.dll
    //   Android/Linux: libmauirustnativelib_native.so
    //   macOS/MacCat : libmauirustnativelib_native.dylib
    //   iOS device   : statically linked into the app binary (see csproj)
    private const string Lib = "mauirustnativelib_native";

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial int mauirustnativelib_add(int a, int b);
}
