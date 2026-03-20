using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Utils;
using YamlDotNet.Serialization;

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

        private object? GetValue(Dictionary<object, object> dict, params string[] keys)
        {
            object? current = dict;
            foreach (var key in keys)
            {
                if (current is Dictionary<object, object> d && d.TryGetValue(key, out var next))
                {
                    current = next;
                }
                else
                {
                    return null;
                }
            }
            return current;
        }

        public async Task<List<MApplication>> GetApplications(string orgId)
        {
            var workingDir = Path.Combine(gitSyncBaseDir, $"data-plane-{Guid.NewGuid()}");
            var git = new GitUtil(workingDir);

            var result = new List<MApplication>();
            var deserializer = new DeserializerBuilder().Build();

            try
            {
                await git.CloneAsync(dataPlaneUrl);

                // ensure branch
                //await git.RunGitPublicAsync($"checkout {dataPlaneBranch}");
                await git.PullAsync(dataPlaneBranch);

                var appPath = Path.Combine(workingDir, "99-deployments", "applications");

                if (!Directory.Exists(appPath))
                    return result;

                var files = Directory.GetFiles(appPath, "*.yaml")
                    .Concat(Directory.GetFiles(appPath, "*.yml"));

                foreach (var file in files)
                {
                    try
                    {
                        var yaml = await File.ReadAllTextAsync(file);

                        var data = deserializer.Deserialize<Dictionary<object, object>>(yaml);

                        var repoUrl = GetValue(data, "spec", "template", "spec", "source", "repoURL")?.ToString();
                        var path = GetValue(data, "spec", "template", "spec", "source", "path")?.ToString();
                        var ns = GetValue(data, "spec", "template", "spec", "destination", "namespace")?.ToString();

                        result.Add(new MApplication
                        {
                            OrgId = orgId,
                            AppName = Path.GetFileNameWithoutExtension(file),
                            RepoUrl = repoUrl,
                            Path = path,
                            Namespace = ns
                        });
                    }
                    catch
                    {
                        // ข้ามไฟล์ที่ parse ไม่ได้
                        continue;
                    }
                }

                return result;
            }
            finally
            {
                try
                {
                    git.Cleanup();
                }
                catch
                {
                    // กัน cleanup fail แล้วกลบ error หลัก
                }
            }
        }

        public async Task<List<MApplication>> GetApplications2(string orgId)
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
                    OrgId = orgId,
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
