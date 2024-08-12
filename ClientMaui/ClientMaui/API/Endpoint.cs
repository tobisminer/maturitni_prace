using RestSharp;

namespace ClientMaui.API
{

    
    public class Endpoint(string url)
    {
        public string url = url;

        private RestClient client = new(url);
        
        public Task<RestResponse> Request(
            string endpoint,
            string body = "",
            Method method = Method.Get,
            int id = 0,
            string? identification = "")
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
                request.AddHeader("Identification", identification);
            }

            if (body != "")
            {
                request.AddParameter("application/json", body, ParameterType.RequestBody);
            }
           
            return client.ExecuteAsync(request);
        }
    }

    struct APIEndpoints
    {
        private const string Api = "api/";
        private const string Room = Api + "room/";


        public struct RoomEndpoints
        {
            public const string Index = Room;
            public const string RoomList = Room + "listDangerous";
            public const string Delete = Room + "delete";
            public const string Create = Room + "create";
            public const string Connect = Room + "connect";
            public const string SendMessage = Room + "sendMessage";
            public const string MessageList = Room + "messageList";
        }

    }


}
