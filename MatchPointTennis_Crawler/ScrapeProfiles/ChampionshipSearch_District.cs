using AngleSharp.Dom.Html;
using MatchPointTennis_Crawler.Models;
using MatchPointTennis_Crawler.Models.Crawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler.ScrapeProfiles
{
    public class ChampionshipSearch_District
        : ScrapeProfile<object>
    {
        private struct Team
        {
            public string Name { get; set; }

            public string Section { get; set; }

            public string District { get; set; }

            public string League { get; set; }

            public string Flight { get; set; }
        }

        public ChampionshipSearch_District(Crawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_pnlCPDrillDownAnchor";
        }

        public ChampionshipSearch_District CreateFormDataFor_DistrictSearch
        (
            Dictionary<string, string> postData
        )
        {
            FormData = postData;

            // Click the search button.
            FormData["ctl00$ScriptManager1"] = "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$btn_District";
            FormData.Add("ctl00$mainContent$btn_District", "View District Championships");

            return this;
        }

        protected async override Task<object> DoParse()
        {
            if(Document.Query("#ctl00_mainContent_tblNoChampionships") != null)
            {
                Mediator.Instance.Notify(ViewModelMessages.ItemsCollected, 0);
                Mediator.Instance.Notify(ViewModelMessages.Finished, 0);

                return null;
            }

            var table = Document.Query("#ctl00_mainContent_tblChampionshipListHeader + table + table") as IHtmlTableElement;

            var championshipRows = table.Rows.Take(table.Rows.Count() - 1).Skip(1);

            Mediator.Instance.Notify(ViewModelMessages.ItemsCollected, championshipRows.Count());

            using (var semaphore = new System.Threading.SemaphoreSlim(1))
            {
                await Task.WhenAll(championshipRows.Select(async championship =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        var item = await new ScrapeProfiles.ChampionshipReport(Crawler)
                            .CreateFormDataFor_FromFlightSearch(championship.Cells[3].QuerySelector("a:first-of-type")?.Id, ReturnedViewstate)
                            .Post();
                        
                        Mediator.Instance.Notify(ViewModelMessages.ItemProcessed, item);
                    }
                    catch
                    {
                        Mediator.Instance.Notify(ViewModelMessages.ItemFailed, championship.Cells[3].QuerySelector("a:first-of-type").InnerHtml.Cleanse());
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            return null;
        }
    }
}
