using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.Downloader;

namespace SpiderTest
{
    /// <summary>
    /// 图片下载
    /// </summary>
    public class ImageDownloader
    {
        #region 单例

        private static readonly ImageDownloader imageDownloader = new ImageDownloader();

        public static ImageDownloader GetInstance()
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
                    Console.WriteLine("图片已下载跳过！");
                    Console.ForegroundColor = ConsoleColor.White;
                    return true;
                }

                //HttpClient.DefaultRequestHeaders.Referrer = new Uri(request.Properties["referer"]);
                var message = await HttpClient.GetAsync(new Uri(request.Url));
                var url = message.Headers.Location;
                var content = await HttpClient.GetByteArrayAsync(url);

                var fs = new FileStream(savePath, FileMode.CreateNew);
                fs.Write(content, 0, content.Length);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("图片下载成功！");
                Console.ForegroundColor = ConsoleColor.White;
                return true;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("图片下载失败！" + e.Message);
                Console.ForegroundColor = ConsoleColor.White;
                downloadQueue.Enqueue(request);
                return false;
            }
        }

        private void CreateDirByPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string GetImagePath(string name)
        {
            //var fileName = imageUrl.Substring(imageUrl.LastIndexOf('=') + 1);
            //var tagPath = Environment.CurrentDirectory + "\\Pictures" + "\\" + tag;

            //CreateDirByPath(tagPath);

            //var subjectPath = tagPath + "\\" + subject;
            CreateDirByPath(Environment.CurrentDirectory + "\\Music");

            var filePath = Environment.CurrentDirectory + "\\Music" + "\\" + name + ".mp3";
            return filePath;
        }

        private async Task<Boolean> DownloadImage(Request request)
        {
            var tag = request.Properties["tag"].Replace("|", "").Replace(" ", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("?", "").Replace("*", "");
            //var subject = request.Properties["subject"];
            var fileUrl = request.Url;
            var filePath = GetImagePath(tag);
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
