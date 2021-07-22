using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.MessageTemplates.Domain.Models.NoSql;
using Service.MessageTemplates.Grpc;
using Service.MessageTemplates.Services;

namespace Service.MessageTemplates.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<TemplateNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), TemplateNoSqlEntity.TableName);
            builder.RegisterType<TemplateService>().As<ITemplateService>();
        }
    }
}