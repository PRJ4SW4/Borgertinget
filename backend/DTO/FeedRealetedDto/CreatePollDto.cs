// backend.DTOs/CreatePollDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class CreatePollDto
    {
        [Required(ErrorMessage = "Spørgsmål må ikke være tomt.")]
        [MaxLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "Der skal være mindst 2 svarmuligheder.")]
        [MaxLength(4, ErrorMessage = "Der kan højst være 4 svarmuligheder.")]
        public List<string> Options { get; set; } = new List<string>();

        [Required(ErrorMessage = "Politiker ID mangler.")]
        public int PoliticianTwitterId { get; set; } // ID på den politiker, det omhandler

        public DateTime? EndedAt { get; set; } // slut dato for afstemningen
    }
}
