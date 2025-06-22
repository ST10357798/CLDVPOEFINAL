using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CLDVPOE1.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Navigation property (optional, helps with relationships)
        public ICollection<Event> Events { get; set; }
    }
}
