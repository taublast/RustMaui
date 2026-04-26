using System.Runtime.InteropServices;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace UseSkiaSharp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    const uint ColorArgb = 0xFFFF8800u;
    private SKPaint? _rectPaint = null;

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

        _rectPaint ??= new SKPaint
            {
                Color = SKColors.CornflowerBlue,
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            };

        //MAUI
        canvas.DrawRect(rect, _rectPaint);

        var cx = rect.MidX;
        var cy = rect.MidY;
        var radius = MathF.Min(rect.Width, rect.Height) * 0.25f;
        
        //RUST
        int rc = Rust.DrawCircle(canvas.Handle, cx, cy, radius, ColorArgb);

        if (rc != 0)
        {
            var required = Rust.LastErrorMessage(IntPtr.Zero, 0);
            if (required == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Rust] DrawCircle failed: rc={rc} msg=(no detail)");
                return;
            }

            var buffer = Marshal.AllocHGlobal(checked((int)required));
            try
            {
                Rust.LastErrorMessage(buffer, required);
                var msg = Marshal.PtrToStringUTF8(buffer) ?? "(invalid utf8)";
                System.Diagnostics.Debug.WriteLine($"[Rust] DrawCircle failed: rc={rc} msg={msg}");
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
