using DotnetSpider.DataFlow.Parser;
using DotnetSpider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using Serilog;
using Microsoft.Extensions.Configuration;
using DotnetSpider.Downloader;
using System.IO;

namespace SpiderTest.Music
{
    public class MusicSpider
    {
        public static void Run()
        {
            Downloader.GetInstance().Start();

            var builder = new SpiderHostBuilder()
                .ConfigureLogging(x => x.AddSerilog())
                .ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.json"))
                .ConfigureServices(services =>
                {
                    services.AddThroughMessageQueue();
                    services.AddLocalDownloadCenter();
                    services.AddDownloaderAgent(x =>
                    {
                        x.UseFileLocker();
                        x.UseDefaultAdslRedialer();
                        x.UseDefaultInternetDetector();
                    });
                    services.AddStatisticsCenter(x => x.UseMemory());
                });
            var provider = builder.Build();
            var spider = provider.Create<Spider>();

            spider.Id = Guid.NewGuid().ToString("N"); // 设置任务标识
            spider.Name = "网易云音乐"; // 设置任务名称
            spider.Speed = 2; // 设置采集速度, 表示每秒下载多少个请求, 大于 1 时越大速度越快, 小于 1 时越小越慢, 不能为0.
            spider.Depth = 5; // 设置采集深度

            spider.AddDataFlow(new MusicListDataParser());
            spider.AddRequests("https://music.163.com/#/playlist?id=2964757969"); // 设置起始链接
            spider.RunAsync(); // 启动
        }

        class MusicListDataParser:DataParser
        {
            public MusicListDataParser()
            {
                Required = DataParserHelper.CheckIfRequiredByRegex("^(http|https)?://music.163.com/#/playlist\\?id=[0-9]*");
            }
            protected override Task<DataFlowResult> Parse(DataFlowContext context)
            {
                context.AddData("URL", context.Response.Request.Url);
                context.AddData("Title", context.Selectable.XPath(".//title").GetValue());
                var songs = new Dictionary<string, string>();
                var tagNodes = context.Selectable
                    .XPath("//*[@id=\"song-list-pre-cache\"]/ul/li/a").Nodes();
                foreach (var node in tagNodes)
                {
                    var url = node.XPath("./@href").GetValue();
                    var urls = url.Split("song");

                    url = urls[0] + "song/media/outer/url" + urls[1] + ".mp3";
                    var name = node.GetValue();
                    songs.Add(url, name);
                    Console.WriteLine("url:" + url + " - name:" + name);
                }

                var requests = new List<Request>();
                foreach (var song in songs)
                {
                    var request = new Request
                    {
                        Url = song.Key,
                        OwnerId = context.Response.Request.OwnerId
                    };
                    request.AddProperty("tag", song.Value);
                    request.AddProperty("path", GetImagePath(song.Value));

                    Downloader.GetInstance().AddRequest(request);
                }

                return Task.FromResult(DataFlowResult.Success);
            }
        }
        private static string GetImagePath(string name)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Music"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Music");
            }
            var filePath = Environment.CurrentDirectory + "\\Music" + "\\" + name.Replace("|", "").Replace(" ", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace("<", "").Replace(">", "").Replace(":", "").Replace("?", "").Replace("*", "") + ".mp3";
            return filePath;
        }
    }
}
