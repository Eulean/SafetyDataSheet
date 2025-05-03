using System;

namespace SDS.Models;

public class SDSContent
{
    public int Id { get; set; }
    public string Content { get; set; } //  Input Data content

    public string ContentID { get; set; } // Header Data content

    public string? HeadersHId { get; set; } // Foreign key to HeadersH table (nullable)

    public string? HeadersHDId { get; set; } // Foreign key to HeadersHD table (nullable)

    public string? ProductId { get; set; } // Foreign key to Product table


}

