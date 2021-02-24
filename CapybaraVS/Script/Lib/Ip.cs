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
        public enum WebMethod
        {
            GET,
            POST
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + ".Web" + "." + nameof(GetContents), "", "RS=>Ip_GetContents")]
        public static string GetContents(string address, double timeout = 5000)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = client.GetAsync(address);
            var contens = response.Result.Content.ReadAsStringAsync();
            return contens.Result;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + ".Web" + "." + nameof(GetHeaders), "", "RS=>Ip_GetHeaders")]
        public static string GetHeaders(string address, double timeout = 5000)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = client.GetAsync(address);
            var contens = response.Result.Headers.ToString();
            return contens;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + ".Web." + nameof(WebAPI), "", "RS=>Ip_WebAPI")]
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
        [ScriptMethod("Net" + "." + nameof(GetMyHostName), "", "RS=>Ip_GetMyHostName")]
        public static string GetMyHostName()
        {
            return Dns.GetHostName();
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + "." + nameof(GetMyHostAddress), "", "RS=>Ip_GetMyHostAddress")]
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
        [ScriptMethod("Net" + "." + nameof(GetHostEntry), "", "RS=>Ip_GetHostEntry")]
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
        [ScriptMethod("Net" + "." + nameof(GetHostName), "", "RS=>Ip_GetHostName")]
        public static string GetHostName(string address)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(address);
            return hostInfo.HostName;
        }
    }
}
