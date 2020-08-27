using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Downloader;
using Org.BouncyCastle.Ocsp;

namespace SpiderTest.Music
{
    /// <summary>
    /// 下载
    /// </summary>
    public class Downloader
    {
        #region 单例

        private static readonly Downloader imageDownloader = new Downloader();

        public static Downloader GetInstance()
        {

            //HttpClient.DefaultRequestHeaders.Add("authority", "img.onvshen.com:85");
            //HttpClient.DefaultRequestHeaders.Add("method", "GET");
            //HttpClient.DefaultRequestHeaders.Add("scheme", "https");
            //HttpClient.DefaultRequestHeaders.Add("accept", "image/webp,image/apng,image/*,*/*;q=0.8");
            //HttpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
            //HttpClient.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8");
            //HttpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "image");
            //HttpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "no-cors");
            //HttpClient.DefaultRequestHeaders.Add("sec-fetch-site", "cross-site");


            return imageDownloader;
        }

        #endregion

        #region 内部字段

        //private static readonly HttpClient HttpClient = new HttpClient();


        private readonly Queue<Request> downloadQueue = new Queue<Request>();

        private Timer _timer;

        #endregion

        #region 私有方法

        private async Task<Boolean> DownloadAsync(Request request, string savePath)
        {
            try
            {
                if (File.Exists(savePath))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("下载跳过！");
                    Console.ForegroundColor = ConsoleColor.White;
                    return true;
                }
                string urlnew = request.Url.Replace("/s/", "/");

                HttpClient HttpClient = new HttpClient();

                HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36");

                HttpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.nvshens.org/img.html?img=" + urlnew);

                //string urlTest = request.Url;

                //HttpClient.DefaultRequestHeaders.Add("path", urlTest.Substring(urlTest.IndexOf("85/") + 2));

                Uri url = new Uri(urlnew);
                if (HttpClient.GetAsync(url).Result.StatusCode == HttpStatusCode.Redirect)
                {
                    Console.WriteLine("跳转");
                    var message = await HttpClient.GetAsync(url);
                    url = message.Headers.Location;
                }
                Console.WriteLine("下载地址：" + url);
                var content = await HttpClient.GetByteArrayAsync(url);

                var fs = new FileStream(savePath, FileMode.CreateNew);
                fs.Write(content, 0, content.Length);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("下载成功！");
                Console.ForegroundColor = ConsoleColor.White;
                return true;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("下载失败！" + e.Message + "地址：" + request.Url);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
        }

        private async Task<Boolean> DownloadImage(Request request)
        {
            var filePath = request.Properties["path"];
            await DownloadAsync(request, filePath);
            return true;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加下载请求
        /// </summary>
        /// <param name="request"></param>
        public void AddRequest(Request request)
        {
            downloadQueue.Enqueue(request);
        }

        /// <summary>
        /// 启动下载器
        /// </summary>
        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(state =>
            {

                lock (downloadQueue)
                {
                    if (downloadQueue.Count > 0)
                    {
                        Task.Run(async () =>
                        {
                            var queue = downloadQueue.Dequeue();
                            if (queue==null)
                            {
                                return;
                            }
                            await DownloadImage(queue);
                        });
                    }
                }

            }, null, 1000, 500);
        }

        #endregion


    }
}
