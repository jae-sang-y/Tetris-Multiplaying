using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Tetris
{
    public class ServerConnector
    {
        private static ServerConnector instance = null;
        protected ServerConnector()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        [STAThread]
        public static ServerConnector GetInstace()
        {
            if (instance == null) instance = new ServerConnector();

            return instance;
        }
        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static int Timeout = 3;
        public Tuple<HttpStatusCode, string, long> SendRequest(string link, string method, string pdata = null, string header = null)
        {
            var watch = new System.Diagnostics.Stopwatch();
            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($@"https://127.0.0.1:443/{ link }");
            req.Method = method;
            req.ContentType = "application/json;charset=UTF-8";
            req.Accept = "*/*";

            //string de = req.RequestUri.ToString();

            if (header != null) req.Headers.Add(header);

            if (pdata != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(pdata);
                req.ContentLength = data.Length;

                using (Stream requestStream = req.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }
            }
            watch.Start();
            WebResponse response = req.GetResponse();
            string res;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                res = reader.ReadToEnd();
            }
            watch.Stop();

            return new Tuple<HttpStatusCode, string, long>(((HttpWebResponse)response).StatusCode, res, watch.ElapsedMilliseconds);
        }
    }
}