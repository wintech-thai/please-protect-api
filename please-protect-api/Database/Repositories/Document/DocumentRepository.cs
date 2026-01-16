using LinqKit;
using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.ViewsModels;
using System.Data.Entity;

namespace Its.PleaseProtect.Api.Database.Repositories
{
    public class DocumentRepository : BaseRepository, IDocumentRepository
    {
        public DocumentRepository(IDataContext ctx)
        {
            context = ctx;
        }

        public async Task<bool> IsDocNameExist(string docName)
        {
            var exists = await context!.Documents!.AsExpandable().AnyAsync(p => p!.DocName!.Equals(docName) && p!.OrgId!.Equals(orgId));
            return exists;
        }

        public async Task<MDocument?> GetDocumentByName(string docName)
        {
            var exists = await context!.Documents!.AsExpandable().Where(p => p!.DocName!.Equals(docName) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return exists;
        }

        public async Task<MDocument?> DeleteDocumentById(string documentId)
        {
            Guid id = Guid.Parse(documentId);
            var existing = await context!.Documents!.AsExpandable().Where(p => p!.DocId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                context.Documents!.Remove(existing);
                await context.SaveChangesAsync();
            }

            return existing;
        }

        public async Task<MDocument?> GetDocumentById(string documentId)
        {
            Guid id = Guid.Parse(documentId);
            var u = await GetSelection().AsExpandable().Where(p => p!.DocId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            return u;
        }

        public async Task<List<MDocument>> GetDocuments(VMDocument param)
        {
            var limit = 0;
            var offset = 0;

            //Param will never be null
            if (param.Offset > 0)
            {
                //Convert to zero base
                offset = param.Offset-1;
            }

            if (param.Limit > 0)
            {
                limit = param.Limit;
            }

            var predicate = DocumentPredicate(param!);
            var result = await GetSelection().AsExpandable()
            .Where(predicate)
            .OrderByDescending(e => e.CreatedDate)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            foreach (var r in result)
            {
                r.MetaData = "";
            }

            return result;
        }

        public async Task<int> GetDocumentCount(VMDocument param)
        {
            var predicate = DocumentPredicate(param!);
            var result = await context!.Documents!.Where(predicate).AsExpandable().CountAsync();

            return result;
        }

        public IQueryable<MDocument> GetSelection()
        {
            var query =
                from doc in context!.Documents
                select new { doc };  // <-- ให้ query ตรงนี้ยังเป็น IQueryable
            return query.Select(x => new MDocument
            {
                DocId = x.doc.DocId,
                OrgId = x.doc.OrgId,
                DocName = x.doc.DocName,
                Description = x.doc.Description,
                DocType = x.doc.DocType,
                Tags = x.doc.Tags,
                MetaData = x.doc.MetaData,
                Bucket = x.doc.Bucket,
                Path = x.doc.Path,
                CreatedDate = x.doc.CreatedDate,
            });
        }

        private ExpressionStarter<MDocument> DocumentPredicate(VMDocument param)
        {
            var pd = PredicateBuilder.New<MDocument>();

            pd = pd.And(p => p.OrgId!.Equals(orgId));

            if ((param.FullTextSearch != "") && (param.FullTextSearch != null))
            {
                var fullTextPd = PredicateBuilder.New<MDocument>();
                fullTextPd = fullTextPd.Or(p => p.DocName!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Description!.Contains(param.FullTextSearch));
                fullTextPd = fullTextPd.Or(p => p.Tags!.Contains(param.FullTextSearch));

                pd = pd.And(fullTextPd);
            }

            return pd;
        }

        public async Task<MDocument> AddDocument(MDocument document)
        {
            document.OrgId = orgId;
            document.CreatedDate = DateTime.UtcNow;

            await context!.Documents!.AddAsync(document);
            await context.SaveChangesAsync();

            return document;
        }

        public async Task<MDocument?> UpdateDocumentById(string documentId, MDocument document)
        {
            Guid id = Guid.Parse(documentId);
            var existing = await context!.Documents!.AsExpandable().Where(p => p!.DocId!.Equals(id) && p!.OrgId!.Equals(orgId)).FirstOrDefaultAsync();
            if (existing != null)
            {
                existing.DocName = document.DocName;
                existing.Tags = document.Tags;
                existing.Description = document.Description;
            }

            await context.SaveChangesAsync();
            return existing;
        }
    }
}