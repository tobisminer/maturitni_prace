namespace ClientMaui.Widgets;

public partial class MessageBubble : ContentView
{
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

    public MessageBubble()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName != nameof(IsIncoming)) return;
        BackgroundColor = IsIncoming ? Colors.LightGray : Colors.Blue;
        HorizontalOptions = IsIncoming ? LayoutOptions.Start : LayoutOptions.End;
    }
}