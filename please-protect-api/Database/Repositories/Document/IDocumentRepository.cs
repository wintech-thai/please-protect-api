using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public interface IDocumentRepository
    {
        public void SetCustomOrgId(string customOrgId);

        public Task<bool> IsDocNameExist(string docName);
        public Task<MDocument?> GetDocumentByName(string docName);
        public Task<MDocument?> DeleteDocumentById(string documentId);
        public Task<MDocument?> GetDocumentById(string documentId);
        public Task<MDocument?> UpdateDocumentById(string documentId, MDocument document);
        public Task<List<MDocument>> GetDocuments(VMDocument param);
        public Task<int> GetDocumentCount(VMDocument param);
        public Task<MDocument> AddDocument(MDocument document);
    }
}
