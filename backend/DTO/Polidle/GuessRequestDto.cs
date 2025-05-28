using System.ComponentModel.DataAnnotations;
using backend.Enums;

namespace backend.DTO
{
    public class GuessRequestDto
    {
        [Required(ErrorMessage = "Gættet politiker ID mangler.")]
        public int GuessedPoliticianId { get; set; }
        [Required(ErrorMessage = "Spiltype mangler.")]
        [EnumDataType(typeof(GamemodeTypes), ErrorMessage = "Ugyldig spiltype angivet.")]
        public GamemodeTypes GameMode { get; set; }
    }
}
