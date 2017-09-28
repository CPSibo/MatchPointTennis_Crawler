namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tblChampionship
    {
        [Key]
        public long ChampionshipID { get; set; }

        public string Name { get; set; }

        public double Rating { get; set; }

        [Required]
        [StringLength(1)]
        public string Gender { get; set; }

        [Required]
        public string Level { get; set; }

        public long OwnerID { get; set; }

        public long? USTAID { get; set; }
    }
}
