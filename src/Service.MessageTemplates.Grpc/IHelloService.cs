using System.ServiceModel;
using System.Threading.Tasks;
using Service.MessageTemplates.Grpc.Models;

namespace Service.MessageTemplates.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}