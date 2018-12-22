using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace WebCrawlerPageAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Web Link:");
            var link = Console.ReadLine();
            Console.Write("Visit Count:");
            var count = int.Parse(Console.ReadLine());
            Console.Write("Only Search Same Origin?(Y/n):");
            var isSameOrigin = Console.ReadLine() == "n" ? false : true;
            //Do
            ServicePointManager.DefaultConnectionLimit = 1024;
            var wb = new WebCrawler(link, count, isSameOrigin);
            wb.NewPageGet += Wb_NewPageGet;
            var result = wb.GetPages();
            Console.WriteLine("Total Pages:{0}", result.Count());
            Console.Write("Output Filename:");
            var filename = Console.ReadLine();
            var outputText = new PageLinkSerializer(result).GetSerializedText();
            File.WriteAllText(filename, outputText);
            Console.ReadLine();
        }

        private static void Wb_NewPageGet(object sender, EventArgs.PageGetEventArgs e)
        {
            Console.WriteLine("Get:{0}\t\tLink:{1}\t{2}", e.PageInfo.PageLink.GetLeftPart(UriPartial.Path), e.PageInfo.PageToOtherLinks.Count(), e.Count);
        }
    }
}
