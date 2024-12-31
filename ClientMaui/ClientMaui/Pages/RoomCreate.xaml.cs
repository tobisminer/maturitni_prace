using ClientMaui.API;
using ClientMaui.Cryptography;
using ClientMaui.Entities.Room;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace ClientMaui.Pages;

public partial class RoomCreate : ContentPage
{
    private readonly Endpoint _endpoint;
    public RoomCreate(Endpoint endpoint)
    {
        InitializeComponent();
        _endpoint = endpoint;
        Load();
    }

    private async void Load()
    {
        RoomTypePicker.ItemsSource = await GetRoomTypes();
        RoomTypePicker.SelectedIndex = 0;
        BlockCypherModePicker.ItemsSource = Enum.GetNames(typeof(BlockCypherMode));
        BlockCypherModePicker.SelectedIndex = 0;
        BlockCypherModePicker.IsVisible = false;
    }

    private async Task<List<string>> GetRoomTypes()
    {
        var response =
            await _endpoint.Request(APIEndpoints.RoomEndpoints.RoomTypes);
        if (response is { StatusCode: HttpStatusCode.OK, Content: not null })
        {
            var roomTypes = JsonConvert
                 .DeserializeObject<List<RoomType>>(response.Content)!;
            return (from roomType in roomTypes
                    let secure = roomType.isSecure ? "\ud83d\udd12" : "\ud83d\udd13"
                    select $"{roomType.name} - {secure}").ToList();
        }

        await DisplayAlert("Error", "Error occured while fetching room types!", "OK");
        return [];

    }

    private async void CreateBtn_OnClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(RoomNameEntry.Text) || RoomTypePicker.SelectedItem == null || BlockCypherModePicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please fill all fields!", "OK");
            return;
        }
        var roomCreateJson = new RoomCreateJson
        {
            name = RoomNameEntry.Text,
            room_type = RoomTypePicker.SelectedItem.ToString()!.Split(" -")[0],
            block_cypher_mode = BlockCypherModeHelper.ConvertFromString(BlockCypherModePicker.SelectedItem.ToString())
        };
        var response = await _endpoint.Request(APIEndpoints.RoomEndpoints.Create, method: Method.Post, body: JsonConvert.SerializeObject(roomCreateJson));
        if (response.StatusCode != HttpStatusCode.OK)
        {
            await DisplayAlert("Error", "Error occured while creating room!", "OK");
            return;
        }
        await DisplayAlert("Success", "Room created successfully!", "OK");
        //Navigate back to RoomSelect page
        await Navigation.PopAsync();
    }

    private void RoomTypePicker_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (RoomTypePicker.SelectedItem == null) return;
        var roomType = RoomTypePicker.SelectedItem.ToString()!.Split(" -")[0];
        var blockMode = CryptographyHelper.GetBlockCypherStatus(roomType);
        BlockCypherModePicker.IsVisible = blockMode;
    }
}