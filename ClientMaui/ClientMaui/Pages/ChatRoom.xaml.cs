using ClientMaui.API;
using ClientMaui.Entities.Room;
using ClientMaui.Widgets;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using SharpHook;
using SharpHook.Native;


namespace ClientMaui;

public partial class ChatRoom : ContentPage
{
    private Endpoint endpoint;
    private Room room;
    private HubConnection? connection;

    private bool focus;
    public ChatRoom(Endpoint endpoint, Room room)
    {
        InitializeComponent();
        this.endpoint = endpoint;
        this.room = room;

        var hook = new TaskPoolGlobalHook();
        hook.KeyPressed += KeyPressed;
        hook.RunAsync();

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
                   
                }


                )
            .Build();
        
        connection.On<string>("ReceiveMessage", message =>
        {
            var messageObject = JsonConvert.DeserializeObject<Message>(message);
            Dispatcher.Dispatch(() =>
            {
                var label = new MessageBubble
                {
                    MessageText = messageObject.message,
                    IsIncoming = messageObject.sender != Authentication.Token,
                };
                MessagesStack.Children.Add(label);
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
        var messages = await endpoint.Request(APIEndpoints.RoomEndpoints.MessageList, id: room.id);
        if (messages.StatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var messageList = JsonConvert.DeserializeObject<List<Message>>(messages.Content);

    }

    private async void SendButton_OnClicked(object? sender, EventArgs e)
    {
        var message = MessageEntry.Text;
        MessageEntry.Text = "";
        var messageObject = new Message
        {
            message = message,
            sender = Authentication.Token
        };
        var response = await endpoint.Request(APIEndpoints.RoomEndpoints.SendMessage, body: JsonConvert.SerializeObject(messageObject), method: Method.Post, id: room.id);

    }

    private void MessageEntry_OnFocused(object? sender, FocusEventArgs e)
    {
        focus = true;

    }

    private void MessageEntry_OnUnfocused(object? sender, FocusEventArgs e)
    {
        focus = false;
    }
}