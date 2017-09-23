using MatchPointTennis_Crawler.Models.Crawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MatchPointTennis_Crawler.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private Crawler _crawler;
        public Crawler Crawler
        {
            get => _crawler;
            set
            {
                _crawler = value;

                NotifyPropertyChanged("Crawler");
            }
        }

        public int Year { get; set; } = 2014;

        public string Section { get; set; } = "USTA/INTERMOUNTAIN";

        public string District { get; set; } = "COLORADO";

        public string Area { get; set; }

        public string Gender { get; set; } = "Female";

        public decimal Rating { get; set; } = 0.0m;

        public List<string> Modes { get; set; } = new List<string>()
        {
            "League Matches" ,
            "Championships",
            "Player Rating Types",
            "Dropdowns",
            "Tournaments",
        };

        public int Mode { get; set; } = 0;

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;

                NotifyPropertyChanged("IsRunning");
                NotifyPropertyChanged("ControlsEnabled");
            }
        }

        public bool ControlsEnabled => !IsRunning;

        public MainWindowViewModel()
        { }

        public ICommand Search => new DelegateCommand(() =>
        {
            DoSearch();
        });

        protected async Task DoSearch()
        {
            IsRunning = true;

            switch(Mode)
            {
                case 0:
                    Crawler = new LeagueMatchCrawler(this);
                    break;

                case 1:
                    Crawler = new ChampionshipCrawler(this);
                    break;

                case 2:
                    break;

                case 3:
                    break;

                case 4:
                    break;
            }

            await Crawler.Search();

            IsRunning = false;
        }

        //public async Task TeamSummaryDirect(double id = 2572278808, int year = 2017)
        //{
        //    var item = await new ScrapeProfiles.Team()
        //            .CreateFormDataFor_Direct(id.ToString(), year.ToString())
        //            .Post();
        //}
    }
}
