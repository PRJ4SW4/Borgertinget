using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using backend.utils; // Add this namespace for the custom validation attribute

namespace backend.DTOs
{
    public class PollDto
    {
        [Required(ErrorMessage = "Spørgsmål må ikke være tomt.")]
        [MaxLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "Der skal være mindst 2 svarmuligheder.")]
        [MaxLength(4, ErrorMessage = "Der kan højst være 4 svarmuligheder.")]
        [DistinctItems(ErrorMessage = "Svarmuligheder må ikke være ens.")]
        public List<string> Options { get; set; } = new List<string>();

        [Required(ErrorMessage = "Politiker ID mangler.")]
        public int PoliticianTwitterId { get; set; } // ID på den politiker, det omhandler

        public DateTime? EndedAt { get; set; } // slut dato for afstemningen
    }
}
