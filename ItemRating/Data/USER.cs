 using System.ComponentModel.DataAnnotations;
 

namespace ItemRating.Data
{
    public class USER
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Name")]
        public string NAME { get; set; }
    }
}
