using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Tmf.Saarthi.Core.RequestModels.Document;

public class DocumentRequest
{
    [JsonPropertyName("fleetId")]
    public long FleetId { get; set; }

    [JsonPropertyName("docTypeId")]
    public int DocTypeId { get; set; }

    [JsonPropertyName("stageId")]
    public int StageId { get; set; }

    [JsonPropertyName("extension")]
    public string Extension { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public Boolean IsActive { get; set; }

    [JsonPropertyName("createdBy")]
    public long CreatedBy { get; set; }   

    [JsonPropertyName("documentName")]
    public string DocumentName { get; set; } = string.Empty;

    [JsonPropertyName("DocumentUpload")]
    public IFormFile DocumentUpload { get; set; }
}
