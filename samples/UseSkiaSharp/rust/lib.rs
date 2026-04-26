//! useskiasharp_native — Rust library called from a .NET MAUI app via P/Invoke.
//!
//! The C# side draws a rectangle; this crate draws a circle onto the same
//! `SKCanvas` by calling into the libSkiaSharp binary the .NET runtime already
//! loaded in the same process.

use std::ffi::c_void;
use std::os::raw::{c_float, c_uint};
use std::sync::Mutex;

mod skia;

static LAST_ERROR: Mutex<Option<String>> = Mutex::new(None);

#[no_mangle]
pub extern "C" fn draw_circle(
    canvas: *mut c_void,
    cx: c_float,
    cy: c_float,
    radius: c_float,
    color_argb: c_uint,
) -> i32 {
    match skia::draw_circle(canvas, cx, cy, radius, color_argb) {
        Ok(()) => {
            *LAST_ERROR.lock().unwrap() = None;
            0
        }
        Err((code, message)) => {
            *LAST_ERROR.lock().unwrap() = Some(message);
            code
        }
    }
}

#[no_mangle]
pub extern "C" fn last_error_message(buffer: *mut u8, buffer_len: usize) -> usize {
    let guard = LAST_ERROR.lock().unwrap();
    let Some(message) = guard.as_ref() else {
        return 0;
    };

    let bytes = message.as_bytes();
    let required_len = bytes.len() + 1;

    if !buffer.is_null() && buffer_len != 0 {
        let copy_len = bytes.len().min(buffer_len.saturating_sub(1));
        unsafe {
            std::ptr::copy_nonoverlapping(bytes.as_ptr(), buffer, copy_len);
            *buffer.add(copy_len) = 0;
        }
    }

    required_len
}
