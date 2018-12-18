using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawlerPageAnalysis
{
    class PageInfo
    {
        public Uri PageLink { get; set; }
        public IEnumerable<Uri> PageToOtherLinks { get; set; }
    }
}
