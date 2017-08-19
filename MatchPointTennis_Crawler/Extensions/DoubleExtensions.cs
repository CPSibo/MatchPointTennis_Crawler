using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler
{
    public static class DoubleExtensions
    {
        public static string BytesToString(this double byteCount)
        {
            string[] suf = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };
            if (byteCount == 0)
                return "0" + suf[0];
            double bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}
