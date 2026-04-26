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
        Result = Rust.mauirustnativelib_add(a, b).ToString();
    }
}
