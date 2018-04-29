using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace xLiAd.Cos
{
    public class Cos
    {
        private string APPID { get; set; }
        private string SecretId { get; set; }
        private string SecretKey { get; set; }
        private string BucketName { get; set; }
        private string Region { get; set; }
        private string Host { get; set; }

        public Cos(string APPID, string secretId, string secretKey, string bucketName, string region)
        {
            this.APPID = APPID;
            this.SecretId = secretId;
            this.SecretKey = secretKey;
            if (bucketName.EndsWith($"-{APPID}"))
                this.BucketName = bucketName;
            else
                this.BucketName = $"{bucketName}-{APPID}";
            this.Region = region;
            this.Host = $"{BucketName}.cos.{Region}.myqcloud.com";
        }

        public string Put(string targetPath, byte[] data)
        {
            string auth = GetAuth(targetPath, HttpMethod.Put);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://{Host}{targetPath}");
            req.Method = "PUT";
            req.ContentLength = data.Length;
            req.Headers.Add("Date", DateTime.Now.AddHours(-8).ToString("r"));
            req.Headers.Add("Authorization", auth);
            req.Headers.Add("Host", Host);
            Stream newStream = req.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            HttpWebResponse res = null;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException eex)
            {
                res = eex.Response as HttpWebResponse;
            }
            return GetResponseString(res);
        }
        /// <summary>
        /// 从本地新增对象
        /// </summary>
        /// <param name="targetPath">目标对象路径</param>
        /// <param name="localFile">本地文件路径</param>
        /// <returns></returns>
        public string Put(string targetPath, string localFile)
        {
            return Put(targetPath, ConvertFileToBytes(localFile));
        }
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="targetPath">目标对象路径</param>
        /// <returns></returns>
        public string Delete(string targetPath)
        {
            string auth = GetAuth(targetPath, HttpMethod.Delete);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://{Host}{targetPath}");
            req.Method = "DELETE";
            req.ContentLength = 0;
            req.Headers.Add("Date", DateTime.Now.AddHours(-8).ToString("r"));
            req.Headers.Add("Authorization", auth);
            req.Headers.Add("Host", Host);
            HttpWebResponse res = null;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException eex)
            {
                res = eex.Response as HttpWebResponse;
            }
            return GetResponseString(res);
        }
        /// <summary>
        /// 公有读情况下的查看对象是否存在
        /// </summary>
        /// <param name="objectPath">目标对象路径</param>
        /// <returns></returns>
        public bool Exists(string objectPath)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://{Host}{objectPath}");
            req.Method = "HEAD";
            req.ContentLength = 0;
            HttpWebResponse res = null;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException eex)
            {
                res = eex.Response as HttpWebResponse;
            }
            switch (res.StatusCode)
            {
                case HttpStatusCode.OK:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 私有读情况下的查看对象是否存在
        /// </summary>
        /// <param name="objectPath">目标对象路径</param>
        /// <returns></returns>
        public bool ExistsWithAuth(string objectPath)
        {
            string auth = GetAuth(objectPath, HttpMethod.Head);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create($"https://{Host}{objectPath}");
            req.Method = "HEAD";
            req.ContentLength = 0;
            req.Headers.Add("Date", DateTime.Now.AddHours(-8).ToString("r"));
            req.Headers.Add("Authorization", auth);
            req.Headers.Add("Host", Host);
            HttpWebResponse res = null;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException eex)
            {
                res = eex.Response as HttpWebResponse;
            }
            switch (res.StatusCode)
            {
                case HttpStatusCode.OK:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// 公有读情况下的获取对象
        /// </summary>
        /// <param name="objectPath">目标对象路径</param>
        /// <returns></returns>
        public byte[] Get(string objectPath)
        {
            WebClient wc = new WebClient();
            return wc.DownloadData($"https://{Host}{objectPath}");
        }
        /// <summary>
        /// 私有读情况下的获取对象
        /// </summary>
        /// <param name="objectPath">目标对象路径</param>
        /// <returns></returns>
        public byte[] GetWithAuth(string objectPath)
        {
            string auth = GetAuth(objectPath, HttpMethod.Get);
            ////////////////////////////////////////////////////
            WebClient wc = new WebClient();
            wc.Headers.Add("Date", DateTime.Now.AddHours(-8).ToString("r"));
            wc.Headers.Add("Authorization", auth);
            return wc.DownloadData($"https://{Host}{objectPath}");
        }
        private static string ConvertDatetimeToUnixTime(DateTime dateTime)
        {
            DateTime o = new DateTime(1970, 1, 1, 8, 0, 0);
            var r = (dateTime - o).TotalSeconds;
            return r.ToString("f0");
        }
        private string GetResponseString(HttpWebResponse res)
        {
            Stream ReceiveStream = res.GetResponseStream();
            Encoding encode = Encoding.UTF8;
            StreamReader sr = new StreamReader(ReceiveStream, encode);
            Char[] read = new Char[256];
            int count = sr.Read(read, 0, 256);
            string strResult = string.Empty;
            while (count > 0)
            {
                String str = new String(read, 0, count);
                strResult += str;
                count = sr.Read(read, 0, 256);
            }
            return strResult;
        }
        private string GetAuth(string targetPath, HttpMethod httpMethod)
        {
            var signTime = ConvertDatetimeToUnixTime(DateTime.Now.AddHours(-17)) + ";" + ConvertDatetimeToUnixTime(DateTime.Now.AddHours(17));
            var headerList = "host";
            var headerListWithValue = $"host={Host}";
            var paramList = "";
            ///////////////Signature 计算
            var SignKey = hash_hmac(signTime, SecretKey);
            var HttpString = $"{httpMethod.ToString().ToLower()}\n{targetPath}\n{paramList}\n{headerListWithValue}\n";
            var sha1edHttpString = SHA1(HttpString, Encoding.UTF8).ToLower();
            var StringToSign = $"sha1\n{signTime}\n{sha1edHttpString}\n";
            var Signature = hash_hmac(StringToSign, SignKey);
            //////////////
            string auth = $"q-sign-algorithm=sha1&q-ak={SecretId}&q-sign-time={signTime}&q-key-time={signTime}&q-header-list={headerList}&q-url-param-list={paramList}&q-signature={Signature}";
            return auth;
        }

        private static string hash_hmac(string signatureString, string secretKey)
        {
            var enc = Encoding.UTF8;
            HMACSHA1 hmac = new HMACSHA1(enc.GetBytes(secretKey));
            hmac.Initialize();
            byte[] buffer = enc.GetBytes(signatureString);
            return BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "").ToLower();
        }
        private static string SHA1(string content, Encoding encode)
        {
            try
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] bytes_in = encode.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                sha1.Dispose();
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("SHA1加密出错：" + ex.Message);
            }
        }
        private static byte[] ConvertFileToBytes(string filename)
        {
            if (!System.IO.File.Exists(filename))
                throw new Exception("File Not Found");
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            byte[] bs = new byte[fs.Length];
            fs.Read(bs, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            fs.Dispose();
            fs = null;
            return bs;
        }
    }
}
