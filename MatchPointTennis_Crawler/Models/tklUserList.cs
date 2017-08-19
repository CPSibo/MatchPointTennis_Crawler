namespace MatchPointTennis_Crawler.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tklUserList")]
    public partial class tklUserList
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tklUserList()
        {
            tblMatches = new HashSet<tblMatch>();
            tblMatches1 = new HashSet<tblMatch>();
            tblMatches2 = new HashSet<tblMatch>();
            tblMatches3 = new HashSet<tblMatch>();
        }

        [Key]
        public long UserID { get; set; }

        public long? USTAID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public decimal? InitialRating { get; set; }

        public int? CurrentYear { get; set; }

        public int? InitialYear { get; set; }

        [StringLength(200)]
        public string City { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblMatch> tblMatches { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblMatch> tblMatches1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblMatch> tblMatches2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblMatch> tblMatches3 { get; set; }
    }
}
