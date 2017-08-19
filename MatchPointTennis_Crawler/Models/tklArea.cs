namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tklArea")]
    public partial class tklArea
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tklArea()
        {
            tklLeagues = new HashSet<tklLeague>();
        }

        [Key]
        public int AreaID { get; set; }

        public int DistrictID { get; set; }

        [Required]
        [StringLength(200)]
        public string Area { get; set; }

        public virtual tklDistrict tklDistrict { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tklLeague> tklLeagues { get; set; }
    }
}
