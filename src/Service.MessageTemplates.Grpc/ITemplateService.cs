using System.ServiceModel;
using System.Threading.Tasks;
using Service.MessageTemplates.Domain.Models;
using Service.MessageTemplates.Grpc.Models;

namespace Service.MessageTemplates.Grpc
{
    [ServiceContract]
    public interface ITemplateService
    {
        [OperationContract]Task<GetTemplateBodyResponse> GetTemplateBody(GetTemplateBodyRequest request);
        [OperationContract]Task<TemplateListResponse> GetAllTemplates();
        [OperationContract]Task CreateNewTemplate(Template template);
        [OperationContract]Task EditTemplate(TemplateEditRequest request);
        [OperationContract]Task DeleteBody(TemplateEditRequest request);
        [OperationContract]Task EditDefaultValues(TemplateEditRequest request);
        Task CreateDefaultTemplates();
    }
}