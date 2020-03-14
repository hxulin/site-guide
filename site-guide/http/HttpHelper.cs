using System;
using System.IO;
using System.Net;
using System.Text;

namespace site_guide.http
{
    class HttpHelper
    {
        public static string SendRequest(string url,string method = "GET", string postData = "")
        {
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(url) as HttpWebRequest;
                request.AllowAutoRedirect = true;
                request.Method = method.ToUpper();
                request.KeepAlive = true;
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36";

                if (method.ToUpper() == "POST")
                {
                    request.ContentLength = postData.Length;
                    if (!string.IsNullOrEmpty(postData))
                    {
                        byte[] data = Encoding.Default.GetBytes(postData);
                        request.ContentLength = data.Length;
                        using (Stream outstream = request.GetRequestStream())
                        {
                            outstream.Write(data, 0, data.Length);
                        }
                    }
                }

                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                using (Stream instream = response.GetResponseStream())
                {

                    using (StreamReader sr = new StreamReader(instream, Encoding.UTF8))
                    {
                        //返回请求结果
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return err;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }
    }
}
