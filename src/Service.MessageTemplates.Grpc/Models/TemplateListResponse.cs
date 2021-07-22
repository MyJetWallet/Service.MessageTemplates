using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.MessageTemplates.Domain.Models;

namespace Service.MessageTemplates.Grpc.Models
{
    [DataContract]
    public class TemplateListResponse
    {
        [DataMember(Order = 1)]
        public List<Template> Templates { get; set; }
    }
}