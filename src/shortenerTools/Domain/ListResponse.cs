using Shortener.Azure.Pocos;
using System.Collections.Generic;

namespace Cloud5mins.domain
{
    public class ListResponse
    {
        public List<ShortUrlPoco> UrlList { get; set; }

        public ListResponse() 
        {
            UrlList = new List<ShortUrlPoco>();
        }
        public ListResponse(List<ShortUrlPoco> list)
        {
            UrlList = list;
        }
    }
}