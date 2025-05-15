// backend.DTOs/VoteDto.cs
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class VoteDto
    {
        [Required(ErrorMessage = "OptionId mangler.")]
        public int OptionId { get; set; } // ID på den PollOption der stemmes på
    }
}
