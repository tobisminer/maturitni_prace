using ClientMaui.Cryptography;
using ClientMaui.Entities.Room;

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

    private Message message;

    public MessageBubble(Message message)
    {
        InitializeComponent();
        BindingContext = this;
        this.id = message.id;
        this.message = message;
        MessageText = message.message;
        timeSend = message.send_at;
        IsIncoming = message.sender == null;
        SetValues();
    }

    public async void setMessageDecryptStatus(ICryptography? cypher, bool decrypt = true)
    {
        MessageText = decrypt && cypher != null ? await cypher.Decrypt(message.message, message.BlockCypherMode ?? BlockCypherMode.None, message.sender == null) : message.message;
    }

    private void SetValues()
    {
        HorizontalOptions = IsIncoming ? LayoutOptions.Start : LayoutOptions.End;
        Frame.BackgroundColor = IsIncoming ? Colors.LightBlue : Colors.Red;
        if (lastMessageSend != null &&
            sameTimeToMinutes(timeSend, lastMessageSend.Value))
        {
            TimeLbl.IsVisible = false;
            return;
        }
        if (lastMessageSend != null)
            TimeLbl.Text = getTimeFirstString(timeSend, lastMessageSend.Value);
    }

    public void SetTimeSend(DateTime timeSend)
    {
        this.timeSend = timeSend;
        SetValues();
    }
    public void SetLastMessageSendTime(DateTime? lastMessageSend)
    {
        this.lastMessageSend = lastMessageSend;
        SetValues();
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

    public bool sameTimeToDay(DateTime first, DateTime second)
    {
        return first.Day == second.Day;
    }

    private string getTimeFirstString(DateTime first, DateTime? second)
    {
        return second == null
            ? first.ToString("f")
            : sameTimeToMinutes(first, second.Value) ? "" : first.ToString(sameTimeToDay(first, second.Value) ? "t" : "f");
    }
}

