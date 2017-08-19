using AngleSharp.Dom.Html;
using MatchPointTennis_Crawler.Models.Crawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler.ScrapeProfiles
{
    public abstract class ScrapeProfile<T> where T : class
    {
        protected string LoadedElementID { get; set; }

        public string Result { get; protected set; }

        public IHtmlDocument Document { get; protected set; }

        protected Dictionary<string, string> FormData { get; set; }

        public string Path { get; set; } = "/leagues/Main/StatsAndStandings.aspx";

        protected long? USTAId { get; set; }

        protected string ReturnedViewstate { get; set; }

        protected Crawler Crawler { get; set; }

        public ScrapeProfile(Crawler crawler)
        {
            Crawler = crawler;
        }

        protected virtual bool IsLoaded()
        {
            if (LoadedElementID == null)
            {
                throw new Exception("LoadedElementID cannot be null!");
            }

            return Document?.Query(LoadedElementID) != null;
        }

        protected long? GetUSTAId()
        {
            if (!long.TryParse(Parser.GetUSTAId(Document), out long id))
            {
                id = 0;
            }

            if (id == 0)
            {
                USTAId = null;
            }
            else
            {
                USTAId = id;
            }

            return USTAId;
        }

        public async Task<T> Post()
        {
            if(FormData == null)
            {
                return null;
            }

            Result = await Browser.SendRequest(Path, FormData);

            return await Parse();
        }

        protected async Task<T> Parse()
        {
            ReturnedViewstate = Parser.GetViewState(Result);

            Document = await Parser.Parse(Parser.GetMainContent(Result));

            if (!IsLoaded())
            {
                return null;
            }

            GetUSTAId();

            return await DoParse();
        }

        protected abstract Task<T> DoParse();
    }
}
