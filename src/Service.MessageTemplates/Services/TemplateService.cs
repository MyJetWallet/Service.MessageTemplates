using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.MessageTemplates.Domain.Models;
using Service.MessageTemplates.Domain.Models.NoSql;
using Service.MessageTemplates.Grpc;
using Service.MessageTemplates.Grpc.Models;

namespace Service.MessageTemplates.Services
{
    public class TemplateService: ITemplateService
    {
        private readonly ILogger<TemplateService> _logger;
        private readonly IMyNoSqlServerDataWriter<TemplateNoSqlEntity> _templateWriter;

        private const string _defaultBrand = "default";
        private const string _defaultLang = "en";
        
        public TemplateService(ILogger<TemplateService> logger, IMyNoSqlServerDataWriter<TemplateNoSqlEntity> templateWriter)
        {
            _logger = logger;
            _templateWriter = templateWriter;
        }

        public async Task DeleteBody(TemplateEditRequest request)
        {
            try
            {
                var partKey = TemplateNoSqlEntity.GeneratePartitionKey();
                var rowKey = TemplateNoSqlEntity.GenerateRowKey(request.TemplateId);
                var templateEntity = await _templateWriter.GetAsync(partKey, rowKey);
                if (templateEntity == null)
                    return;
            
                var template = templateEntity.ToTemplate();
                if (template.DefaultBrand == request.Brand && template.DefaultLang == request.Lang)
                {
                    _logger.LogWarning("Unable to delete default template for type {TemplateId}", template.TemplateId);
                    throw new InvalidOperationException($"Unable to delete default template for type {template.TemplateId}");
                }

                if (template.Bodies.ContainsKey((request.Brand, request.Lang)))
                {
                    template.Bodies.Remove((request.Brand, request.Lang));
                    await _templateWriter.InsertOrReplaceAsync(TemplateNoSqlEntity.Create(template));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "When trying to delete body for template {TemplateId} with brand {Brand} and lang {Lang}", request.TemplateId, request.Brand, request.Lang);
                throw;
            }
        }

        public async Task<GetTemplateBodyResponse> GetTemplateBody(GetTemplateBodyRequest request)
        {
            var templateId = request.TemplateId.ToLower();
            var brand = request.Brand.ToLower();
            var lang = request.Lang.ToLower();
            
            var partKey = TemplateNoSqlEntity.GeneratePartitionKey();
            var rowKey = TemplateNoSqlEntity.GenerateRowKey(templateId);
            var templateEntity = await _templateWriter.GetAsync(partKey, rowKey);
            if (templateEntity == null)
                return new GetTemplateBodyResponse()
                {
                    Body = String.Empty
                };
            
            var template = templateEntity.ToTemplate();
            string body; 
            if (!template.Bodies.TryGetValue((brand, lang), out body))
            {
                _logger.LogWarning("No template found for {TemplateId}, {Brand} and {Lang}", templateId, brand, lang);
                if(!template.Bodies.TryGetValue((brand, template.DefaultLang), out body))
                {
                    _logger.LogWarning("No template found for {TemplateId}, {Brand} and {DefaultLang}", templateId,
                        brand, template.DefaultLang);
                    if (!template.Bodies.TryGetValue((template.DefaultBrand, lang), out body))
                    {
                        _logger.LogWarning("No template found for {TemplateId}, {DefaultBrand} and {Lang}", templateId,
                            template.DefaultBrand, lang);
                        if (!template.Bodies.TryGetValue((template.DefaultBrand, template.DefaultLang), out body))
                        {
                            _logger.LogError("No default template for {TemplateId}", templateId);
                            throw new Exception();
                        }
                    }
                }
            }

            return new GetTemplateBodyResponse()
            {
                Body = body
            };
        }

        public async Task CreateDefaultTemplates()
        {
            var templateEntities = (await _templateWriter.GetAsync())?.ToList();
            foreach (var id in templateEntities.Select(entity => entity.TemplateId))
            {
                var templateEntity = templateEntities?.FirstOrDefault(e => e.TemplateId == id);
                if (templateEntity == null)
                {
                    var template = new Template()
                    {
                        TemplateId = id,
                        DefaultBrand = _defaultBrand,
                        DefaultLang = _defaultLang,
                        Params = await GetTemplateBodyParams(id),
                        Bodies = await GetDefaultTemplateBodies(id, _defaultBrand, _defaultLang)
                    };

                    var newTemplateEntity = TemplateNoSqlEntity.Create(template);
                    await _templateWriter.InsertAsync(newTemplateEntity);

                    _logger.LogInformation("Template (ID: {templateId}) doesn't exist, creating the new one.",
                        id);
                }
            }
        }
        
