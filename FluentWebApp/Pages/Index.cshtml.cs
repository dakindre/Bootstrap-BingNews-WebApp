using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FluentWebApp.Pages
{
    public class IndexModel : PageModel
    {

        const string accessKey = "e89c2365677a4de6a23b09eb982d6565";
        const string uriBase = "https://api.cognitive.microsoft.com/bing/v7.0/news/search";
        const ushort count = 21;
        const string sortBy = "Date";

        public struct SearchResult
        {
            public String jsonResult;
            public Dictionary<String, String> relevantHeaders;
        }

        public void OnGet()
        {
            SearchResult result = BingNewsSearch(count, sortBy);
            ViewData["News"] = CreateHeadlineList(result);

            ViewData["Publishers"] = CreatePublisherList(result);

        }

        public void OnPost()
        {
            SearchResult result = BingNewsSearch(count, sortBy);
            ViewData["News"] = CreateHeadlineList(result);
            ViewData["Publishers"] = CreatePublisherList(result);
        }

        public List<Object> CreateHeadlineList(SearchResult result)
        {
            var Message = new List<Object>();
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.jsonResult);

            for (int i = 0; i < jsonObj["value"].Count; i++)
            { Message.Add((jsonObj["value"][i]["name"])); }

            return Message;
        }

        public IOrderedEnumerable<object> CreatePublisherList(SearchResult result)
        {
            var Publishers = new List<Object>();
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.jsonResult);
            for (int i = 0; i < jsonObj["value"].Count; i++)
            { Publishers.Add(jsonObj["value"][i]["provider"][0]["name"]); }

            var pub = Publishers.GroupBy(x => x)
                .Select(g => new { Value = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);

            return pub;
        }

        static SearchResult BingNewsSearch(ushort count, string sortBy)
        {
            // Set URI parameters to sortBy Date and get count of 20 News articles
            var uriQuery = uriBase + "?q=" + "&sortBy=" + sortBy + "&count=" + count;

            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = accessKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Create result object for return
            var searchResult = new SearchResult();
            searchResult.jsonResult = json;
            searchResult.relevantHeaders = new Dictionary<String, String>();

            // Extract Bing HTTP headers
            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }

            return searchResult;
        }
    }
}

