using DotnetSpider.Sample.samples;
using SpiderTest.Image;
using SpiderTest.Music;
using System;
using System.Threading.Tasks;

namespace SpiderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入网易云歌单地址,示例:https://music.163.com/#/playlist?id=922733710");
            var loaction = Console.ReadLine();
            MusicSpider.Run(loaction);

            Console.ReadLine();
        }
    }
}
