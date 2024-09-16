using ClientMaui.API;
using ClientMaui.Entities.Room;
using ClientMaui.Widgets;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RestSharp;
using SharpHook;
using SharpHook.Native;
using System.Net;
using System.Security.Cryptography;

namespace ClientMaui;
public partial class ChatRoom : ContentPage
{
    private Endpoint endpoint;
    private Room room;
    private HubConnection? connection;

    private bool focus;
    private TaskPoolGlobalHook hook;
    
    private readonly int oldMessageToLoadCount = 20;
    private int lastMessageCount = 0;
    private int? messageCount = null;
    
    public ChatRoom(Endpoint endpoint, Room room)
    {
        InitializeComponent();
        this.endpoint = endpoint;
        this.room = room;

        hook = new TaskPoolGlobalHook();
        hook.KeyPressed += KeyPressed;
        hook.RunAsync();
       

    }

    protected override void OnDisappearing()
    {
        hook.Dispose();
        base.OnDisappearing();
    }


    private void KeyPressed(object? obj, KeyboardHookEventArgs args)
    {
        if (args.Data.KeyCode == KeyCode.VcEnter && focus)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SendButton_OnClicked(null, null);
            });

        }
    }


    protected override async void OnAppearing()
    {
        ConnectToRoom();
        connection = new HubConnectionBuilder()
            .WithUrl($"{endpoint.url}/new-message",
                (opts) =>
                {
                    opts.Headers["Authorization"] = $"Bearer {Authentication.Token}";
                    opts.HttpMessageHandlerFactory = message =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // always verify the SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (_, _, _, _) => true;
                        return message;
                    };

                })
            .Build();

        connection.On<string>("ReceiveMessage", async message =>
        {
            var messageObject = JsonConvert.DeserializeObject<Message>(message);
            Dispatcher.Dispatch(() =>
            {
                if (messageObject?.message == null) return;
                var label = new MessageBubble(messageObject.id)
                {
                    MessageText = messageObject?.message,
                    IsIncoming = messageObject.sender != Authentication.Token,
                };
                MessagesStack.Children.Add(label);
            });
            await Task.Delay(100);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ScrollView.ScrollToAsync(MessagesStack, ScrollToPosition.End,
                    true);
            });
        });
        await connection.StartAsync();
    }

    private async void ConnectToRoom()
    {
        var response = await endpoint.Request(APIEndpoints.RoomEndpoints.Connect, id: room.id);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            await DisplayAlert("Error", "Error occured while connecting to server, check IP address and port!", "OK");
            return;
        }
        if (response.Content != null) messageCount = int.Parse(response.Content);
    
        lastMessageCount += oldMessageToLoadCount;
        var messages = await endpoint.Request(APIEndpoints.RoomEndpoints.MessageList, id: room.id, from: 0, to: oldMessageToLoadCount);
        if (messages.StatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var messageList = JsonConvert.DeserializeObject<List<Message>>(messages.Content);
        foreach (var label in messageList.Select(message => new MessageBubble(message.id)
                 {
                     MessageText = message.message,
                     IsIncoming = message.sender != Authentication.Token,
                 }))
        {
            MessagesStack.Children.Add(label);
        }
        await Task.Delay(100);
        await ScrollView.ScrollToAsync(MessagesStack, ScrollToPosition.End,
                true);

    }

    private async void SendButton_OnClicked(object? sender, EventArgs e)
    {
        var message = MessageEntry.Text;
        MessageEntry.Text = "";
        Message messageObject = new()
        {
            message = message,
            sender = Authentication.Token
        };
        var response = await endpoint.Request(APIEndpoints.RoomEndpoints.SendMessage, body: JsonConvert.SerializeObject(messageObject), method: Method.Post, id: room.id);

    }

    private void SortMessageStack()
    {
        var elements = MessagesStack.Children.OfType<MessageBubble>().OrderByDescending(message => message.id).ToList();
        MessagesStack.Children.Clear();
        foreach (var element in elements)
        {
            MessagesStack.Children.Add(element);
        }
    }

    private async void LoadOldMessages()
    {
        var messages = await endpoint.Request(APIEndpoints.RoomEndpoints.MessageList, id: room.id, from: lastMessageCount, to: lastMessageCount  + oldMessageToLoadCount);
        lastMessageCount += oldMessageToLoadCount;
        if (messages.StatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var messageList = JsonConvert.DeserializeObject<List<Message>>(messages.Content);
        var firstMessage = MessagesStack.Children.OfType<MessageBubble>().First();
        foreach (var label in messageList.Select(message => new MessageBubble(message.id)
        {
            MessageText = message.message,
            IsIncoming = message.sender != Authentication.Token,
        }))
        {
            MessagesStack.Children.Add(label);
        }

        ScrollView.ScrollToAsync(firstMessage, ScrollToPosition.Start,
            true);
        SortMessageStack();
    }

    private void MessageEntry_OnFocused(object? sender, FocusEventArgs e)
    {
        focus = true;
    }

    private void MessageEntry_OnUnfocused(object? sender, FocusEventArgs e)
    {
        focus = false;
    }

    private void ScrollView_OnScrolled(object? sender, ScrolledEventArgs e)
    {
        if(e.ScrollY == 0 && messageCount > lastMessageCount)
        {
            LoadOldMessages();
        }
    }
}