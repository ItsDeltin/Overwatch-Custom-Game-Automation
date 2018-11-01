using System;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace ZombieBot
{
    class Abyxa
    {
        string abyxaurl = "http://localhost:80/zombie/";

        string id;

        string crypt;

        public Abyxa(string name, string region, bool local = false)
        {
            if (local == false)
                abyxaurl = "http://www.abyxa.net/zombie/";
            dynamic r = JsonConvert.DeserializeObject(new WebClient().DownloadString(abyxaurl + "create?name=" + name + "&region=" + region));
            id = r.id;
            crypt = r.crypt;
        }

        public string[] Queuelist()
        {
            return WebUtility.HtmlDecode(new WebClient().DownloadString(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&inqueue")).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void RemoveFromQueue(string battletag)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&remove=" + WebUtility.UrlEncode(battletag.Replace("#", "-")), true);
        }

        public void SetPlayerCount(int playercount)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&playercount=" + playercount);
        }

        public void SetMode(int mode)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&mode=" + mode);
        }

        public void SetMap(string map)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&map=" + map);
        }

        public void SetSurvivorCount(string survivors)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&survivors=" + survivors);
        }

        public void SetGameEnd(DateTime time)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&gamestart=" + time.ToString());
        }

        public void Update()
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&update");
        }

        public void SetInviteCount(int invited)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&invited=" + invited);
        }

        public void SetMinimumPlayerCount(int minimumPlayerCount)
        {
            Req(abyxaurl + "request?id=" + id + "&crypt=" + crypt + "&mpc=" + minimumPlayerCount);
        }

        private static void Req(string url, bool wait = false)
        {
            Task requestTask = Task.Run(() =>
            {
                WebClient webClient = new WebClient();
                webClient.DownloadData(url);
                webClient.Dispose();
            });
            if (wait)
                requestTask.Wait();
        }
    }
}
