using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CLDVPOE1.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        public string? VenueName { get; set; }

        [Required]
        public string? Location { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage="Capacity must be greater than 0")]
        public int Capacity { get; set; }
        public string? ImageURL { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
       
    }
}
