using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Server.MirNetwork
{
    public class MirHttpSrv
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

        void Dispather(HttpListenerContext context)
        {
            try
            {
                // 发送短信内容
                var reader = new System.IO.StreamReader(context.Request.InputStream);
                String data = reader.ReadToEnd();

                // Http回复
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                    context.Response.StatusCode = 200;
                    writer.Write(data);
                    writer.Close();
                }
                context.Response.StatusCode = 200;
                context.Response.Close();
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
    }
}
