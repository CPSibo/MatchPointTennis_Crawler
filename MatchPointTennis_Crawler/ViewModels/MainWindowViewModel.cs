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
        {
            Crawler = new Crawler();
        }

        public ICommand Search => new DelegateCommand(() =>
        {
            DoSearch();
        });

        protected async Task DoSearch()
        {
            IsRunning = true;

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
