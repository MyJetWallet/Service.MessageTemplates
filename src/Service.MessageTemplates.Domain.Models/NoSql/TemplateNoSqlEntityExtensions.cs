using System.Linq;

namespace Service.MessageTemplates.Domain.Models.NoSql
{
    public static class TemplateNoSqlEntityExtensions
    {
        public static Template ToTemplate(this TemplateNoSqlEntity entity)
        {
            return new Template()
            {
                TemplateId = entity.TemplateId,
                DefaultBrand = entity.DefaultBrand,
                DefaultLang = entity.DefaultLang,
                Params = entity.Params,
                Bodies = entity.BodiesSerializable.ToDictionary(pair => ParseTuple(pair.Key), pair => pair.Value)
            };
            
            //locals
            static (string, string) ParseTuple(string key) => (key.Split(";-;")[0], key.Split(";-;")[1]);
        }
    }
}