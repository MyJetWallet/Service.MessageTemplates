using System.Runtime.Serialization;
using Service.MessageTemplates.Domain.Models;

namespace Service.MessageTemplates.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}