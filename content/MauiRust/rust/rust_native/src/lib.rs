//! rust_native — minimal Rust library called from a .NET MAUI app via P/Invoke.
//! Draws onto the same SkiaSharp `SKCanvas` the C# side is painting into.
//!
//! ## Non-iOS-device platforms (Windows / Android / MacCatalyst / iOS simulator)
//!
//! SkiaSharp ships `libSkiaSharp.{dll,so,dylib}` — Skia behind a flat C ABI.
//! The .NET process has already loaded that library, so we `dlopen` it from
//! Rust and the linker returns a handle to the same in-memory copy. We resolve
//! only the symbols we need at runtime and do not link against libSkiaSharp
//! at build time, keeping the crate buildable on any host.
//!
//! ## iOS device
//!
//! Apple forbids `dlopen` of arbitrary dynamic libraries. SkiaSharp on iOS is
//! statically linked into the app binary, so all `sk_*` symbols are present.
//! We declare them as plain `extern "C"` and the .NET iOS linker resolves them
//! at app link time. This crate is built as `staticlib` and pulled into the
//! final binary via `<NativeReference Kind=Static ForceLoad=True>` in the csproj.

use std::ffi::c_void;
use std::os::raw::{c_float, c_uint};
use std::sync::Mutex;

static LAST_ERROR: Mutex<Option<String>> = Mutex::new(None);

fn set_last_error(message: impl Into<String>) {
    *LAST_ERROR.lock().unwrap() = Some(message.into());
}

// =========================================================================
// Backend: dlopen-based (everything except iOS device)
// =========================================================================
#[cfg(any(
    not(target_os = "ios"),
    target_abi = "macabi",
    target_abi = "sim"
))]
mod backend {
    use super::*;
    use libloading::{Library, Symbol};
    use once_cell::sync::OnceCell;

    type SkPaintNew = unsafe extern "C" fn() -> *mut c_void;
    type SkPaintDelete = unsafe extern "C" fn(*mut c_void);
    type SkPaintSetAntialias = unsafe extern "C" fn(*mut c_void, bool);
    type SkPaintSetColor = unsafe extern "C" fn(*mut c_void, c_uint);
    type SkCanvasDrawCircle =
        unsafe extern "C" fn(*mut c_void, c_float, c_float, c_float, *const c_void);

    pub(crate) struct SkApi {
        _lib: Library,
        paint_new_fn: SkPaintNew,
        paint_delete_fn: SkPaintDelete,
        paint_set_antialias_fn: SkPaintSetAntialias,
        paint_set_color_fn: SkPaintSetColor,
        canvas_draw_circle_fn: SkCanvasDrawCircle,
    }

    unsafe impl Send for SkApi {}
    unsafe impl Sync for SkApi {}

    impl SkApi {
        pub(crate) fn paint_new(&self) -> *mut c_void {
            unsafe { (self.paint_new_fn)() }
        }
        pub(crate) fn paint_delete(&self, paint: *mut c_void) {
            unsafe { (self.paint_delete_fn)(paint) }
        }
        pub(crate) fn paint_set_antialias(&self, paint: *mut c_void, aa: bool) {
            unsafe { (self.paint_set_antialias_fn)(paint, aa) }
        }
        pub(crate) fn paint_set_color(&self, paint: *mut c_void, color: c_uint) {
            unsafe { (self.paint_set_color_fn)(paint, color) }
        }
        pub(crate) fn canvas_draw_circle(
            &self,
            canvas: *mut c_void,
            cx: c_float,
            cy: c_float,
            r: c_float,
            paint: *const c_void,
        ) {
            unsafe { (self.canvas_draw_circle_fn)(canvas, cx, cy, r, paint) }
        }
    }

    static SK_API: OnceCell<Result<SkApi, String>> = OnceCell::new();

