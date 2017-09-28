namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tblTeamMatch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblTeamMatch()
        {
            tblMatches = new HashSet<tblMatch>();
        }

        [Key]
        public long TeamMatchID { get; set; }

        public long? USTAID { get; set; }

        public long HomeTeamID { get; set; }

        public long VisitingTeamID { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? DateScheduled { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? DatePlayed { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? DateEntered { get; set; }

        public long? ChampionshipID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblMatch> tblMatches { get; set; }

        public virtual tklTeam tklTeam { get; set; }

        public virtual tklTeam tklTeam1 { get; set; }
    }
}
