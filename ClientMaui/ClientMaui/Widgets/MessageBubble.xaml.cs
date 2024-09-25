using System.Globalization;
using System.Runtime.InteropServices.JavaScript;

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

    public DateTime timeSend;
    public DateTime? lastMessageSend;

    public MessageBubble(int id)
    {
        InitializeComponent();
        this.id = id;
        BindingContext = this;
        SetValues();
    }

    private void SetValues()
    {
        HorizontalOptions = IsIncoming ? LayoutOptions.Start : LayoutOptions.End;
        Frame.BackgroundColor = IsIncoming ? Colors.LightBlue : Colors.Red;
        if (lastMessageSend != null && sameTimeToMinutes(timeSend, lastMessageSend.Value))
            TimeLbl.IsEnabled = false;
        TimeLbl.Text = timeSend.ToString("f");
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName != nameof(IsIncoming)) return;
        SetValues();
    }

    public bool sameTimeToMinutes(DateTime first, DateTime second)
    {
       return first.Day == second.Day && first.Hour == second.Hour &&
               first.Minute == second.Minute;
    }
}

