using System.Runtime.Serialization;

namespace Service.MessageTemplates.Grpc.Models
{
    [DataContract]
    public class GetTemplateBodyResponse
    {
        [DataMember(Order = 1)] public string Body { get; set; }
        
    }
}