using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.MessageTemplates.Grpc;

namespace Service.MessageTemplates.Client
{
    [UsedImplicitly]
    public class MessageTemplatesClientFactory: MyGrpcClientFactory
    {
        public MessageTemplatesClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public ITemplateService GetHelloService() => CreateGrpcService<ITemplateService>();
    }
}
