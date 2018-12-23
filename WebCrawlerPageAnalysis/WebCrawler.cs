using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WebCrawlerPageAnalysis.EventArgs;

namespace WebCrawlerPageAnalysis
{
    class WebCrawler
    {
        private int visitPageCount { get; set; }
        private int count { get; set; }
        private bool isRunning { get; set; } = false;
        private Uri firstPage { get; set; }
        private bool sameHost { get; set; } = true;
        private IList<PageInfo> availablePages { get; set; } = new List<PageInfo>();

        private string downloadPage(Uri link,out bool isHTML)
        {
            var http = (HttpWebRequest)HttpWebRequest.Create(link);
            http.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
            http.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            http.AutomaticDecompression = DecompressionMethods.GZip;
            http.Timeout = 10000;
            http.Method = "GET";
            WebResponse response;
            string text = "";
            isHTML = false;
            try
            {
                response = http.GetResponse();
                isHTML = response.ContentType.ToLower().Contains("text/html");
                text = new StreamReader(response.GetResponseStream()).ReadToEnd();
                http.Abort();
            }
            catch (Exception)
            {
                http.Abort();
                text = "";
            }
            return text;
        }

        private void recursePage(Uri pageLink)
        {
            //Check if reach max count
            if (availablePages.Count >= visitPageCount || isRunning == false)
                return;
            
            //Start to visit
            var isHtml = false;
            var content = downloadPage(pageLink, out isHtml);
            if (!isHtml) return;
            var nowCount = count;
            count++;
            var regex = new Regex(@"(?<=href\=\"")[^\""]*(?=\"")");
            var matchURL = regex.Matches(content).Cast<Match>().Select(a => a.Value);
            //Exclude empty url
            matchURL = matchURL.Where(a => a.Trim() != "");
            //Exclude JavaScript
            matchURL = matchURL.Where(a => !a.Contains("javascript"));
            //Convert to Uri type and concatenate relative url
            var baseUri = new Uri(pageLink.GetLeftPart(UriPartial.Authority));
            var availableURI = matchURL.Select(a => new Uri(baseUri, a));
            //exclude file
            var invalidExtensions = new string[] { ".css", ".js", ".png", ".jpg", ".ico", ".svg", ".gif" };
            availableURI = availableURI.Where(u => !invalidExtensions.Contains(Path.GetExtension(u.GetLeftPart(UriPartial.Path)).ToLower()));
            //exclude none http and https protocol
            availableURI = availableURI.Where(u => u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
            //exclude weblink not in same host according to settings
            if(sameHost)
            {
                var authority = firstPage.GetLeftPart(UriPartial.Authority).ToLower();
                availableURI = availableURI.Where(u => u.GetLeftPart(UriPartial.Authority).ToLower() == authority);
            }
            //add this pages to visited list
            var pi = new PageInfo()
            {
                PageToOtherLinks = availableURI,
                PageLink = pageLink
            };
            availablePages.Add(pi);
            this.NewPageGet?.Invoke(this, new PageGetEventArgs() { PageInfo = pi, Count = nowCount });
            //Check if has been visited
            availableURI.ToList().ForEach(u =>
            {
                if (availablePages.FirstOrDefault(a => a.PageLink.GetLeftPart(UriPartial.Path).ToLower() == u.GetLeftPart(UriPartial.Path).ToLower()) == null)
                {
                    recursePage(u);
                }
            });
        }

        public event EventHandler<PageGetEventArgs> NewPageGet;

        public IList<PageInfo> GetPages()
        {
            isRunning = true;
            this.count = 0;
            this.availablePages.Clear();
            recursePage(firstPage);
            return this.availablePages;
        }

        public void Stop()
        {
            isRunning = false;
        }
        public WebCrawler(string firstPage, int pageCount, bool isSameHost)
        {
            this.firstPage = new Uri(firstPage);
            this.visitPageCount = pageCount;
            this.sameHost = isSameHost;
        }
    }
}
