using Autofac;
using MyNoSqlServer.DataReader;
using Service.MessageTemplates.Domain.Models.NoSql;
using Service.MessageTemplates.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.MessageTemplates.Client
{
    public static class AutofacHelper
    {
        public static void RegisterMessageTemplatesClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new MessageTemplatesClientFactory(grpcServiceUrl);
            builder.RegisterInstance(factory.GetTemplateService()).As<ITemplateService>().SingleInstance();
        }
        
        public static void RegisterMessageTemplatesCachedClient(this ContainerBuilder builder, string grpcServiceUrl,IMyNoSqlSubscriber myNoSqlSubscriber)
        {
            var factory = new MessageTemplatesClientFactory(grpcServiceUrl);
            var templateService = factory.GetTemplateService();
            var subs = new MyNoSqlReadRepository<TemplateNoSqlEntity>(myNoSqlSubscriber, TemplateNoSqlEntity.TableName);
            
            builder.RegisterInstance(templateService).As<ITemplateService>().SingleInstance();
            builder
                .RegisterInstance(new TemplateClient(subs, templateService))
                .As<ITemplateClient>()
                .SingleInstance();
        }
    }
}
