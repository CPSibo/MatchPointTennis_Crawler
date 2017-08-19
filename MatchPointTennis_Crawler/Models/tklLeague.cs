namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tklLeague")]
    public partial class tklLeague
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tklLeague()
        {
            tklFlights = new HashSet<tklFlight>();
        }

        [Key]
        public int LeagueID { get; set; }

        public long? USTAID { get; set; }

        public int LeagueYear { get; set; }

        public int AreaID { get; set; }

        [Required]
        [StringLength(200)]
        public string LeagueName { get; set; }

        public virtual tklArea tklArea { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tklFlight> tklFlights { get; set; }

        public virtual tklYear tklYear { get; set; }
    }
}
