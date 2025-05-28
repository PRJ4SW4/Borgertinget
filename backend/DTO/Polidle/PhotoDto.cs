namespace backend.DTO
{
    /// DTO til at levere URL'en for dagens billede i Foto-gamemode.
    public class PhotoDto
    {
        /// URL til portrætbilledet for dagens politiker.
        /// <example>/path/to/image.jpg</example>
        public string? PhotoUrl { get; set; } // Brug URL'en fra Aktor.PictureMiRes
    }
}