    fn lib_candidates() -> &'static [&'static str] {
        #[cfg(target_os = "windows")]
        { &["libSkiaSharp.dll", "SkiaSharp.dll"] }
        #[cfg(any(
            target_os = "macos",
            all(target_os = "ios", target_abi = "macabi"),
            all(target_os = "ios", target_abi = "sim")
        ))]
        {
            &[
                "libSkiaSharp",
                "libSkiaSharp.framework/libSkiaSharp",
                "@rpath/libSkiaSharp.framework/libSkiaSharp",
                "libSkiaSharp.dylib",
                "@rpath/libSkiaSharp.dylib",
            ]
        }
        #[cfg(target_os = "android")]
        { &["libSkiaSharp.so"] }
        #[cfg(all(
            unix,
            not(any(
                target_os = "macos",
                target_os = "android",
                all(target_os = "ios", target_abi = "macabi"),
                all(target_os = "ios", target_abi = "sim")
            ))
        ))]
        { &["libSkiaSharp.so"] }
    }

    macro_rules! get_sym {
        ($lib:ident, $ty:ty, $name:literal) => {{
            let sym: Symbol<$ty> = $lib
                .get(concat!($name, "\0").as_bytes())
                .map_err(|e| format!("{}: {}", $name, e))?;
            *sym
        }};
    }

    unsafe fn load_api() -> Result<SkApi, String> {
        let mut last_err = String::new();
        for name in lib_candidates() {
            match Library::new(name) {
                Ok(lib) => {
                    let api = SkApi {
                        paint_new_fn: get_sym!(lib, SkPaintNew, "sk_paint_new"),
                        paint_delete_fn: get_sym!(lib, SkPaintDelete, "sk_paint_delete"),
                        paint_set_antialias_fn: get_sym!(lib, SkPaintSetAntialias, "sk_paint_set_antialias"),
                        paint_set_color_fn: get_sym!(lib, SkPaintSetColor, "sk_paint_set_color"),
                        canvas_draw_circle_fn: get_sym!(lib, SkCanvasDrawCircle, "sk_canvas_draw_circle"),
                        _lib: lib,
                    };
                    return Ok(api);
                }
                Err(e) => {
                    last_err = format!("{name}: {e}");
                }
            }
        }
        Err(format!("could not load libSkiaSharp ({last_err})"))
    }

    pub(crate) fn api() -> Result<&'static SkApi, &'static str> {
        let result = SK_API.get_or_init(|| unsafe { load_api() });
        match result {
            Ok(a) => Ok(a),
            Err(msg) => {
                *super::LAST_ERROR.lock().unwrap() = Some(msg.clone());
                Err("libSkiaSharp not loaded — see rust_native_last_error()")
            }
        }
    }
}

// =========================================================================
// Backend: static-link (iOS device)
// =========================================================================
#[cfg(all(
    target_os = "ios",
    not(any(target_abi = "macabi", target_abi = "sim"))
))]
mod backend {
    use super::*;

    extern "C" {
        fn sk_paint_new() -> *mut c_void;
        fn sk_paint_delete(paint: *mut c_void);
        fn sk_paint_set_antialias(paint: *mut c_void, antialias: bool);
        fn sk_paint_set_color(paint: *mut c_void, color: c_uint);
        fn sk_canvas_draw_circle(
            canvas: *mut c_void,
            cx: c_float,
            cy: c_float,
            radius: c_float,
            paint: *const c_void,
        );
    }

    pub(crate) struct SkApi;

    impl SkApi {
        pub(crate) fn paint_new(&self) -> *mut c_void { unsafe { sk_paint_new() } }
        pub(crate) fn paint_delete(&self, paint: *mut c_void) { unsafe { sk_paint_delete(paint) } }
        pub(crate) fn paint_set_antialias(&self, paint: *mut c_void, aa: bool) {
            unsafe { sk_paint_set_antialias(paint, aa) }
        }
        pub(crate) fn paint_set_color(&self, paint: *mut c_void, color: c_uint) {
            unsafe { sk_paint_set_color(paint, color) }
        }
        pub(crate) fn canvas_draw_circle(
            &self,
            canvas: *mut c_void,
            cx: c_float,
            cy: c_float,
            r: c_float,
            paint: *const c_void,
        ) {
            unsafe { sk_canvas_draw_circle(canvas, cx, cy, r, paint) }
        }
    }

    static SK_API: SkApi = SkApi;

    pub(crate) fn api() -> Result<&'static SkApi, &'static str> {
        Ok(&SK_API)
    }
}

// =========================================================================
// Public C ABI — entry points P/Invoked from .NET
// =========================================================================

/// Draw an antialiased filled circle onto an existing `SKCanvas`.
///
/// `canvas` is `SKCanvas.Handle` — a raw `sk_canvas_t*` inside the same process.
/// `color_argb` is packed 0xAARRGGBB matching `sk_color_t`.
/// Returns 0 on success, non-zero on failure (call `rust_native_last_error` for detail).
#[no_mangle]
pub extern "C" fn rust_native_draw_circle(
    canvas: *mut c_void,
    cx: c_float,
    cy: c_float,
    radius: c_float,
    color_argb: c_uint,
) -> i32 {
    if canvas.is_null() {
        set_last_error("canvas handle was null");
        return 1;
    }
    let api = match backend::api() {
        Ok(a) => a,
        Err(_) => return 2,
    };
    let paint = api.paint_new();
    if paint.is_null() {
        set_last_error("sk_paint_new returned null");
        return 3;
    }
    api.paint_set_antialias(paint, true);
    api.paint_set_color(paint, color_argb);
    api.canvas_draw_circle(canvas, cx, cy, radius, paint);
    api.paint_delete(paint);
    0
}

/// Returns a pointer to a NUL-terminated UTF-8 string describing the most
/// recent error, or NULL if there is none. The string is owned by the library;
/// do not free it. Valid until the next call into this crate.
#[no_mangle]
pub extern "C" fn rust_native_last_error() -> *const u8 {
    let guard = LAST_ERROR.lock().unwrap();
    match guard.as_ref() {
        None => std::ptr::null(),
        Some(s) => {
            let mut bytes = s.as_bytes().to_vec();
            bytes.push(0);
            Box::leak(bytes.into_boxed_slice()).as_ptr()
        }
    }
}
