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
    public abstract class Crawler : ObservableObject
    {
        public MainWindowViewModel ViewModel { get; set; }

        private StringBuilder _log = new StringBuilder();
        public StringBuilder Log
        {
            get => _log;

            set
            {
                _log = value;

                NotifyPropertyChanged("Log");
                NotifyPropertyChanged("LogValue");
            }
        }

        public string LogValue => _log.ToString();

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

        private int _totalNumberOfItems = 0;
        public int TotalNumberOfItems
        {
            get => _totalNumberOfItems;
            set
            {
                _totalNumberOfItems = value;

                NotifyPropertyChanged("TotalNumberOfItems");
                NotifyPropertyChanged("TotalProgressText");
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

        private int _totalItemsProcessed = 0;
        public int TotalItemsProcessed
        {
            get => _totalItemsProcessed;
            set
            {
                _totalItemsProcessed = value;

                NotifyPropertyChanged("TotalItemsProcessed");
                NotifyPropertyChanged("TotalProgressText");
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

        private TimeSpan _totalElapsed = TimeSpan.FromMilliseconds(0);
        public TimeSpan TotalElapsed
        {
            get => _totalElapsed;
            set
            {
                _totalElapsed = value;

                NotifyPropertyChanged("TotalElapsed");
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

        private UInt64 _totalNumberOfRequests;
        public UInt64 TotalNumberOfRequests
        {
            get => _totalNumberOfRequests;
            set
            {
                _totalNumberOfRequests = value;

                NotifyPropertyChanged("TotalNumberOfRequests");
            }
        }

        private long _numberOfBytes;
        public long NumberOfBytes
        {
            get => _numberOfBytes;
            set
            {
                _numberOfBytes = value;

                NotifyPropertyChanged("NumberOfBytes");
            }
        }

        private long _totalNumberOfBytes;
        public long TotalNumberOfBytes
        {
            get => _totalNumberOfBytes;
            set
            {
                _totalNumberOfBytes = value;

                NotifyPropertyChanged("TotalNumberOfBytes");
            }
        }

        public string ProgressText => $"{ItemsProcessed}/{NumberOfItems}";

        protected Stopwatch ElapsedTimer { get; set; } = new Stopwatch();

        protected DispatcherTimer UpdateTimer { get; set; } = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

        public Crawler(MainWindowViewModel viewModel)
        {
            ViewModel = viewModel;

            UpdateTimer.Tick += UpdateTimer_Tick;

            Mediator.Instance.Register((object args) =>
            {
                ItemsProcessed++;
                TotalItemsProcessed++;
            }, ViewModelMessages.ItemProcessed);

            Mediator.Instance.Register((object args) =>
            {
                NumberOfItems = (int)args;
                TotalNumberOfItems += (int)args;
            }, ViewModelMessages.ItemsCollected);

            Mediator.Instance.Register((object args) =>
            {
                NumberOfRequests++;
                TotalNumberOfRequests++;
            }, ViewModelMessages.RequestSent);

            Mediator.Instance.Register((object args) =>
            {
                NumberOfBytes += (long)args;
                TotalNumberOfBytes += (long)args;
            }, ViewModelMessages.RequestReceived);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var lastElapsed = Elapsed;
            Elapsed = ElapsedTimer.Elapsed;
            var diff = Elapsed - lastElapsed;
            TotalElapsed += diff;

            if (ItemsProcessed > 0)
            {
                var itemsRemaining = NumberOfItems - ItemsProcessed;

                var timePerItem = ElapsedTimer.ElapsedMilliseconds / (long)ItemsProcessed;

                ETA = TimeSpan.FromMilliseconds(itemsRemaining * timePerItem);
            }
        }

        public void ResetStats()
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

            Log = new StringBuilder();

            ElapsedTimer.Stop();
            UpdateTimer.Stop();
        }

        public abstract Task Search();
    }
}
