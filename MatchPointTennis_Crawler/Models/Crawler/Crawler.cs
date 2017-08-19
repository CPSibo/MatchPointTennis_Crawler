using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MatchPointTennis_Crawler.Models.Crawler
{
    public class Crawler : ObservableObject
    {
        public int Year { get; set; }

        public string Section { get; set; }

        public string District { get; set; }

        public string Area { get; set; }

        public string Gender { get; set; }

        public decimal Rating { get; set; }

        private int _numberOfItems = 0;

        public int NumberOfItems
        {
            get => _numberOfItems;
            set
            {
                _numberOfItems = value;

                NotifyPropertyChanged("NumberOfItems");
                NotifyPropertyChanged("ProgressText");
            }
        }

        private int _itemsProcessed = 0;

        public int ItemsProcessed
        {
            get => _itemsProcessed;
            set
            {
                _itemsProcessed = value;

                NotifyPropertyChanged("ItemsProcessed");
                NotifyPropertyChanged("ProgressText");
            }
        }

        private TimeSpan _elapsed = TimeSpan.FromMilliseconds(0);

        public TimeSpan Elapsed
        {
            get => _elapsed;
            set
            {
                _elapsed = value;

                NotifyPropertyChanged("Elapsed");
            }
        }

        private TimeSpan _eta;

        public TimeSpan ETA
        {
            get => _eta;
            set
            {
                _eta = value;

                NotifyPropertyChanged("ETA");
            }
        }

        private UInt64 _numberOfRequests;

        public UInt64 NumberOfRequests
        {
            get => _numberOfRequests;
            set
            {
                _numberOfRequests = value;

                NotifyPropertyChanged("NumberOfRequests");
            }
        }

        private string _numberOfBytes;

        public string NumberOfBytes
        {
            get => _numberOfBytes;
            set
            {
                _numberOfBytes = value;

                NotifyPropertyChanged("NumberOfBytes");
            }
        }

        public string ProgressText => $"{ItemsProcessed}/{NumberOfItems}";

        private Stopwatch ElapsedTimer { get; set; } = new Stopwatch();

        private Timer UpdateTimer { get; set; } = new Timer(1000);

        public Crawler()
        {
            Year = 2014;
            Section = "USTA/INTERMOUNTAIN";
            District = "COLORADO";
            Gender = "Female";
            Rating = 2.0M;

            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;

            Mediator.Instance.Register((object args) =>
            {
                ItemsProcessed++;
            }, ViewModelMessages.TeamProcessed);

            Mediator.Instance.Register((object args) =>
            {
                NumberOfItems = (int)args;
            }, ViewModelMessages.TeamsCollected);

            Mediator.Instance.Register((object args) =>
            {
                NumberOfRequests++;
            }, ViewModelMessages.RequestSent);

            Mediator.Instance.Register((object args) =>
            {
                NumberOfBytes = ((long)args).BytesToString();
            }, ViewModelMessages.RequestReceived);
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Elapsed = ElapsedTimer.Elapsed;

            if (ItemsProcessed > 0)
            {
                var itemsRemaining = NumberOfItems - ItemsProcessed;

                var timePerItem = ElapsedTimer.ElapsedMilliseconds / (long)ItemsProcessed;

                ETA = TimeSpan.FromMilliseconds(itemsRemaining * timePerItem);
            }
        }

        public async Task Search()
        {
            var viewstate = await Browser.GetInitialViewState(Year.ToString(), Section, District);

            ElapsedTimer.Restart();
            UpdateTimer.Start();

            await new ScrapeProfiles.LeagueSearch_Teams(this)
                .CreateFormDataFor_TeamSearch
                (
                    viewstate: viewstate,
                    gender: Gender.Substring(0, 1),
                    rating: Rating.ToString("0.0")
                )
                .Post();

            ElapsedTimer.Stop();

            //ETAText = "Finished!";
        }
    }
}
