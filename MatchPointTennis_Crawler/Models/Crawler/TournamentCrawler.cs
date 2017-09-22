using MatchPointTennis_Crawler.ViewModels;
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
    public class TournamentCrawler : Crawler
    {
        public TournamentCrawler(MainWindowViewModel viewModel)
            : base(viewModel)
        { }

        public override async Task Search()
        {
            ResetStats();

            ElapsedTimer.Restart();
            UpdateTimer.Start();

            Log = new StringBuilder();

            if (ViewModel.Rating > 0)
            {
                await RunSearch(ViewModel.Rating);
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

            var viewstate = await Browser.GetInitialViewState(ViewModel.Year.ToString(), ViewModel.Section, ViewModel.District);

            await new ScrapeProfiles.LeagueSearch_Teams(this)
                .CreateFormDataFor_TeamSearch
                (
                    viewstate: viewstate,
                    gender: ViewModel.Gender.Substring(0, 1),
                    rating: rating.ToString("0.0")
                )
                .Post();

            Log.Append($"Finished {ViewModel.Section} | {ViewModel.District} | {ViewModel.Area} | {ViewModel.Gender} | {ViewModel.Year} | {rating}{Environment.NewLine}");
            Log.Append($"\t{Elapsed:dd\\.hh\\:mm\\:ss} | {NumberOfRequests} Requests | {NumberOfBytes}{Environment.NewLine}{Environment.NewLine}");
            Log = Log;
        }
    }
}
