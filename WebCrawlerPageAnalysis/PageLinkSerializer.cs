using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawlerPageAnalysis
{
    class PageLinkSerializer
    {
        private IList<PageInfo> pages { get; set; }

        public string GetMatrix()
        {
            var lines = pages.Select(p => new { Link = p.PageLink.GetLeftPart(UriPartial.Path), LinkTo = new int[pages.Count] });
            var text = "";
            pages.ToList().ForEach(p =>
            {
                var line = lines.FirstOrDefault(l => l.Link == p.PageLink.GetLeftPart(UriPartial.Path));
                p.PageToOtherLinks.ToList().ForEach(l =>
                {
                    if (pages.FirstOrDefault(s => s.PageLink.GetLeftPart(UriPartial.Path) == l.GetLeftPart(UriPartial.Path)) != null)
                    {
                        var index = pages.IndexOf(pages.First(c => c.PageLink.GetLeftPart(UriPartial.Path) == l.GetLeftPart(UriPartial.Path)));
                        line.LinkTo[index] += 1;
                    }
                });
                text += string.Join(",", line.LinkTo.Select(a => a.ToString())) + "\n";
            });
            return text;
        }

        public string GetSerializedText()
        {
            var text = pages.Count().ToString() + "\n";
            text += string.Join("\n", pages.Select(l => l.PageLink.GetLeftPart(UriPartial.Path))) + "\n";
            text += GetMatrix();
            return text;
        }

        public PageLinkSerializer(IList<PageInfo> pages)
        {
            this.pages = pages;
        }
    }
}
