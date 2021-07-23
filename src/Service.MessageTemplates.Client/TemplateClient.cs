using System.Collections.Generic;
using System.Threading.Tasks;
using MyNoSqlServer.DataReader;
using Service.MessageTemplates.Domain.Models;
using Service.MessageTemplates.Domain.Models.NoSql;
using Service.MessageTemplates.Grpc;
using Service.MessageTemplates.Grpc.Models;

namespace Service.MessageTemplates.Client
{
    public class TemplateClient : ITemplateClient
    {
        private readonly MyNoSqlReadRepository<TemplateNoSqlEntity> _templateReader;
        private readonly ITemplateService _templateGrpcService;
        private readonly Dictionary<string, string> _cachedTemplates = new ();

        public TemplateClient(MyNoSqlReadRepository<TemplateNoSqlEntity> templateReader, ITemplateService templateGrpcService)
        {
            _templateReader = templateReader;
            _templateGrpcService = templateGrpcService;
            _templateReader.SubscribeToUpdateEvents(HandleUpdate, HandleUpdate);
        }

        private void HandleUpdate(IReadOnlyList<TemplateNoSqlEntity> list) => _cachedTemplates.Clear();

        public async Task<string> GetTemplateBody(string templateId, string brand, string lang)
        {
            if (_cachedTemplates.TryGetValue($"{templateId}:{brand}:{lang}", out var body))
                return body;

            var templateEntity = _templateReader.Get(TemplateNoSqlEntity.GeneratePartitionKey(),
                TemplateNoSqlEntity.GenerateRowKey(templateId));

            if (templateEntity != null)
            {
                var template = templateEntity.ToTemplate();
                if (template.Bodies.TryGetValue((brand, lang), out body))
                {
                    _cachedTemplates.Add($"{templateId}:{brand}:{lang}", body);
                    return body;
                }
            }

            var response = await _templateGrpcService.GetTemplateBody(new GetTemplateBodyRequest()
            {
                TemplateId = templateId,
                Brand = brand,
                Lang = lang
            });
            _cachedTemplates.Add($"{templateId}:{brand}:{lang}", response.Body);
            return response.Body;
        }
    }
}