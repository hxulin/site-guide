using site_guide.domain;
using site_guide.http;
using site_guide.log;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using TinyJson;

namespace site_guide
{
    class Program
    {
        // 获取自身描述信息
        private static readonly string SELF_DESC_INFO = GetSelfDescInfo();

        // 定义最小时间单元: 1分钟
        private static readonly int TIME_UNIT = 60000;

        // 后台接口服务器地址
        private static readonly string API_HOST = "http://api.huangxulin.cn:4980";

        // 获取自身描述信息
        private static string GetSelfDescInfo()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyDescriptionAttribute asmdis = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));
            return asmdis.Description;
        }

        // 联网获取基础配置信息
        private static ClientConf GetConfInfo()
        {
            var confUrl = API_HOST + "/client_conf/get";
            while (true)
            {
                LogHelper.WriteLog("请求网络获取配置信息：" + confUrl);
                var result = HttpHelper.SendRequest(confUrl);
                LogHelper.WriteLog("返回结果：" + result);
                var apiResponse = JSONParser.FromJson<ApiResponse<ClientConf>>(result);
                if (apiResponse != null && apiResponse.code == 200)
                {
                    return apiResponse.data;
                }
                LogHelper.WriteLog("配置信息获取失败，正在重试");
                Thread.Sleep(TIME_UNIT);
            }
        }

        // 获取本机IP地址
        private static string GetLocalIP(string dnsAddress)
        {
            LogHelper.WriteLog("预定本地DNS主机地址：" + dnsAddress);
            LogHelper.WriteLog("尝试匹配本机IP地址");
            string ipPattern = dnsAddress.Substring(0, dnsAddress.LastIndexOf(".") + 1);
            string hostName = Dns.GetHostName();
            IPAddress[] addressList = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ipAddr in addressList)
            {
                string address = ipAddr.ToString();
                if (address.StartsWith(ipPattern))
                {
                    LogHelper.WriteLog("成功获取本机IP地址：" + address);
                    return address;
                }
            }
            return null;
        }

        // 更新IP信息
        private static void UpdateIP(string ip)
        {
            var updIpUrl = API_HOST + "/user/upd_ip";
            while (true)
            {
                LogHelper.WriteLog("同步更新客户端IP信息：" + updIpUrl);
                var result = HttpHelper.SendRequest(updIpUrl, "post", string.Format("desc={0}&lanIp={1}", Uri.EscapeDataString(SELF_DESC_INFO), ip));
                LogHelper.WriteLog("返回结果：" + result);
                var apiResponse = JSONParser.FromJson<ApiResponse<object>>(result);
                if (apiResponse != null)
                {
                    switch (apiResponse.code)
                    {
                        case 200:
                            return;
                        case 403:
                            LogHelper.WriteLog("程序自行退出，原因：" + apiResponse.msg);
                            Environment.Exit(0);
                            break;
                    }
                }
                LogHelper.WriteLog("同步更新IP信息失败，正在重试");
                Thread.Sleep(TIME_UNIT);
            }
        }

        static void Main(string[] args)
        {
            #region 判断当前程序是否启动，如果已启动则退出，保证只有一个实例启动
            Mutex mutexApp = new Mutex(false, Assembly.GetExecutingAssembly().FullName, out bool appIsRunning);
            if (!appIsRunning)
            {
                LogHelper.WriteLog("程序已经运行，请不要重复打开");
                return;
            }
            #endregion

            LogHelper.WriteLog("程序启动");

            Random random = new Random();

            while (true)
            {
                ClientConf clientConf = GetConfInfo();
                string localIP = GetLocalIP(clientConf.localDnsAddress);
                if (string.IsNullOrEmpty(localIP))
                {
                    LogHelper.WriteLog("匹配本机IP地址失败，重新获取配置信息");
                    Thread.Sleep(TIME_UNIT);
                    continue;
                }

                // 更新IP地址信息
                UpdateIP(localIP);

                // 产生一个随机睡眠时间
                var sleepTime = random.Next(clientConf.minTime, clientConf.maxTime + 1);
                Thread.Sleep(TIME_UNIT * sleepTime);
            }
        }
    }
}
