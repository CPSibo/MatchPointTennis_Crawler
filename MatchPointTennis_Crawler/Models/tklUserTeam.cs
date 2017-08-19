namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tklUserTeam
    {
        [Key]
        public long TUID { get; set; }

        public long UserID { get; set; }

        public long TeamID { get; set; }
    }
}
