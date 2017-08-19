namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tklSection
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tklSection()
        {
            tklDistricts = new HashSet<tklDistrict>();
        }

        [Key]
        public int SectionID { get; set; }

        [Required]
        [StringLength(200)]
        public string Section { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tklDistrict> tklDistricts { get; set; }
    }
}
