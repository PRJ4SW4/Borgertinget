using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class SubscribeDto
    {
        [Required]
        public int? PoliticianId { get; set; }
    }
}
