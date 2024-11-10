using ClientMaui.API;
using ClientMaui.Cryptography;
using ClientMaui.Entities;
using ClientMaui.Entities.Room;
using ClientMaui.Widgets;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RestSharp;
using SharpHook;
using SharpHook.Native;
using System.Net;

namespace ClientMaui;
public partial class ChatRoom : ContentPage
{
    private Endpoint endpoint;
    private Room room;
    private HubConnection? connection;

    private bool focus;
    private TaskPoolGlobalHook hook;

    private ICryptography? cypher;

    private readonly int oldMessageToLoadCount = 20;
    private int lastMessageCount = 0;
    private int? messageCount = null;
    private bool decryptMessages = true;

    public ChatRoom(Endpoint endpoint, Room room)
    {
        InitializeComponent();
        this.endpoint = endpoint;
        this.room = room;
        HeaderLabel.Text = room.name;

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
        List<KeyCode> keyListSend =
        [
            KeyCode.VcEnter,
            KeyCode.VcNumPadEnter
        ];

        if (keyListSend.Contains(args.Data.KeyCode) && focus)
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
                AddMessageToStack(messageObject);
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

        await CryptographyTask();


        lastMessageCount += oldMessageToLoadCount;
        var messages = await endpoint.Request(APIEndpoints.RoomEndpoints.MessageList, id: room.id, from: 0, to: oldMessageToLoadCount);
        if (messages.StatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var messageList = JsonConvert.DeserializeObject<List<Message>>(messages.Content);
        foreach (var message in messageList)
        {
            AddMessageToStack(message);
        }
        await Task.Delay(100);
        await ScrollView.ScrollToAsync(MessagesStack, ScrollToPosition.End,
                true);

    }

    private async Task CryptographyTask()
    {
        if (room.RoomType == "No Encryption") return;

        cypher = CryptographyHelper.GetCryptography(room.RoomType);

        if (cypher.GetType() == typeof(RSAInstance))
        {
            var RSAInstance = (RSAInstance)cypher;
            RSAInstance.endpoint = endpoint;
            RSAInstance.room = room;
            _ = await RSAInstance.GetOtherPublicKey();

            if (await RSAInstance.LoadKey()) return;
            var myNewPublicKey = RSAInstance.GenerateKey();
            var myPublicKeyJson = new Key
            {
                key = RSAInstance.Base64Encode(myNewPublicKey)
            };
            await endpoint.Request(APIEndpoints.RoomEndpoints.SetKey, body: JsonConvert.SerializeObject(myPublicKeyJson), method: Method.Post, id: room.id);
            return;

        }

        if (cypher.GetType() == typeof(RSAandAES))
        {
            var instance = (RSAandAES)cypher;
            var rsa = instance.rsa;
            rsa.endpoint = endpoint;
            rsa.room = room;
            _ = await rsa.GetOtherPublicKey();

            if (await rsa.LoadKey()) return;
            var myNewPublicKey = rsa.GenerateKey();
            var myPublicKeyJson = new Key
            {
                key = RSAInstance.Base64Encode(myNewPublicKey)
            };
            await endpoint.Request(APIEndpoints.RoomEndpoints.SetKey, body: JsonConvert.SerializeObject(myPublicKeyJson), method: Method.Post, id: room.id);
            return;
        }


        var key = (await endpoint.Request(APIEndpoints.RoomEndpoints.GetKey, id: room.id)).Content;
        if (string.IsNullOrEmpty(key))
        {
            key = cypher.GenerateKey();
            var keyJson = new Key
            {
                key = key
            };
            await endpoint.Request(APIEndpoints.RoomEndpoints.SetKey, body: JsonConvert.SerializeObject(keyJson), method: Method.Post, id: room.id);
        }
        key = key.Replace("\"", "");
        cypher.key = key;

    }

    private void AddMessageToStack(Message? message)
    {
        if (message == null) return;

        //check if MessageStack is empty
        MessageBubble? lastMessage = default;
        if (MessagesStack.Children.Count != 0)
        {
            lastMessage = MessagesStack.Children.OfType<MessageBubble>().Last();
        }
        var label = new MessageBubble(message);
        label.SetTimeSend(message.send_at);
        label.SetLastMessageSendTime(lastMessage?.timeSend);
        label.setMessageDecryptStatus(cypher, decryptMessages);
        MessagesStack.Children.Add(label);
    }


    private async Task<string> EncryptMessage(string message, BlockCypherMode mode)
    {
        return cypher == null ? message : await cypher.Encrypt(message, mode);
    }

    private async void SendButton_OnClicked(object? sender, EventArgs e)
    {
        var message = MessageEntry.Text;
        if (string.IsNullOrWhiteSpace(message))
            return;
        MessageEntry.Text = "";
        Message messageObject = new()
        {
            message = await EncryptMessage(message, room.BlockCypherMode ?? BlockCypherMode.None),
            sender = Authentication.Token,
            BlockCypherMode = room.BlockCypherMode
        };
        var response = await endpoint.Request(APIEndpoints.RoomEndpoints.SendMessage, body: JsonConvert.SerializeObject(messageObject), method: Method.Post, id: room.id);

    }

    private void SortMessageStack()
    {
        var elements = MessagesStack.Children.OfType<MessageBubble>().OrderBy(message => message.id).ToList();
        MessagesStack.Children.Clear();
        foreach (var element in elements)
        {
            MessagesStack.Children.Add(element);
        }
    }

    private async void LoadOldMessages()
    {
        var messages = await endpoint.Request(APIEndpoints.RoomEndpoints.MessageList, id: room.id, from: lastMessageCount, to: lastMessageCount + oldMessageToLoadCount);
        lastMessageCount += oldMessageToLoadCount;
        if (messages.StatusCode != HttpStatusCode.OK)
        {
            return;
        }
        var messageList = JsonConvert.DeserializeObject<List<Message>>(messages.Content);
        foreach (var message in messageList)
        {
            AddMessageToStack(message);
        }

        await ScrollView.ScrollToAsync(MessagesStack, ScrollToPosition.Start, true);
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
        if (e.ScrollY == 0 && messageCount > lastMessageCount)
        {
            LoadOldMessages();
        }
    }

    private void SwitchModeButton_OnClicked(object? sender, EventArgs e)
    {
        decryptMessages = !decryptMessages;
        foreach (var message in MessagesStack.Children.OfType<MessageBubble>())
        {
            message.setMessageDecryptStatus(cypher, decryptMessages);
        }

        SwitchModeButton.Text = decryptMessages ? "Klasický režim" : "Nedešifrovaně";

    }
}