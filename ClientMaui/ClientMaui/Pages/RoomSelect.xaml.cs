using ClientMaui.API;
using ClientMaui.Entities.Room;
using ClientMaui.Pages;
using ClientMaui.Widgets;
using Newtonsoft.Json;
using System.Net;

namespace ClientMaui;

public partial class RoomSelect
{
    private Endpoint endpoint;
    public RoomSelect(Endpoint endpoint)
    {
        InitializeComponent();
        this.endpoint = endpoint;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        RoomList.Children.Clear();
        var response = await endpoint.Request(APIEndpoints.RoomEndpoints.RoomList);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            await DisplayAlert("Error", "Error occured while connecting to server, check IP address and port!", "OK");
            return;
        }
        var rooms = JsonConvert.DeserializeObject<List<Room>>(response.Content);
        foreach (var room in rooms)
        {
            //add control to the stacklayout
            var roomWidget = new RoomListWidget(room);
            roomWidget.ConnectButton.Clicked += async (_, _) =>
            {
                await Navigation.PushAsync(new ChatRoom(endpoint, room));
            };

            RoomList.Children.Add(roomWidget);
        }
    }

    private void CreateRoomButton_OnClicked(object? sender, EventArgs e)
    {
        var page = new RoomCreate(endpoint);
        Navigation.PushAsync(page);
    }
}