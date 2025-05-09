using System.ComponentModel.DataAnnotations;

namespace SDS.Models
{
    public class HeaderHImage
    {
        public int Id { get; set; }
        public string ContentID { get; set; } // Associate with SDSContent.ContentID
        public string ProductId { get; set; } // Associate images with a product
        public string ImageName { get; set; }
        public string ContentType { get; set; } // MIME type (e.g., "image/jpeg")
        public byte[] ImageData { get; set; } // Store the binary data of the image
        public string Base64Image { get; set; }

        [Range(1, 5)]
        public int Order { get; set; } // Limit to 5 images per ContentID
    }
}