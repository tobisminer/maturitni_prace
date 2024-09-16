using System.Globalization;

namespace ClientMaui.Widgets;

public partial class MessageBubble : ContentView
{
    public int id;

    public static readonly BindableProperty MessageTextProperty =
        BindableProperty.Create(nameof(MessageText), typeof(string), typeof(MessageBubble), string.Empty);

    public static readonly BindableProperty IsIncomingProperty =
        BindableProperty.Create(nameof(IsIncoming), typeof(bool), typeof(MessageBubble), true);


    public string MessageText
    {
        get => (string)GetValue(MessageTextProperty);
        set => SetValue(MessageTextProperty, value);
    }

    public bool IsIncoming
    {
        get => (bool)GetValue(IsIncomingProperty);
        set => SetValue(IsIncomingProperty, value);
    }

    public MessageBubble(int id)
    {
        InitializeComponent();
        this.id = id;
        BindingContext = this;
        HorizontalOptions = IsIncoming ? LayoutOptions.Start : LayoutOptions.End;
        Frame.BackgroundColor = IsIncoming ? Colors.LightBlue : Colors.Red;
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName != nameof(IsIncoming)) return;
        HorizontalOptions = IsIncoming ? LayoutOptions.Start : LayoutOptions.End;
        Frame.BackgroundColor = IsIncoming ? Colors.LightBlue : Colors.Red;
    }
}
public class BoolToHorizontalOptionsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? LayoutOptions.Start : LayoutOptions.End;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}