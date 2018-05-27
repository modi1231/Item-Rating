using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemRating.Data
{
    public class ITEM
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Title")]
        public string TITLE { get; set; }

        [NotMapped]
        public decimal RATING { get; set; }

        [NotMapped]
        public Int32 NEXT { get; set; }

        [NotMapped]
        public Int32 PREVIOUS { get; set; }
    }
}
