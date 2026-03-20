using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Utils;

namespace Its.PleaseProtect.Api.Services
{
    public class ApplicationService : BaseService, IApplicationService
    {
        private readonly string dataPlaneUrl = "http://gitea-http.gitea.svc.cluster.local:3000/local/data-plane.git";
        private readonly string dataPlaneBranch = "main";
        private readonly string gitSyncBaseDir = "/tmp/git";

        public ApplicationService() : base()
        {
        }

        public async Task<List<MApplication>> GetApplications(string orgId)
        {
            // ✅ unique working dir
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            try
            {
                await git.CloneAsync(dataPlaneUrl);
                await git.PullAsync(dataPlaneBranch);

                var appPath = Path.Combine(
                    workingDir,
                    "99-deployments",
                    "applications"
                );

                if (!Directory.Exists(appPath))
                    return new List<MApplication>();

                var files = Directory.GetFiles(appPath, "*.yaml")
                    .Concat(Directory.GetFiles(appPath, "*.yml"));

                return files.Select(f => new MApplication
                {
                    AppName = Path.GetFileNameWithoutExtension(f)
                }).ToList();
            }
            finally
            {
                git.Cleanup();
            }
        }
    }
}
