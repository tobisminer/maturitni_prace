using RestSharp;
using System.Net;
using Newtonsoft.Json;

namespace ClientMaui.API
{
    public class Endpoint(string url)
    {
        public string url = url;

        public string username = "";

        private readonly RestClient _client = new(url);
        


        public async Task<RestResponse> Request(
            string endpoint,
            string? body = null,
            Method method = Method.Get,
            int? id = null,
            int? from = null,
            int? to = null
            )
        {
            var request = new RestRequest(endpoint)
            {
                Method = method
            };
            if (Authentication.Token != "")
            {
                request.AddHeader("Authorization", "Bearer " + Authentication.Token);
            }

            if (id != null)
            {
                request.Resource += $"/{id}";
            }
            if(from != null)
            {
                request.Resource += $"/{from}";
            }
            if (to != null)
            {
                request.Resource += $"/{to}";
            }


            if (body != null)
            {
                request.AddParameter("application/json", body, ParameterType.RequestBody);
            }
            var response = await _client.ExecuteAsync(request);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;
            await new Authentication(this).RenewToken();
            response = await _client.ExecuteAsync(request);


            return response;
        }
    }

    struct APIEndpoints
    {
        private const string Api = "api/";
        private const string Room = Api + "room/";
        private const string Authentication = Api + "authentication/";


        public struct RoomEndpoints
        {
            public const string Index = Room;
            public const string RoomList = Room + "list";
            public const string Delete = Room + "delete";
            public const string Create = Room + "create";
            public const string RoomTypes = Room + "roomTypes";
            public const string Connect = Room + "connect";
            public const string SendMessage = Room + "sendMessage";
            public const string MessageList = Room + "messageList";
            public const string GetKey = Room + "getSecretKey";
            public const string SetKey = Room + "setSecretKey";
        }
        public struct UserEndpoints
        {
            public const string Register = Authentication + "register";
            public const string Login = Authentication + "login";
            public const string Renew = Authentication + "renew";
        }

    }


}
