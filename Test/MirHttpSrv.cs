using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Test
{
    class MirHttpSrv
    {
        HttpListener httpListener = new HttpListener();
        public void Setup(int port = 8080)
        {
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add(string.Format("http://*:{0}/", port));//如果发送到8080 端口没有被处理，则这里全部受理，+是全部接收
            httpListener.Start();//开启服务

            Receive();//异步接收请求
        }

        private void Receive()
        {
            httpListener.BeginGetContext(new AsyncCallback(EndReceive), null);
        }

        void EndReceive(IAsyncResult ar)
        {
            var context = httpListener.EndGetContext(ar);
            Dispather(context);//解析请求
            Receive();
        }

        public static byte[] GetFile(string file)
        {
            if (!File.Exists(file)) return null;
            FileStream readIn = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[1024 * 1000];
            int nRead = readIn.Read(buffer, 0, 10240);
            int total = 0;
            while (nRead > 0)
            {
                total += nRead;
                nRead = readIn.Read(buffer, total, 10240);
            }
            readIn.Close();
            byte[] maxresponse_complete = new byte[total];
            System.Buffer.BlockCopy(buffer, 0, maxresponse_complete, 0, total);
            return maxresponse_complete;
        }

        void Dispather(HttpListenerContext context)
        {
            try
            {
                HttpListenerResponse response = context.Response;
                HttpListenerRequest request = context.Request;

                Console.WriteLine("URL: {0}", request.Url.OriginalString);
                Console.WriteLine("Raw URL: {0}", request.RawUrl);

                Console.WriteLine("Query: {0}", request.QueryString);
                Console.WriteLine("{0} {1} HTTP/1.1", request.HttpMethod, request.RawUrl);
                Console.WriteLine("Accept: {0}", string.Join(",", request.AcceptTypes));
                Console.WriteLine("Accept-Language: {0}",
                    string.Join(",", request.UserLanguages));
                Console.WriteLine("User-Agent: {0}", request.UserAgent);
                Console.WriteLine("Accept-Encoding: {0}", request.Headers["Accept-Encoding"]);
                Console.WriteLine("Connection: {0}",
                    request.KeepAlive ? "Keep-Alive" : "close");
                Console.WriteLine("Host: {0}", request.UserHostName);
                Console.WriteLine("Pragma: {0}", request.Headers["Pragma"]);

                Console.WriteLine("Path: {0}", HttpUtility.UrlDecode(request.Url.AbsolutePath));

                string path = HttpUtility.UrlDecode(request.Url.AbsolutePath);
                switch (path)
                {
                    case "/":
                        HandleHome(context);
                        break;

                    case "/login":
                        HandleLogin(context);
                        break;

                    case "/send":
                        HandleSend(context);
                        break;

                    default:
                        context.Response.StatusCode = 400;
                        context.Response.StatusDescription = "NOT FOUND";
                        context.Response.Close();
                        break;
                }


                string access_token = HttpUtility.ParseQueryString(request.Url.Query).Get("access_token");

                // Http回复
           
            }
            catch (Exception e)
            {
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    context.Response.StatusCode = 400;
                    writer.Write(e.Message);
                    writer.Close();
                }
            }
        }

        private void HandleSend(HttpListenerContext context)
        {
            throw new NotImplementedException();
        }

        private void HandleLogin(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            string access_token = HttpUtility.ParseQueryString(request.Url.Query).Get("access_token");
           // string access_token = HttpUtility.ParseQueryString(request.Url.Query).Get("access_token");
            throw new NotImplementedException();
        }

        private void HandleHome(HttpListenerContext context)
        {
            CallPage(context, "Web/index.html");
        }

        private void CallPage(HttpListenerContext context, string filePath)
        {
            HttpListenerResponse response = context.Response;
            context.Response.StatusCode = 200;
            byte[] page = GetFile(filePath);
            response.ContentLength64 = page.Length;
            Stream output = response.OutputStream;
            output.Write(page, 0, page.Length);
            output.Close();
        }
    }
}
