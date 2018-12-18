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
        private bool isRunning { get; set; } = false;
        private Uri firstPage { get; set; }
        private IList<PageInfo> availablePages { get; set; } = new List<PageInfo>();

        private async Task recursePage(Uri pageLink)
        {
            //检查页面个数是否已经到达
            if (availablePages.Count >= visitPageCount || isRunning == false)
                return;
            //开始访问
            var regex = new Regex(@"(?<=href\=\"")[^\""]*(?=\"")");
            var client = new HttpClient();
            var content = await client.GetStringAsync(pageLink);
            //排除CSS、JS脚本
            var matchURL = regex.Matches(content).Cast<Match>().Select(a => a.Value);
            //排除空URL
            matchURL = matchURL.Where(a => a.Trim() != "");
            //排除JS脚本
            matchURL = matchURL.Where(a => !Uri.IsWellFormedUriString(a, UriKind.RelativeOrAbsolute));
            //排除CSS、JS
            var invalidExtensions = new string[] { "css", "js", "png", "jpg", "ico", "svg", "gif" };
            matchURL = matchURL.Where(a => !invalidExtensions.Contains(Path.GetExtension(a).ToLower()));
            //转换为URI，链接相对地址
            var baseUri = new Uri(pageLink.Host);
            var availableURI = matchURL.Select(a => new Uri(baseUri, a));
            //将本页添加到访问集合中
            var pi = new PageInfo()
            {
                PageToOtherLinks = availableURI,
                PageLink = pageLink
            };
            availablePages.Add(pi);
            //触发新页面获取事件
            this.NewPageGet?.Invoke(this, new PageGetEventArgs() { PageInfo = pi });
            //检查是否已经访问，没有就递归访问。
            availableURI.ToList().ForEach(async u =>
            {
                if (availablePages.FirstOrDefault(a => a.PageLink.Equals(u)) != null)
                {
                    await recursePage(u);
                }
            });
        }

        public event EventHandler<PageGetEventArgs> NewPageGet;

        public async Task<IList<PageInfo>> GetPagesAsync()
        {
            isRunning = true;
            this.availablePages.Clear();
            await recursePage(firstPage);
            return this.availablePages;
        }

        public void Stop()
        {
            isRunning = false;
        }
        public WebCrawler(string firstPage,int pageCount)
        {
            this.firstPage = new Uri(firstPage);
            this.visitPageCount = pageCount;
        }
    }
}
