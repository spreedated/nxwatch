using HtmlAgilityPack;
using Scraper.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Scraper
{
    public class Scraper : IDisposable
    {
        internal HttpClient httpClient;

        /// <summary>
        /// Remove "Update" category from the result
        /// </summary>
        public bool FilterUpdates { get; set; } = true;

        /// <summary>
        /// Number of available pages<br/>
        /// Refresh by using <see cref="GetPagecount"/>
        /// </summary>
        public int Pagecount { get; set; }

        public event EventHandler TaskCanceled;
        public event EventHandler<GameRetrievedEventArgs> GameRetrieved;

        #region Constructor
        public Scraper()
        {
            this.httpClient = new()
            {
                Timeout = TimeSpan.FromSeconds(10),
                DefaultRequestHeaders =
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246" }
                }
            };
        }
        #endregion

        #region Static
        private static IEnumerable<string> GetCategories(HtmlNode htmlNode)
        {
            HtmlNodeCollection htmlNodes = htmlNode.SelectNodes(".//div[@class='post-category']/a");

            if (htmlNodes == null || htmlNodes.Count <= 0)
            {
                return Array.Empty<string>();
            }

            return htmlNodes.Select(x => x.InnerText);
        }
        #endregion

        public async Task<int> GetPagecount()
        {
            string doc = null;

            using (HttpResponseMessage m = await this.httpClient.GetAsync($"https://nxbrew.com/"))
            {
                if (!m.IsSuccessStatusCode)
                {
                    return default;
                }

                doc = await m.Content.ReadAsStringAsync();
            }

            HtmlDocument d = new();
            d.LoadHtml(doc);

            _ = int.TryParse(d.DocumentNode.SelectSingleNode("//a[@aria-label='Last Page']")?.GetAttributeValue<string>("href", null)?.TrimEnd('/')[24..], out int p);
            this.Pagecount = p;

            return p;
        }

        private static string BuildUrl(int page)
        {
            if (page <= 1)
            {
                return "https://nxbrew.com/";
            }

            return $"https://nxbrew.com/page/{page}/";
        }

        private async Task<HtmlNodeCollection> GetNodeCollection(int page)
        {
            string doc = null;

            try
            {
                using (HttpResponseMessage m = await this.httpClient.GetAsync(BuildUrl(page)))
                {
                    //# ### # 
                    if (!m.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    doc = await m.Content.ReadAsStringAsync();
                }
            }
            catch (TaskCanceledException)
            {
                this.TaskCanceled?.Invoke(this, EventArgs.Empty);
                return null;
            }

            HtmlDocument d = new();
            d.LoadHtml(doc);

            return d.DocumentNode.SelectNodes("//div[@class='content']//div[@class='post-content']");
        }

        /// <summary>
        /// Retrieve games from a specific page<br/>
        /// Default is the first page
        /// </summary>
        /// <param name="page">Select the page you want to retrieve the games from</param>
        /// <returns></returns>
        public async Task<IEnumerable<SwitchGame>> GetGamesFromPage(int page = 1)
        {
            HtmlNodeCollection nodes = await this.GetNodeCollection(page);

            if (nodes == null)
            {
                return Array.Empty<SwitchGame>();
            }

            List<SwitchGame> games = [];

            foreach (HtmlNode c in nodes)
            {
                HtmlNode link = c.SelectSingleNode(".//a[@rel='bookmark']");

                string[] categories = GetCategories(c).ToArray();

                if (this.FilterUpdates && categories.Length >= 1 && categories.Any(x => x.Contains("update", StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                games.Add(new SwitchGame()
                {
                    Categories = categories.Length != 0 ? categories : null,
                    Date = DateTime.Now,
                    Name = link.InnerText,
                    Link = link.Attributes["href"].Value,
                    NxDate = DateTime.Parse(c.SelectSingleNode(".//div[@class='post-date']").InnerText, CultureInfo.InvariantCulture)
                });

                this.GameRetrieved?.Invoke(this, new(games[^1]));
            }

            return games;
        }

        public async Task<IEnumerable<SwitchGame>> GetGamesFromPages(int from, int to)
        {
            if (to < from || to - from == 0)
            {
                return Array.Empty<SwitchGame>();
            }

            List<SwitchGame> games = [];

            for (int i = from; i < to + 1; i++)
            {
                games.AddRange(await this.GetGamesFromPage(i));
            }

            return games;
        }

        #region Dispose
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.httpClient?.Dispose();
        }
        #endregion
    }
}
