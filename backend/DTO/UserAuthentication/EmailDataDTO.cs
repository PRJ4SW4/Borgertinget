namespace backend.DTO.UserAuthentication;
public class EmailDataDto
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlMessage { get; set; } = string.Empty;
};