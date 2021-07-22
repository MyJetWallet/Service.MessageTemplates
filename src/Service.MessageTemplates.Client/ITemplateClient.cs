using System.Threading.Tasks;
using Service.MessageTemplates.Domain.Models;

namespace Service.MessageTemplates.Client
{
    public interface ITemplateClient
    {
        Task<string> GetTemplateBody(string templateId, string brand, string lang);
    }
}