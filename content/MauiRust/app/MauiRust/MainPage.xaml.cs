using System.Runtime.InteropServices;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace MauiRust;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        var info = e.Info;
        var rect = SKRect.Create(
            info.Width * 0.25f,
            info.Height * 0.25f,
            info.Width * 0.5f,
            info.Height * 0.5f);

        using (var rectPaint = new SKPaint
        {
            Color = SKColors.CornflowerBlue,
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
        })
        {
            canvas.DrawRect(rect, rectPaint);
        }

        var cx = rect.MidX;
        var cy = rect.MidY;
        var radius = MathF.Min(rect.Width, rect.Height) * 0.25f;

        // 0xAARRGGBB — opaque orange
        const uint colorArgb = 0xFFFF8800u;

        var rc = NativeMethods.rust_native_draw_circle(canvas.Handle, cx, cy, radius, colorArgb);
        if (rc != 0)
        {
            var errPtr = NativeMethods.rust_native_last_error();
            var msg = errPtr == IntPtr.Zero ? "(no detail)" : Marshal.PtrToStringUTF8(errPtr);
            System.Diagnostics.Debug.WriteLine($"[rust_native] draw_circle failed: rc={rc} msg={msg}");
        }
    }

    private static partial class NativeMethods
    {
        // "rust_native" maps per-platform:
        //   Windows      : rust_native.dll
        //   Android/Linux: librust_native.so
        //   macOS/MacCat : librust_native.dylib
        //   iOS device   : statically linked into the app binary (see csproj)
        private const string Lib = "rust_native";

        [LibraryImport(Lib)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial int rust_native_draw_circle(
            IntPtr canvas,
            float cx,
            float cy,
            float radius,
            uint colorArgb);

        [LibraryImport(Lib)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static partial IntPtr rust_native_last_error();
    }
}
