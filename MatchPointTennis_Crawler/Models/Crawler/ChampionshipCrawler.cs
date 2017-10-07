using AngleSharp.Dom.Html;
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
    public class ChampionshipCrawler : Crawler
    {
        public ChampionshipCrawler(MainWindowViewModel viewModel)
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
            string response;
            string searchPage = "/leagues/Main/StatsAndStandings.aspx";

            ItemsProcessed = 0;
            NumberOfBytes = 0;
            NumberOfRequests = 0;
            Browser.NumberOfRequests = 0;
            Browser.NumberOfBytesTransfered = 0;

            (var postData, var doc) = await GetInitialViewState(rating.ToString("0.0"));

            try
            {
                if (ViewModel.SubMode == 1)
                {
                    // Permutate areas...
                    foreach (var area in ((IHtmlSelectElement)doc.Query("#ctl00_mainContent_ddlAreaChampion")).Options.Where(f => f.Value != "0").Select(f => f.InnerHtml))
                    {
                        var areaPostData = SelectIntoPostData(postData, doc,
                           "ctl00_mainContent_ddlAreaChampion", "ctl00$mainContent$ddlAreaChampion", area);
                        response = await Browser.SendRequest(searchPage, areaPostData, true);
                        areaPostData["__VIEWSTATE"] = Parser.GetViewState(response);
                        var areaDoc = await Parser.Parse(response);

                        // Permutate leagues...
                        foreach (var league in ((IHtmlSelectElement)areaDoc.Query("#ctl00_mainContent_ddlLeagueChampion")).Options.Where(f => f.Value != "0").Select(f => f.InnerHtml))
                        {
                            var leaguePostData = SelectIntoPostData(areaPostData, areaDoc,
                               "ctl00_mainContent_ddlLeagueChampion", "ctl00$mainContent$ddlLeagueChampion", league);
                            response = await Browser.SendRequest(searchPage, leaguePostData, true);
                            leaguePostData["__VIEWSTATE"] = Parser.GetViewState(response);
                            var leagueDoc = await Parser.Parse(response);

                            // Permutate flights...
                            foreach (var flight in ((IHtmlSelectElement)leagueDoc.Query("#ctl00_mainContent_ddlFlightChampion")).Options.Where(f => f.Value != "0").Select(f => f.InnerHtml))
                            {
                                var flightPostData = SelectIntoPostData(leaguePostData, leagueDoc,
                                   "ctl00_mainContent_ddlFlightChampion", "ctl00$mainContent$ddlFlightChampion", flight);
                                response = await Browser.SendRequest(searchPage, flightPostData, true);
                                flightPostData["__VIEWSTATE"] = Parser.GetViewState(response);
                                var flightDoc = await Parser.Parse(response);



                                await new ScrapeProfiles.ChampionshipSearch_Flight(this)
                                    .CreateFormDataFor_FlightSearch(flightPostData)
                                    .Post();
                            }
                        }
                    }
                }
                else
                {
                    await new ScrapeProfiles.ChampionshipSearch_District(this)
                        .CreateFormDataFor_DistrictSearch(postData)
                        .Post();
                }
            }
            catch (Exception ex)
            { }

            Log.Append($"Finished {ViewModel.Section} | {ViewModel.District} | {ViewModel.Area} | {ViewModel.Gender} | {ViewModel.Year} | {rating}{Environment.NewLine}");
            Log.Append($"\t{Elapsed:dd\\.hh\\:mm\\:ss} | {NumberOfRequests} Requests | {NumberOfBytes}{Environment.NewLine}{Environment.NewLine}");
            Log = Log;
        }

        public async Task<(Dictionary<string, string>, IHtmlDocument)> GetInitialViewState(string rating)
        {
            /*
             * GET BASE VIEWSTATE
             */
            var postData = new Dictionary<string, string>()
            {
                {"ctl00$ScriptManager1", ""},
                {"__EVENTTARGET", ""},
                {"__EVENTARGUMENT", ""},
                {"__ASYNCPOST", "true"},
                {"__VIEWSTATE", ""},
            };

            string searchPage = "/leagues/Main/StatsAndStandings.aspx";

            // Get a blank viewstate to seed everything else.
            var response = await Browser.SendRequest(searchPage, null, true);
            postData["__VIEWSTATE"] = Parser.GetViewState(response);
            var doc = await Parser.Parse(response);



            /*
             * SELECT YEAR
             */
            postData = SelectIntoPostData(postData, doc,
                "ctl00_mainContent_ddlCYear", "ctl00$mainContent$ddlCYear", ViewModel.Year.ToString());

            if (((IHtmlSelectElement)doc.QuerySelector("#ctl00_mainContent_ddlCYear")).Value != ViewModel.Year.ToString())
            {
                response = await Browser.SendRequest(searchPage, postData, true);
                postData["__VIEWSTATE"] = Parser.GetViewState(response);
                doc = await Parser.Parse(response);
            }



            /*
             * ADD NON-POSTBACK SELECTIONS
             */
            postData.Add("ctl00$mainContent$ddlNTRPlevelChampionlevel", rating);
            postData.Add("ctl00$mainContent$ddlGenderChampion", ViewModel.Gender.ToUpper()[0].ToString());



            /*
             * SELECT LEVEL
             */
            postData = SelectIntoPostData(postData, doc, 
                "ctl00_mainContent_ddlClevel", "ctl00$mainContent$ddlClevel", "Flight Championships");
            response = await Browser.SendRequest(searchPage, postData, true);
            postData["__VIEWSTATE"] = Parser.GetViewState(response);
            doc = await Parser.Parse(response);



            /*
             * SELECT SECTION
             */
            postData = SelectIntoPostData(postData, doc,
                "ctl00_mainContent_ddlSectionChampion", "ctl00$mainContent$ddlSectionChampion", ViewModel.Section);
            response = await Browser.SendRequest(searchPage, postData, true);
            postData["__VIEWSTATE"] = Parser.GetViewState(response);
            doc = await Parser.Parse(response);



            /*
             * SELECT DISTRICT
             */
            postData = SelectIntoPostData(postData, doc,
                "ctl00_mainContent_ddlDistrictChampion", "ctl00$mainContent$ddlDistrictChampion", ViewModel.District);
            response = await Browser.SendRequest(searchPage, postData, true);
            postData["__VIEWSTATE"] = Parser.GetViewState(response);
            doc = await Parser.Parse(response);



            return (postData, doc);
        }

        protected Dictionary<string, string> SelectIntoPostData
        (
            Dictionary<string, string> postData, 
            IHtmlDocument doc,
            string elementId,
            string elementName,
            string valueToSelect
        )
        {
            var selectionValue = doc.Query($"#{elementId}").OptionWithText(valueToSelect)?.Value;

            if (selectionValue == null)
            {
                throw new Exception($"Value '{valueToSelect}' not found!");
            }

            var newPostDate = new Dictionary<string, string>(postData);

            newPostDate["ctl00$ScriptManager1"] = $"ctl00$mainContent$UpdatePanel1|{elementName}";
            newPostDate["__EVENTTARGET"] = elementName;

            if(newPostDate.ContainsKey(elementName))
            {
                newPostDate[elementName] = selectionValue;
            }
            else
            {
                newPostDate.Add(elementName, selectionValue);
            }

            return newPostDate;
        }
    }
}
