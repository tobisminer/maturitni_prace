using RestSharp;

namespace ClientMaui.API
{
    public class Endpoint(string url)
    {
        private RestClient client = new RestClient(url);

        public Task<RestResponse> request(
            string endpoint,
            string body = "",
            Method method = Method.Get,
            int id = 0,
            string identification = "")
        {
            var request = new RestRequest(endpoint)
            {
                Method = method
            };

            if (id != 0)
            {
                request.Resource += $"/{id}";
            }

            if (identification != "")
            {
                request.AddHeader("Authorization", identification);
            }

            return client.ExecuteAsync(request);
        }
    }

    struct APIEndpoints
    {
        private const string API = "api/";
        private const string ROOM = API + "room/";


        public struct RoomEndpoints
        {
            public const string Index = ROOM;
            public const string RoomList = ROOM + "listDangerous";
            public const string Delete = ROOM + "delete";
            public const string Create = ROOM + "create";
            public const string Connect = ROOM + "connect";
            public const string SendMessage = ROOM + "sendMessage";
            public const string MessageList = ROOM + "messageList";
        }

    }


}
