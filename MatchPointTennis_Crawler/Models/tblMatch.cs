namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tblMatch
    {
        [Key]
        public long MatchID { get; set; }

        public long TeamMatchID { get; set; }

        public long? WinningTeamID { get; set; }

        [StringLength(50)]
        public string MatchType { get; set; }

        public long? Home_PlayerID_1 { get; set; }

        public long? Home_PlayerID_2 { get; set; }

        public long? Visiting_PlayerID_1 { get; set; }

        public long? Visiting_PlayerID_2 { get; set; }

        public int? Set1_HomeScore { get; set; }

        public int? Set1_VisitingScore { get; set; }

        public int? Set2_HomeScore { get; set; }

        public int? Set2_VisitingScore { get; set; }

        public int? Set3_HomeScore { get; set; }

        public int? Set3_VisitingScore { get; set; }

        public virtual tblTeamMatch tblTeamMatch { get; set; }

        public virtual tklTeam tklTeam { get; set; }

        public virtual tklUserList tklUserList { get; set; }

        public virtual tklUserList tklUserList1 { get; set; }

        public virtual tklUserList tklUserList2 { get; set; }

        public virtual tklUserList tklUserList3 { get; set; }
    }
}
