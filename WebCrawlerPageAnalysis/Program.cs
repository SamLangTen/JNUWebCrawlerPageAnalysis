﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
            //Do
            var wb = new WebCrawler(link, count, true);

            wb.NewPageGet += Wb_NewPageGet;
            GetResult(wb);
            Console.ReadLine();
        }
        
        private static void GetResult(WebCrawler wb)
        {
            var result = wb.GetPagesAsync();
            Console.WriteLine("Total Pages:{0}", result.Count());
            Console.Write("Output Filename:");
            var filename = Console.ReadLine();
            var outputText = new PageLinkSerializer(result).GetSerializedText();
            File.WriteAllText(filename, outputText);
        }

        private static void Wb_NewPageGet(object sender, EventArgs.PageGetEventArgs e)
        {
            Console.WriteLine("Get:{0}\tLink:{1}", e.PageInfo.PageLink.GetLeftPart(UriPartial.Path), e.PageInfo.PageToOtherLinks.Count());
        }
    }
}
