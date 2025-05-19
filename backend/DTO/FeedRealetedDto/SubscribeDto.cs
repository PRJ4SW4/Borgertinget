using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class SubscribeDto
    {
        [Required]
        public int PoliticianId { get; set; }
    }
}
