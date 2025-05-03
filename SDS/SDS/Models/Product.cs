using System;
using System.ComponentModel.DataAnnotations;

namespace SDS.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string ProductCode { get; set; } // from input
    public string ProductNo { get; set; } // from autogenerate
    public string ProductName { get; set; } // just product name
    // public string? BioDef { get; set; } // Biological Definition
    // public string? InciName { get; set; }

    // [MaxLength(50)]
    // public string? Cas { get; set; }

    // [MaxLength(50)]
    // public string? Fema { get; set; }

    // [MaxLength(50)]
    // public string? Einecs { get; set; }

    // public string? CreatedBy { get; set; }
    // public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

