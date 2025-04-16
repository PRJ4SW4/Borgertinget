namespace backend.DTO;
using System.Text.Json.Serialization;
//Backend purposes
public class UpdateAktor{
        public DateTime? slutdato {get; set;} //Kun for grupper, ministertitler- og -områder
        public DateTime? startdato {get; set;}
        public string? biografi {get; set;}
}

public class CreateAktor{
    [JsonPropertyName("id")]
    public int Id{get; set;}
    public string? navn{get; set;}
    
    public string? fornavn{get; set;}
    public string? efternavn {get; set;}
    public string? biografi{get; set;}
    public DateTime? startdato{get; set;}
    public DateTime? slutdato{get; set;}
}


