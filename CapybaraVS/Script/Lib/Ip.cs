using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CapybaraVS.Script.Lib
{
    class Ip
    {
        [ScriptMethod("Net" + ".Web" + "." + nameof(GetContents), "",
            "RS=>Ip_GetContents"//"HTTPのコンテンツを参照します。"
            )]
        public static string GetContents(string address, double timeout = 5000)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = client.GetAsync(address);
            var contens = response.Result.Content.ReadAsStringAsync();
            return contens.Result;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + ".Web" + "." + nameof(GetHeaders), "",
            "RS=>Ip_GetHeaders"//"HTTPのヘッダーを参照します。"
            )]
        public static string GetHeaders(string address, double timeout = 5000)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = client.GetAsync(address);
            var contens = response.Result.Headers.ToString();
            return contens;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + "." + nameof(GetMyHostName), "",
            "RS=>Ip_GetMyHostName"//"ホスト名を参照します。"
            )]
        public static string GetMyHostName()
        {
            return Dns.GetHostName();
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + "." + nameof(GetMyHostAddress), "",
            "RS=>Ip_GetMyHostAddress"//"ホストアドレスを参照します。"
            )]
        public static List<string> GetMyHostAddress()
        {
            List<string> addressList = new List<string>();
            foreach (var node in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                addressList.Add(node.ToString());
            }
            return addressList;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + "." + nameof(GetHostEntry), "",
            "RS=>Ip_GetHostEntry"//"ホスト名からアドレス情報を参照します。"
            )]
        public static List<string> GetHostEntry(string hostname)
        {
            List<string> ipAddressList = new List<string>();
            foreach (var node in Dns.GetHostEntry(hostname).AddressList)
            {
                ipAddressList.Add(node.ToString());
            }
            return ipAddressList;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Net" + "." + nameof(GetHostName), "",
            "RS=>Ip_GetHostName"//"アドレスからホスト名を参照します。"
            )]
        public static string GetHostName(string address)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(address);
            return hostInfo.HostName;
        }
    }
}
