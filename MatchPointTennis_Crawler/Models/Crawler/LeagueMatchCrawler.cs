using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace MatchPointTennis_Crawler.Models.Crawler
{
    public class LeagueMatchCrawler : Crawler
    {
        public LeagueMatchCrawler()
            : base()
        { }

        public override async Task Search()
        {
            Browser.NumberOfRequests = 0;
            Browser.NumberOfBytesTransfered = 0;

            ItemsProcessed = 0;
            NumberOfItems = 0;
            NumberOfBytes = 0;
            NumberOfRequests = 0;
            Elapsed = TimeSpan.FromSeconds(0);
            ETA = TimeSpan.FromSeconds(0);

            TotalItemsProcessed = 0;
            TotalNumberOfItems = 0;
            TotalNumberOfBytes = 0;
            TotalNumberOfRequests = 0;
            TotalElapsed = TimeSpan.FromSeconds(0);

            ElapsedTimer.Restart();
            UpdateTimer.Start();

            Log = new StringBuilder();

            if (Rating > 0)
            {
                await RunSearch(Rating);
            }
            else
            {
                var rating = 2.0m;

                while (rating < 12.5m)
                {
                    await RunSearch(rating);

                    rating += 0.5m;
                }
            }

            ElapsedTimer.Stop();
            UpdateTimer.Stop();

            Log.Append("FINISHED!");
            Log = Log;
        }

        protected async Task RunSearch(decimal rating)
        {
            ItemsProcessed = 0;
            NumberOfBytes = 0;
            NumberOfRequests = 0;
            Browser.NumberOfRequests = 0;
            Browser.NumberOfBytesTransfered = 0;

            var viewstate = await Browser.GetInitialViewState(Year.ToString(), Section, District);

            await new ScrapeProfiles.LeagueSearch_Teams(this)
                .CreateFormDataFor_TeamSearch
                (
                    viewstate: viewstate,
                    gender: Gender.Substring(0, 1),
                    rating: rating.ToString("0.0")
                )
                .Post();

            Log.Append($"Finished {Section} | {District} | {Area} | {Gender} | {Year} | {rating}{Environment.NewLine}");
            Log.Append($"\t{Elapsed:dd\\.hh\\:mm\\:ss} | {NumberOfRequests} Requests | {NumberOfBytes}{Environment.NewLine}{Environment.NewLine}");
            Log = Log;
        }
    }
}
