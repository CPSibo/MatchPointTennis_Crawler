namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tklDistrict
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tklDistrict()
        {
            tklAreas = new HashSet<tklArea>();
        }

        [Key]
        public int DistrictID { get; set; }

        public int SectionID { get; set; }

        [Required]
        [StringLength(200)]
        public string District { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tklArea> tklAreas { get; set; }

        public virtual tklSection tklSection { get; set; }
    }
}
