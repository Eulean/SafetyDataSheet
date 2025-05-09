using System.ComponentModel.DataAnnotations;

namespace SDS.Models
{

    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProductCode { get; set; }

        public string ProductNo { get; set; } // from autogenerate

        [Required]
        public string ProductName { get; set; }

        //public string? BioDef { get; set; }

        //public string? InciName { get; set; }

        //[MaxLength(50)]
        //public string? Cas { get; set; }

        //[MaxLength(50)]
        //public string? Fema { get; set; }

        //[MaxLength(50)]
        //public string? Einecs { get; set; }
    }
}
