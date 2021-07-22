using System.Runtime.Serialization;

namespace Service.MessageTemplates.Grpc.Models
{
    [DataContract]
    public class TemplateEditRequest
    {
        [DataMember(Order = 1)] public string TemplateId { get; set; }
        
        [DataMember(Order = 2)] public string Brand { get; set; }

        [DataMember(Order = 3)] public string Lang { get; set; }
        
        [DataMember(Order = 4)] public string TemplateBody { get; set; }
    }
}