using System.Runtime.InteropServices;

namespace TestApp;

// P/Invoke bindings for the Rust native library.
// Add new #[no_mangle] exports in rust/lib.rs, then declare them here.
public static partial class Rust
{
    // "testapp_native" maps per-platform:
    //   Windows      : testapp_native.dll
    //   Android/Linux: libtestapp_native.so
    //   macOS/MacCat : libtestapp_native.dylib
    //   iOS device   : statically linked into the app binary (see csproj)
    private const string Lib = "testapp_native";

    [LibraryImport(Lib, EntryPoint = "add")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial int Add(int a, int b);
}