        public async Task<TemplateListResponse> GetAllTemplates()
        {
            try
            {
                var templateEntities = (await _templateWriter.GetAsync())?.ToList();
                var templates = new List<Template>();
                foreach (var id in templateEntities.Select(entity => entity.TemplateId))
                {
                    var templateEntity = templateEntities?.FirstOrDefault(e => e.TemplateId == id);
                    templates.Add(templateEntity.ToTemplate());
                }
                return new TemplateListResponse
                {
                    Templates = templates
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "When getting all templates");
                throw;
            }
        }

        public async Task<Template> CreateNewTemplate(Template template)
        {
            if (string.IsNullOrWhiteSpace(template.DefaultBrand))
                template.DefaultBrand = _defaultBrand;
            if (string.IsNullOrWhiteSpace(template.DefaultLang))
                template.DefaultLang = _defaultLang;
            
            template.DefaultBrand = template.DefaultBrand.ToLower().Trim();
            template.DefaultLang = template.DefaultLang.ToLower().Trim();
            
            template.Bodies ??= new();
            template.TemplateId = template.TemplateId.ToLower().Trim();

            if (!template.Bodies.TryGetValue((template.DefaultBrand, template.DefaultLang), out var body))
            {
                var value = await GenerateTemplateBodyPlaceholderWithParams(template.TemplateId, template.Params);
                template.Bodies[(template.DefaultBrand, template.DefaultLang)] = value;
            }

            await _templateWriter.InsertOrReplaceAsync(TemplateNoSqlEntity.Create(template));

            return template;
        }

        public async Task EditTemplate(TemplateEditRequest request)
        {
            var templateId = request.TemplateId.ToLower().Trim();
            var brand = request.Brand.ToLower().Trim();
            var lang = request.Lang.ToLower().Trim();
            
            var partKey = TemplateNoSqlEntity.GeneratePartitionKey();
            var rowKey = TemplateNoSqlEntity.GenerateRowKey(templateId);

            var templateEntity = await _templateWriter.GetAsync(partKey, rowKey);
            if (templateEntity == null)
                return;
            
            var template = templateEntity.ToTemplate();
            template.Bodies[(brand, lang)] = request.TemplateBody;

            await _templateWriter.InsertOrReplaceAsync(TemplateNoSqlEntity.Create(template));
        }
        
        public async Task EditDefaultValues(TemplateEditRequest request)
        {
            var templateId = request.TemplateId.ToLower().ToLower();
            var brand = request.Brand.ToLower().ToLower();
            var lang = request.Lang.ToLower().ToLower();
            
            var partKey = TemplateNoSqlEntity.GeneratePartitionKey();
            var rowKey = TemplateNoSqlEntity.GenerateRowKey(templateId);

            var templateEntity = await _templateWriter.GetAsync(partKey, rowKey);
            if (templateEntity == null)
                return;
            
            var template = templateEntity.ToTemplate();
            template.DefaultBrand = brand;
            template.DefaultLang = lang;

            if (string.IsNullOrWhiteSpace(request.TemplateBody))
            {
                template.Bodies[(brand, lang)] =
                    await GenerateTemplateBodyPlaceholder(templateId);
            }
            else
            {
                template.Bodies[(brand, lang)] = request.TemplateBody;
            }
            await _templateWriter.InsertOrReplaceAsync(TemplateNoSqlEntity.Create(template));
        }

        private async Task<Dictionary<(string, string), string>> GetDefaultTemplateBodies(string templateId, string brand,
            string lang)
        {
            try
            {
                return new Dictionary<(string, string), string>
                {
                    {
                        (brand, lang), await GenerateTemplateBodyPlaceholder(templateId)
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "No default template body found for type {TemplateId}", templateId);
                throw;
            }
        }

        private async Task<List<string>> GetTemplateBodyParams(string templateId)
        {
            try
            {
                return (await _templateWriter.GetAsync(TemplateNoSqlEntity.GeneratePartitionKey(),TemplateNoSqlEntity.GenerateRowKey(templateId))).Params;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "No default template params found for type {Type}", templateId);
                throw;
            }
        }

        private async Task<string> GenerateTemplateBodyPlaceholder(string templateId)
        {
            var body = $"Placeholder for {templateId}: ";
            var parameters = await GetTemplateBodyParams(templateId);
            return (parameters!= null && parameters.Any()) 
                ? parameters.Aggregate(body, (current, param) => current + $"{param} ") 
                : body;
        }
        
        private async Task<string> GenerateTemplateBodyPlaceholderWithParams(string templateId, List<string> parameters)
        {
            var body = $"Placeholder for {templateId}: ";
            return (parameters!= null && parameters.Any()) 
                ? parameters.Aggregate(body, (current, param) => current + $"{param} ") 
                : body;
        }
    }
}
