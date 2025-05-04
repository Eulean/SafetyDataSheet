using System.ComponentModel.DataAnnotations;

namespace SDS.Models
{
    public class HeaderHImage
    {
        public int Id { get; set; }

        // public int HeaderHId { get; set; } // not needed for now
        // public HeadersH HeadersH { get; set; } // not needed for now

        // public int HeadersDataId { get; set; } // not needed for now
        // public HeadersData HeadersData { get; set; } // not needed for now

        public string ContentID { get; set; } // Associate with SDSContent.ContentID

        public string ProductId { get; set; } // Associate images with a product

        public string ImageName { get; set; }

        public string ContentType { get; set; } // MIME type (e.g., "image/jpeg")

        public byte[] ImageData { get; set; } // Store the binary data of the image

        [Range(1, 5)]
        public int Order { get; set; } // Limit to 5 images per ContentID
    }
}