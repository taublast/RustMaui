using System.Runtime.InteropServices;

namespace MauiRust;

public partial class MainPage : ContentPage
{
    private string _result = "–";
    public string Result
    {
        get => _result;
        private set { _result = value; OnPropertyChanged(); }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void OnInputChanged(object? sender, TextChangedEventArgs e)
    {
        if (!int.TryParse(EntryA.Text, out var a) || !int.TryParse(EntryB.Text, out var b))
        {
            Result = "–";
            return;
        }
        Result = NativeMethods.mauirustnativelib_add(a, b).ToString();
    }

    private static partial class NativeMethods
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
}
