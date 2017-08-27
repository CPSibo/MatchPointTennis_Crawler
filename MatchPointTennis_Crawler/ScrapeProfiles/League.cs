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
    public class League
        : ScrapeProfile<tklLeague>
    {
        public League(Crawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_tblLeagueAnchor";
        }

        public League CreateFormDataFor_FromTeam(string linkId, string viewstate)
        {
            if (string.IsNullOrWhiteSpace(linkId))
            {
                return null;
            }

            var correctedId = linkId.Replace('_', '$');

            FormData = new Dictionary<string, string>() {
                { "ctl00$ScriptManager1", $"ctl00$mainContent$UpdatePanel1|{correctedId}" },
                { "ctl00$mainContent$hdnSearchType", "DefaultType"},
                { "__EVENTTARGET", correctedId},
                { "__EVENTARGUMENT", ""},
                { "__ASYNCPOST", "true"},
                { "__VIEWSTATE", viewstate },
                { "__VIEWSTATEGENERATOR", "FAFE42EE"},
            };

            return this;
        }

        protected async override Task<tklLeague> DoParse()
        {
            var league = new Repository().Get<tklLeague>(f => f.USTAID == USTAId);

            if (league != null)
            {
                return league;
            }

            var summaryTable = Document.Query("#ctl00_mainContent_tblLeagueAnchor") as IHtmlTableElement;

            var section = ProcessSection(summaryTable);
            var district = ProcessDistrict(summaryTable, section);
            var area = ProcessArea(summaryTable, district);
            var year = ProcessYear();

            var leagueLeagueTypeText = summaryTable.Rows[1].Cells[0].QuerySelector("strong").InnerHtml.Cleanse();

            league = new tklLeague()
            {
                USTAID = USTAId,
                LeagueName = leagueLeagueTypeText,
                AreaID = area.AreaID,
                LeagueYear = year.Year
            };

            new Repository().Add(league).Save(league);

            return league;
        }

        private tklYear ProcessYear()
        {
            var year = new Repository().Get<tklYear>(f => f.Year == Crawler.Year);
            if (year == null)
            {
                year = new tklYear()
                {
                    Year = Crawler.Year
                };
                new Repository().Add(year).Save(year);
            }

            return year;
        }

        private tklSection ProcessSection(IHtmlTableElement summaryTable)
        {
            var sectionText = summaryTable.Rows[1].Cells[1].QuerySelector("strong").InnerHtml.Cleanse();
            var section = new Repository().Get<tklSection>(f => f.Section == sectionText);
            if (section == null)
            {
                section = new tklSection()
                {
                    Section = sectionText
                };
                new Repository().Add(section).Save(section);
            }

            return section;
        }

        private tklDistrict ProcessDistrict(IHtmlTableElement summaryTable, tklSection section)
        {
            var districtText = summaryTable.Rows[1].Cells[2].QuerySelector("strong").InnerHtml.Split(new char[] { '-' }, 2)[0].Cleanse();
            var district = new Repository().Get<tklDistrict>(f => f.District == districtText && f.SectionID == section.SectionID);
            if (district == null)
            {
                district = new tklDistrict()
                {
                    District = districtText,
                    SectionID = section.SectionID
                };
                new Repository().Add(district).Save(district);
            }

            return district;
        }

        private tklArea ProcessArea(IHtmlTableElement summaryTable, tklDistrict district)
        {
            var areaText = summaryTable.Rows[1].Cells[2].QuerySelector("strong").InnerHtml.Split(new char[] { '-' }, 2)[1].Cleanse();
            var area = new Repository().Get<tklArea>(f => f.Area == areaText && f.DistrictID == district.DistrictID);
            if (area == null)
            {
                area = new tklArea()
                {
                    Area = areaText,
                    DistrictID = district.DistrictID
                };
                new Repository().Add(area).Save(area);
            }

            return area;
        }
    }
}
