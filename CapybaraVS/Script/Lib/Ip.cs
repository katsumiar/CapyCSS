using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CapybaraVS.Script.Lib
{
    public class Ip
    {
        private const string LIB_NAME = "Net";
        private const string LIB_NAME2 = "Net.Web";

        public enum WebMethod
        {
            GET,
            POST
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static string GetContents(string address, double timeout = 5000)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = client.GetAsync(address);
            var contens = response.Result.Content.ReadAsStringAsync();
            return contens.Result;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static string GetHeaders(string address, double timeout = 5000)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = client.GetAsync(address);
            var contens = response.Result.Headers.ToString();
            return contens;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static HttpWebResponse WebAPI(string command, string account, string passwd, WebMethod method = WebMethod.GET)
        {
            WebRequest req = WebRequest.Create(command);
            switch (method)
            {
                case WebMethod.GET:
                    req.Method = "GET";
                    break;

                case WebMethod.POST:
                    req.Method = "POST";
                    break;
            }
            req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(account + ":" + passwd));
            return req.GetResponse() as HttpWebResponse;
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
