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

        private string downloadPage(Uri link)
        {
            var http = (HttpWebRequest)HttpWebRequest.Create(link);
            http.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
            http.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            http.AutomaticDecompression = DecompressionMethods.GZip;
            http.Method = "GET";
            WebResponse response;
            string text = "";
            try
            {
                response = http.GetResponse();
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
            //检查页面个数是否已经到达
            if (availablePages.Count >= visitPageCount || isRunning == false)
                return;
            count++;
            var nowCount = count;
            //开始访问
            var regex = new Regex(@"(?<=href\=\"")[^\""]*(?=\"")");
            var content = downloadPage(pageLink);
            //排除CSS、JS脚本
            var matchURL = regex.Matches(content).Cast<Match>().Select(a => a.Value);
            //排除空URL
            matchURL = matchURL.Where(a => a.Trim() != "");
            //排除JS脚本
            //matchURL = matchURL.Where(a => Uri.IsWellFormedUriString(a, UriKind.RelativeOrAbsolute));
            matchURL = matchURL.Where(a => !a.Contains("javascript"));
            //转换为URI，链接相对地址
            var baseUri = new Uri(pageLink.GetLeftPart(UriPartial.Authority));
            var availableURI = matchURL.Select(a => new Uri(baseUri, a));
            //排除CSS、JS
            var invalidExtensions = new string[] { ".css", ".js", ".png", ".jpg", ".ico", ".svg", ".gif" };
            availableURI = availableURI.Where(u => !invalidExtensions.Contains(Path.GetExtension(u.GetLeftPart(UriPartial.Path)).ToLower()));
            //剔除非https和http的地址
            availableURI = availableURI.Where(u => u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
            //按设定选择剔除与根网站相同域名的页面
            var authority = firstPage.GetLeftPart(UriPartial.Authority).ToLower();
            availableURI = availableURI.Where(u => u.GetLeftPart(UriPartial.Authority).ToLower() == authority);
            //将本页添加到访问集合中
            var pi = new PageInfo()
            {
                PageToOtherLinks = availableURI,
                PageLink = pageLink
            };
            availablePages.Add(pi);
            //触发新页面获取事件
            this.NewPageGet?.Invoke(this, new PageGetEventArgs() { PageInfo = pi, Count = nowCount });
            //检查是否已经访问，没有就递归访问。
            availableURI.ToList().ForEach(async u =>
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
