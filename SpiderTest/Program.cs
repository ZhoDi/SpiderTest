using DotnetSpider.Sample.samples;
using SpiderTest.Blog;
using SpiderTest.Music;
using System;
using System.Threading.Tasks;

namespace SpiderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MusicSpider.Run();

            Console.Read();
        }
    }
}
