namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tklSubFlight")]
    public partial class tklSubFlight
    {
        [Key]
        public long SubFlightID { get; set; }

        public long? USTAID { get; set; }

        public long FlightID { get; set; }

        [Required]
        [StringLength(200)]
        public string SubFlight { get; set; }

        public virtual tklFlight tklFlight { get; set; }
    }
}
