namespace TemplateWorkload.Models
{
    public class CreateExternalDataShareRequest
    {
        public List<string> Paths { get; set; } = new();
        public ExternalDataShareRecipient Recipient { get; set; } = new();
    }

    public class ExternalDataShareRecipient
    {
        public string PrincipalId { get; set; } = string.Empty;
        public string PrincipalType { get; set; } = string.Empty;
    }

    public class CreateExternalDataShareResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
