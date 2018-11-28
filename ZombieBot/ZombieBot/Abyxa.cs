using System;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using ZombieBot;

namespace ZombieBot
{
    class Abyxa
    {
        private const string CouldNotConnect = "Error: Could not connected to host.";

        public const int Pregame = 0;
        public const int SettingUpNextGame = 1;
        public const int Ingame = 2;

        public readonly ZombieServer ZombieServer = new ZombieServer();
        private readonly string URL;
        private readonly string AccessInfo;

        private bool Initialized = false;

        public Abyxa(string name, int region, bool local)
        {
            URL = !local ? "http://www.abyxa.net/zombie/" : "http://localhost:80/zombie/";
            Log($"Server browser host: {URL}");

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string createJson = webClient.DownloadString(string.Format("{0}create?name={1}&region={2}", URL, name, region));
                    dynamic json = JsonConvert.DeserializeObject(createJson);

                    AccessInfo = string.Format("?id={0}&crypt={1}", json.id, json.crypt);

                    Initialized = true;
                }
                catch (WebException)
                {
                    Log(CouldNotConnect);
                    return;
                }
            }
        }

        public void Update()
        {
            if (!Initialized)
                return;

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.UploadString(FormatURL("serverupdate"), JsonConvert.SerializeObject(ZombieServer));
                }
                catch (WebException)
                {
                    Log(CouldNotConnect);
                    return;
                }
            }
        }

        public List<QueueUser> GetQueue()
        {
            if (!Initialized)
                return new List<QueueUser>();

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<QueueUser>>(webClient.DownloadString(FormatURL("getqueue")));
                }
                catch (WebException)
                {
                    Log(CouldNotConnect);
                    return new List<QueueUser>();
                }
            }
        }

        public void RemoveFromQueue(string battletag)
        {
            if (!Initialized)
                return;

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadData(FormatURL("removefromqueue") + "&battletag=" + WebUtility.UrlEncode(battletag));
                }
                catch (WebException)
                {
                    Log(CouldNotConnect);
                    return;
                }
            }
        }

        private string FormatURL(string page)
        {
            return URL + page + AccessInfo;
        }

        private static void Log(string text)
        {
            Console.WriteLine("[Abyxa] " + text);
        }
    }
}
