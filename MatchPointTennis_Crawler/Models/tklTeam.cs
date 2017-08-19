namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tklTeam
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tklTeam()
        {
            tblMatches = new HashSet<tblMatch>();
            tblTeamMatches = new HashSet<tblTeamMatch>();
            tblTeamMatches1 = new HashSet<tblTeamMatch>();
        }

        [Key]
        public long TeamID { get; set; }

        public long? USTAID { get; set; }

        [Required]
        [StringLength(50)]
        public string TeamName { get; set; }

        public string TeamFacility { get; set; }

        public long SubFlightID { get; set; }

        public byte TeamPlayerCount { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblMatch> tblMatches { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblTeamMatch> tblTeamMatches { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblTeamMatch> tblTeamMatches1 { get; set; }
    }
}
