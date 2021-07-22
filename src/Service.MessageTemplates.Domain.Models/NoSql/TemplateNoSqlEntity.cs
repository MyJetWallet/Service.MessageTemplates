using System.Collections.Generic;
using System.Linq;
using MyNoSqlServer.Abstractions;

namespace Service.MessageTemplates.Domain.Models.NoSql
{
    public class TemplateNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-message-templates";

        public static string GeneratePartitionKey() => "Templates";

        public static string GenerateRowKey(string templateId) => templateId;
        
        public string TemplateId { get; set; }
        
        public string DefaultBrand { get; set; }
        
        public string DefaultLang { get; set; }
        
        public Dictionary<string,string> BodiesSerializable { get; set; }
        
        public List<string> Params { get; set; }
        
        public static TemplateNoSqlEntity Create(Template template) =>
            new()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(template.TemplateId),
                TemplateId = template.TemplateId,
                DefaultBrand = template.DefaultBrand,
                DefaultLang = template.DefaultLang,
                Params = template.Params,
                BodiesSerializable = template.Bodies.ToDictionary((pair => $"{pair.Key.brand};-;{pair.Key.lang}"),pair => pair.Value)
            };


    }
}