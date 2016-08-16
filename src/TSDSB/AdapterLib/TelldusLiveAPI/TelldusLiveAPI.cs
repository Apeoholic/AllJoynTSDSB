using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Portable.Authenticators;
using Newtonsoft.Json;
using AdapterLib.Devices;
using Windows.Foundation;
using AdapterLib.Helpers;

namespace TelldusLiveAPI
{
    class TelldusLiveAPIClient
    {
        public TelldusLiveAPIClient()
        {
            LoadConfiguration();
        }
        const string Publickey = "WAC2EMU3UQUGAPHAHA5RUPHEZ432JARU";
        const string Privatekey = "WRACAWRUBUBAFAZEVUSPETHUTE4RUC3X";
        public string Token = "";
        public string Tokensecret = "";

        public string GetURL()
        {
            var client = new RestClient(BaseUrl);
            client.Authenticator = OAuth1Authenticator.ForRequestToken(Publickey, Privatekey);

            var request = new RestRequest("oauth/requestToken", Method.POST);
            var responseTask = client.Execute(request);
            Task.WaitAll(responseTask);
            var response = responseTask.Result;

            
            WwwFormUrlDecoder d = new WwwFormUrlDecoder(response.Content);
            Token = d[1].Value;
            Tokensecret = d[2].Value;
            SaveConfiguration();
            request = new RestRequest("oauth/authorize");
            request.AddParameter("oauth_token", Token);

            var url = client.BuildUri(request).ToString();
            
            return url;
        }



        public void Finish()
        {

            var client = new RestClient(BaseUrl);

            client.Authenticator = OAuth1Authenticator.ForAccessToken(Publickey, Privatekey, Token, Tokensecret);

            var request = new RestRequest("oauth/accessToken");

            var responseTask = client.Execute(request);
            Task.WaitAll(responseTask);
            var response = responseTask.Result;

            WwwFormUrlDecoder d = new WwwFormUrlDecoder(response.Content);

            Token = d[0].Value;
            Tokensecret = d[1].Value;
            SaveConfiguration();
        }

        public void SaveConfiguration()
        {
            SaveConfiguration(Token, Tokensecret);
        }

        public void SaveConfiguration(string token, string tokenSecret)
        {
            StorageHelper.SaveSettings("Token", token);
            StorageHelper.SaveSettings("TokenSecret", tokenSecret);
        }

        public void LoadConfiguration()
        {
            Token = StorageHelper.LoadSettings("Token");
            Tokensecret = StorageHelper.LoadSettings("TokenSecret");
        }

        private const string BaseUrl = "http://api.telldus.com/";

        public async Task<string> getData(string url,Parameter[] parameters)
        {
            try
            {
                var client = new RestClient(BaseUrl);
                client.Authenticator = OAuth1Authenticator.ForProtectedResource(Publickey, Privatekey, Token, Tokensecret);
                var request = new RestRequest(url, Method.GET);
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        request.AddParameter(p);
                    }
                }
                var response = client.Execute(request);
                return (await response).Content;
            }
            catch(Exception ex)
            {


            }
            return "";
        }

        public async Task<List<Device>> GetDevices()
        {
            return JsonConvert.DeserializeObject<Devices>(await getData("json/devices/list",new Parameter[] { new Parameter() { Name = "supportedMethods", Value = "19" }})).device;
        }

        public async Task<Device> GetDevice(string id)
        {
            return JsonConvert.DeserializeObject<Device>(await getData("json/device/info" , new Parameter[] { new Parameter() { Name = "supportedMethods", Value = "19" },new Parameter() { Name = "id", Value = id} }));
        }


        public async Task<List<Sensor>> GetSensors()
        {
            return JsonConvert.DeserializeObject<SensorResponse>(await getData("json/sensors/list",null)).sensor;
        }

        public async Task<Sensor> GetSensor(string id)
        {
            return JsonConvert.DeserializeObject<Sensor>(await getData("json/sensor/info", new Parameter[] { new Parameter() { Name = "id", Value = id } }));
        }

        public async Task SendCommand(string id, string method, string value)
        {
            List<Parameter> paramaters = new List<Parameter>();
            paramaters.Add(new Parameter() { Name="id",Value=id });
            paramaters.Add(new Parameter() { Name = "method", Value = method });
            paramaters.Add(new Parameter() { Name = "value", Value = value });
            await getData("json/device/command", paramaters.ToArray());
        }

    }
}
