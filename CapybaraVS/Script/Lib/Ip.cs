using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CapybaraVS.Script.Lib
{
    [ScriptClass]
    public static class Ip
    {
        private const string LIB_NAME = "Net";
        private const string LIB_NAME2 = "Net.Web";

        private static readonly HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(5000) };

        static void SetClient(string account, string passwd)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            if (account != null && passwd != null && account.Trim().Length != 0 && passwd.Trim().Length != 0)
            {
                var authToken = Encoding.ASCII.GetBytes($"{account}:{passwd}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            }
            client.DefaultRequestHeaders.Add("User-Agent", $"{MainWindow.AppName}-{MainWindow.AppVersion}");
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static HttpResponseMessage GetWebAPI(string url, string account = null, string passwd = null)
        {
            if (url is null || url.Trim() == "")
                return null;
            SetClient(account, passwd);
            var response = client.GetAsync(url);
            return response.Result;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static HttpResponseMessage PostWebAPI(string url, HttpContent content, string account = null, string passwd = null)
        {
            if (url is null || url.Trim() == "" || content is null)
                return null;
            SetClient(account, passwd);
            var response = client.PostAsync(url, content);
            return response.Result;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static string GetHeaders(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage is null)
                return null;
            return httpResponseMessage.Headers.ToString();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static string GetContents(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage is null)
                return null;
            var contens = httpResponseMessage.Content.ReadAsStringAsync();
            return contens.Result;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetMyHostName()
        {
            return Dns.GetHostName();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static ICollection<string> GetMyHostAddress()
        {
            var addressList = new List<string>();
            foreach (var node in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                addressList.Add(node.ToString());
            }
            return addressList;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static ICollection<string> GetHostEntry(string hostname)
        {
            var ipAddressList = new List<string>();
            foreach (var node in Dns.GetHostEntry(hostname).AddressList)
            {
                ipAddressList.Add(node.ToString());
            }
            return ipAddressList;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME)]
        public static string GetHostName(string address)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(address);
            return hostInfo.HostName;
        }
    }
}
