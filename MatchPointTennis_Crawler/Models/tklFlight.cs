namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tklFlight
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tklFlight()
        {
            tklSubFlights = new HashSet<tklSubFlight>();
        }

        [Key]
        public long FlightID { get; set; }

        public long? USTAID { get; set; }

        public int LeagueID { get; set; }

        public decimal FlightLevel { get; set; }

        [Required]
        [StringLength(10)]
        public string FlightGender { get; set; }

        public virtual tklLeague tklLeague { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tklSubFlight> tklSubFlights { get; set; }
    }
}
