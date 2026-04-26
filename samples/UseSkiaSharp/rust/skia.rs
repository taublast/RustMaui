use std::ffi::c_void;
use std::os::raw::{c_float, c_uint};

#[cfg(any(
    not(target_os = "ios"),
    target_abi = "macabi"
))]
mod backend {
    use super::*;
    use libloading::{Library, Symbol};
    use once_cell::sync::OnceCell;

    type FnPaintNew = unsafe extern "C" fn() -> *mut c_void;
    type FnPaintDelete = unsafe extern "C" fn(*mut c_void);
    type FnPaintSetAntialias = unsafe extern "C" fn(*mut c_void, bool);
    type FnPaintSetColor = unsafe extern "C" fn(*mut c_void, c_uint);
    type FnDrawCircle = unsafe extern "C" fn(*mut c_void, c_float, c_float, c_float, *const c_void);

    pub(crate) struct SkApi {
        _lib: Library,
        paint_new: FnPaintNew,
        paint_delete: FnPaintDelete,
        paint_set_antialias: FnPaintSetAntialias,
        paint_set_color: FnPaintSetColor,
        draw_circle: FnDrawCircle,
    }

    unsafe impl Send for SkApi {}
    unsafe impl Sync for SkApi {}

    impl SkApi {
        pub(crate) fn paint_new(&self) -> *mut c_void { unsafe { (self.paint_new)() } }
        pub(crate) fn paint_delete(&self, paint: *mut c_void) { unsafe { (self.paint_delete)(paint) } }
        pub(crate) fn paint_set_antialias(&self, paint: *mut c_void, antialias: bool) { unsafe { (self.paint_set_antialias)(paint, antialias) } }
        pub(crate) fn paint_set_color(&self, paint: *mut c_void, color: c_uint) { unsafe { (self.paint_set_color)(paint, color) } }
        pub(crate) fn draw_circle(&self, canvas: *mut c_void, cx: c_float, cy: c_float, radius: c_float, paint: *const c_void) {
            unsafe { (self.draw_circle)(canvas, cx, cy, radius, paint) }
        }
    }

    static SK_API: OnceCell<Result<SkApi, String>> = OnceCell::new();

    fn lib_candidates() -> &'static [&'static str] {
        #[cfg(target_os = "windows")]
        { &["libSkiaSharp.dll", "SkiaSharp.dll"] }
        #[cfg(any(
            target_os = "macos",
                    all(target_os = "ios", target_abi = "macabi")
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
                all(target_os = "ios", target_abi = "macabi")
            ))
        ))]
        { &["libSkiaSharp.so"] }
    }

    macro_rules! sym {
        ($lib:ident, $ty:ty, $name:literal) => {{
            let symbol: Symbol<$ty> = $lib
                .get(concat!($name, "\0").as_bytes())
                .map_err(|error| format!("{}: {}", $name, error))?;
            *symbol
        }};
    }

    unsafe fn load() -> Result<SkApi, String> {
        let mut last_error = String::new();
        for name in lib_candidates() {
            match Library::new(name) {
                Ok(lib) => {
                    return Ok(SkApi {
                        paint_new: sym!(lib, FnPaintNew, "sk_paint_new"),
                        paint_delete: sym!(lib, FnPaintDelete, "sk_paint_delete"),
                        paint_set_antialias: sym!(lib, FnPaintSetAntialias, "sk_paint_set_antialias"),
                        paint_set_color: sym!(lib, FnPaintSetColor, "sk_paint_set_color"),
                        draw_circle: sym!(lib, FnDrawCircle, "sk_canvas_draw_circle"),
                        _lib: lib,
                    });
                }
                Err(error) => last_error = format!("{name}: {error}"),
            }
        }
        Err(format!("could not load libSkiaSharp ({last_error})"))
    }

    pub(crate) fn api() -> Result<&'static SkApi, String> {
        match SK_API.get_or_init(|| unsafe { load() }) {
            Ok(api) => Ok(api),
            Err(message) => Err(message.clone()),
        }
    }
}

#[cfg(all(target_os = "ios", not(target_abi = "macabi")))]
mod backend {
    use super::*;

    extern "C" {
        fn sk_paint_new() -> *mut c_void;
        fn sk_paint_delete(paint: *mut c_void);
        fn sk_paint_set_antialias(paint: *mut c_void, antialias: bool);
        fn sk_paint_set_color(paint: *mut c_void, color: c_uint);
        fn sk_canvas_draw_circle(canvas: *mut c_void, cx: c_float, cy: c_float, radius: c_float, paint: *const c_void);
    }

    pub(crate) struct SkApi;

    static SK_API: SkApi = SkApi;

    impl SkApi {
        pub(crate) fn paint_new(&self) -> *mut c_void { unsafe { sk_paint_new() } }
        pub(crate) fn paint_delete(&self, paint: *mut c_void) { unsafe { sk_paint_delete(paint) } }
        pub(crate) fn paint_set_antialias(&self, paint: *mut c_void, antialias: bool) { unsafe { sk_paint_set_antialias(paint, antialias) } }
        pub(crate) fn paint_set_color(&self, paint: *mut c_void, color: c_uint) { unsafe { sk_paint_set_color(paint, color) } }
        pub(crate) fn draw_circle(&self, canvas: *mut c_void, cx: c_float, cy: c_float, radius: c_float, paint: *const c_void) {
            unsafe { sk_canvas_draw_circle(canvas, cx, cy, radius, paint) }
        }
    }

    pub(crate) fn api() -> Result<&'static SkApi, String> {
        Ok(&SK_API)
    }
}

pub(crate) fn draw_circle(
    canvas: *mut c_void,
    cx: c_float,
    cy: c_float,
    radius: c_float,
    color_argb: c_uint,
) -> Result<(), (i32, String)> {
    if canvas.is_null() {
        return Err((1, "canvas handle was null".into()));
    }

    let api = backend::api().map_err(|message| (2, message))?;
    let paint = api.paint_new();
    if paint.is_null() {
        return Err((3, "sk_paint_new returned null".into()));
    }

    api.paint_set_antialias(paint, true);
    api.paint_set_color(paint, color_argb);
    api.draw_circle(canvas, cx, cy, radius, paint);
    api.paint_delete(paint);

    Ok(())
}