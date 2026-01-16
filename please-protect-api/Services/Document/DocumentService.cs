using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Database.Repositories;
using Its.PleaseProtect.Api.ViewsModels;
using Its.PleaseProtect.Api.ModelsViews;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Services
{
    public class DocumentService : BaseService, IDocumentService
    {
        private readonly IDocumentRepository? repository = null;
        private readonly IJobService _jobService;
        private readonly IObjectStorageService _storageService;

        public DocumentService(IDocumentRepository repo, 
            IObjectStorageService storageService,
            IJobService jobService) : base()
        {
            repository = repo;
            _jobService = jobService;
            _storageService = storageService;
        }

        public async Task<MVDocument> GetDocumentById(string orgId, string documentId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(documentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Document ID [{documentId}] format is invalid";

                return r;
            }

            var result = await repository!.GetDocumentById(documentId);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Document ID [{documentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Document = result;
            r.Document.MetaData = "";

            return r;
        }

        public async Task<MVDocument> AddDocument(string orgId, MDocument document)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVDocument();
            r.Status = "OK";
            r.Description = "Success";

            if (string.IsNullOrEmpty(document.DocName))
            {
                r.Status = "NAME_MISSING";
                r.Description = $"Document name is missing!!!";

                return r;
            }

            var isExist = await repository!.IsDocNameExist(document.DocName);
            if (isExist)
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Document name [{document.DocName}] already exist!!!";

                return r;
            }

            //Check เพิ่มเติมว่ามีไฟล์วางอยู่จริงแล้วที่ Bucket/Path
            var isObjExist = await _storageService!.IsObjectExist(document.Bucket!, document.Path!);
            if (!isObjExist)
            {
                r.Status = "OBJECT_NOT_FOUND";
                r.Description = $"Storage oject [{document.Path}] is missing from bucket [{document.Bucket}] !!!";

                return r;
            }

            var result = await repository!.AddDocument(document);
            r.Document = result;

            r.Document.MetaData = "";

            var job = new MJob()
            {
                DocumentId = result.DocId.ToString(),
                Name = $"{Guid.NewGuid()}",
                Description = "Document.AddDocument()",
                Type = "DocumentExtract",
                Status = "Pending",
                Tags = "",

                Parameters =
                [
                    new MKeyValue { Name = "ORG_ID", Value = orgId },

                    new MKeyValue { Name = "DOCUMENT_ID", Value = result.DocId.ToString() },
                    new MKeyValue { Name = "DOCUMENT_NANE", Value = result.DocName },
                    new MKeyValue { Name = "DOCUMENT_TYPE", Value = result.DocType },
                    new MKeyValue { Name = "DOCUMENT_BUCKET", Value = result.Bucket },
                    new MKeyValue { Name = "DOCUMENT_PATH", Value = result.Path },
                ]
            };

            var _ = _jobService.AddJob(orgId, job);

            return r;
        }

        public async Task<MVDocument> DeleteDocumentById(string orgId, string documentId)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(documentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Document ID [{documentId}] format is invalid";

                return r;
            }

            var m = await repository!.DeleteDocumentById(documentId);
            if (m == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Document ID [{documentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Document = m;
            return r;
        }

        public async Task<List<MDocument>> GetDocuments(string orgId, VMDocument param)
        {
            if ((param.Limit >= 100) || (param.Limit <= 0))
            {
                param.Limit = 100;
            }

            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetDocuments(param);

            return result;
        }

        public async Task<int> GetDocumentCount(string orgId, VMDocument param)
        {
            repository!.SetCustomOrgId(orgId);
            var result = await repository!.GetDocumentCount(param);

            return result;
        }

        public async Task<MVDocument> UpdateDocumentById(string orgId, string documentId, MDocument document)
        {
            repository!.SetCustomOrgId(orgId);

            var r = new MVDocument()
            {
                Status = "OK",
                Description = "Success"
            };

            if (!ServiceUtils.IsGuidValid(documentId))
            {
                r.Status = "UUID_INVALID";
                r.Description = $"Document ID [{documentId}] format is invalid";

                return r;
            }

            var docName = document.DocName;
            var cr = await repository!.GetDocumentByName(docName!);
            if ((cr != null) && (cr.DocId.ToString() != documentId))
            {
                r.Status = "NAME_DUPLICATE";
                r.Description = $"Document name [{docName}] already exist!!!";

                return r;
            }

            var result = await repository!.UpdateDocumentById(documentId, document);
            if (result == null)
            {
                r.Status = "NOTFOUND";
                r.Description = $"Document ID [{documentId}] not found for the organization [{orgId}]";

                return r;
            }

            r.Document = result;
            //ไม่ให้ส่งออกไป แต่เช็คเพิ่มเติมนะว่าไม่ได้ update กลับไปที่ DB
            r.Document.MetaData = "";

            return r;
        }

        public async Task<MVPresignedUrl> GetDocumentPostUploadUrl(string orgId, VMPresignedRequest param)
        {
            var r = new MVPresignedUrl()
            {
                Status = "OK",
                Description = "Success"
            };

            var docType = param.DocumentType;
            var fileName = param.FileName;

            if (string.IsNullOrEmpty(docType))
            {
                r.Status = "DOC_TYPE_EMPTY";
                r.Description = $"Document type is missing";

                return r;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                r.Status = "FILE_NAME_EMPTY";
                r.Description = $"File name is missing";

                return r;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            var normalizedPath = $"{orgId}/documents/{docType}/{timestamp}_{fileName}";
            var presignedResult = await _storageService.GetPresignedUrlPost("", normalizedPath, 600);

            r.PresignedResult = presignedResult;

            return r;
        }
    }
}
