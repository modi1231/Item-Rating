using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ItemRating.Data
{
    public class ITEM_RATING
    {
        [Key]
        public int ID_ITEM { get; set; }

        [Key]
        public int ID_USER { get; set; }

        public int RATING { get; set; }
        public DateTime DATE_ENTERED { get; set; }

        [NotMapped]
        public string USER_NAME { get; set; }
    }
}
