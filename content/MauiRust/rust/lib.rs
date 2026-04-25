//! mauirustnativelib_native — Rust library called from a .NET MAUI app via P/Invoke.
//!
//! Add your own #[no_mangle] pub extern "C" functions here and declare matching
//! [LibraryImport] P/Invoke signatures in MainPage.xaml.cs (or wherever in C#).
//! MSBuild compiles and links this crate automatically on every dotnet build / F5.

use std::os::raw::c_int;

/// Returns the sum of `a` and `b`.
/// Called from C# via P/Invoke — see NativeMethods in MainPage.xaml.cs.
#[no_mangle]
pub extern "C" fn mauirustnativelib_add(a: c_int, b: c_int) -> c_int {
    a + b
}
