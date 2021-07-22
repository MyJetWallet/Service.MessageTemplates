using Autofac;
using Service.MessageTemplates.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.MessageTemplates.Client
{
    public static class AutofacHelper
    {
        public static void RegisterMessageTemplatesClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new MessageTemplatesClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<ITemplateService>().SingleInstance();
        }
    }
}
