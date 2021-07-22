using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.MessageTemplates.Domain.Models
{
    [DataContract]
    public class Template
    {
        [DataMember(Order = 1)]
        public string TemplateId { get; set; }
        
        [DataMember(Order = 2)]
        public string DefaultBrand { get; set; }
        
        [DataMember(Order = 3)]
        public string DefaultLang { get; set; }
        
        [DataMember(Order = 4)]
        public Dictionary<(string brand,string lang), string> Bodies { get; set; }
        
        [DataMember(Order = 5)]
        public List<string> Params { get; set; }

    }
}