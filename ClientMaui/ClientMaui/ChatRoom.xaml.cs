using System.Net;
using ClientMaui.API;
using ClientMaui.Entities.Room;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RestSharp;

namespace ClientMaui;

public partial class ChatRoom : ContentPage
{
    private Endpoint endpoint;
	private Room room;
	private HubConnection connection;
    private string token;
    public ChatRoom(Endpoint endpoint, Room room)
	{
		InitializeComponent();
		this.endpoint = endpoint;
		this.room = room;
	}

    protected override async void OnAppearing()
    {
		connection = new HubConnectionBuilder()
            .WithUrl($"http://{Preferences.Default.Get("IPAddress", "")}:5282/new-message",
                (opts) =>
                {
                    opts.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // always verify the SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        return message;
                    };
                }


                )
            .Build();

        connection.On<string>("RecieveId", async token => await connectToRoom(token));

		connection.On<string>("ReceiveMessage", (message) =>
        {
            var messageObject = JsonConvert.DeserializeObject<Message>(message);
            this.Dispatcher.Dispatch(() =>
            {
                var label = new Label
                {
                    Text = messageObject.message,
                    HorizontalOptions = messageObject.sender == token ? LayoutOptions.End : LayoutOptions.Start,
                };
                MessagesStack.Children.Add(label);
            });

        });
        await connection.StartAsync();

    }

    private async Task connectToRoom(string token)
    {
        this.token = token;
        var response = await endpoint.request(APIEndpoints.RoomEndpoints.Connect, id: room.id, identification: token);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            await DisplayAlert("Error", "Error occured while connecting to server, check IP address and port!", "OK");
            return;
        }
        var messages = await endpoint.request(APIEndpoints.RoomEndpoints.MessageList, id:room.id, identification: token);
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
        var response = await endpoint.request(APIEndpoints.RoomEndpoints.SendMessage, body: JsonConvert.SerializeObject(messageObject), method: Method.Post, id: room.id, identification: token);

    }
}