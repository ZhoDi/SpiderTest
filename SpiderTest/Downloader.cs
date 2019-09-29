using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Downloader;

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
            return imageDownloader;
        }

        #endregion

        #region 内部字段

        private static readonly HttpClient HttpClient = new HttpClient();

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

                //HttpClient.DefaultRequestHeaders.Referrer = new Uri(request.Properties["referer"]);
                Uri url = new Uri(request.Url);
                if (HttpClient.GetAsync(request.Url).Result.StatusCode == HttpStatusCode.Redirect)
                {
                    var message = await HttpClient.GetAsync(url);
                    url = message.Headers.Location;
                }
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
                Console.WriteLine("下载失败！" + e.Message);
                Console.ForegroundColor = ConsoleColor.White;
                downloadQueue.Enqueue(request);
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
                if (downloadQueue.Count > 0)
                {
                    Task.Run(async () =>
                    {
                        await DownloadImage(downloadQueue.Dequeue());
                    });
                }

            }, null, 1000, 500);
        }

        #endregion


    }
}
