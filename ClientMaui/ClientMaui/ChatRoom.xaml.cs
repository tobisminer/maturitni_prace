using System.Net;
using ClientMaui.API;
using ClientMaui.Entities.Room;
using ClientMaui.Widgets;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RestSharp;


namespace ClientMaui;

public partial class ChatRoom : ContentPage
{
    private Endpoint endpoint;
	private Room room;
	private HubConnection? connection;
    private string? token;
    public ChatRoom(Endpoint endpoint, Room room)
	{
		InitializeComponent();
		this.endpoint = endpoint;
		this.room = room;
	}

    protected override async void OnAppearing()
    {
		connection = new HubConnectionBuilder()
            .WithUrl($"{endpoint.url}/new-message",
                (opts) =>
                {
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

        connection.On<string>("ReceiveId", ConnectToRoom);

		connection.On<string>("ReceiveMessage", message =>
        {
            var messageObject = JsonConvert.DeserializeObject<Message>(message);
            Dispatcher.Dispatch(() =>
            {
                var label = new MessageBubble
                {
                    MessageText = messageObject.message,
                    IsIncoming = messageObject.sender != token,
                };
                MessagesStack.Children.Add(label);
            });

        });
        await connection.StartAsync();

    }

    private async void ConnectToRoom(string? roomToken)
    {
        token = roomToken;
        var response = await endpoint.Request(APIEndpoints.RoomEndpoints.Connect, id: room.id, identification: roomToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            await DisplayAlert("Error", "Error occured while connecting to server, check IP address and port!", "OK");
            return;
        }
        var messages = await endpoint.Request(APIEndpoints.RoomEndpoints.MessageList, id:room.id, identification: roomToken);
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
            sender = token
        };
        var response = await endpoint.Request(APIEndpoints.RoomEndpoints.SendMessage, body: JsonConvert.SerializeObject(messageObject), method: Method.Post, id: room.id, identification: token);

    }
}