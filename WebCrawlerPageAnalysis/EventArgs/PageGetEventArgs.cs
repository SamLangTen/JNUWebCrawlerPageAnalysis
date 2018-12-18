using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawlerPageAnalysis.EventArgs
{
    class PageGetEventArgs : System.EventArgs
    {
        public PageInfo PageInfo { get; set; }
    }
}
