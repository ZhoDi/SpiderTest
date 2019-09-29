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
            //MusicSpider.Run();
            NvshensSpider.Run();
            Console.Read();
        }
    }
}
