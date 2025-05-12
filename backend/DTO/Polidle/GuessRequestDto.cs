using backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace backend.DTO
{
    /// DTO til at sende et gæt fra frontend til backend.
    public class GuessRequestDto
    {
        /// ID på den politiker, brugeren gætter på.
        [Required(ErrorMessage = "Gættet politiker ID mangler.")]
        public int GuessedPoliticianId { get; set; }

        /// Hvilken spiltype gættet tilhører.
        [Required(ErrorMessage = "Spiltype mangler.")]
        // Overvej EnumDataType hvis du vil have model validering på enum værdier
        [EnumDataType(typeof(GamemodeTypes), ErrorMessage = "Ugyldig spiltype angivet.")]
        public GamemodeTypes GameMode { get; set; }

        //TODO: Evt. Tilføj UserId senere, hvis gæt skal knyttes til en bruger
        //* public string? UserId { get; set; }
    }
}