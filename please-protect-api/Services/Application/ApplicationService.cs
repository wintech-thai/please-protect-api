using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Utils;
using YamlDotNet.Serialization;

namespace Its.PleaseProtect.Api.Services
{
    public class ApplicationService : BaseService, IApplicationService
    {
        private readonly string dataPlaneUrl = "http://gitea-http.gitea.svc.cluster.local:3000/local/data-plane.git";
        private readonly string dataPlaneBranch = "main";

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

        public async Task<List<MApplication>> GetApplications(string orgId, GitUtil git, bool withCleanup)
        {
            var workingDir = git.GetWorkingDir();

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
                        var branch = GetValue(data, "spec", "template", "spec", "source", "targetRevision")?.ToString();
                        var path = GetValue(data, "spec", "template", "spec", "source", "path")?.ToString();
                        var ns = GetValue(data, "spec", "template", "spec", "destination", "namespace")?.ToString();

                        result.Add(new MApplication
                        {
                            OrgId = orgId,
                            AppName = Path.GetFileNameWithoutExtension(file),
                            RepoUrl = repoUrl,
                            Path = path,
                            Namespace = ns,
                            Branch = branch,
                            Directory = workingDir,
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
                    if (withCleanup)
                    {
                        git.Cleanup();
                    }
                }
                catch
                {
                    // กัน cleanup fail แล้วกลบ error หลัก
                }
            }
        }

        private async Task<string> GetFileContent(string orgId, GitUtil git, string appName, string fileName)
        {
            var apps = await GetApplications(orgId, git, false);

            foreach (var app in apps)
            {
                if (app.AppName == appName)
                {
                    try
                    {
                        // รวม path: base directory + app path + values.yaml
                        var fullPath = Path.Combine(app.Directory!, app.Path!, fileName);
//Console.WriteLine($"DEBUG1 ==> [{fullPath}]");
                        // เช็คว่าไฟล์มีอยู่ไหม
                        if (!File.Exists(fullPath))
                        {
                            return "ERR:VALUE_FILE_NOTFOUND";
                        }

                        // อ่านไฟล์แล้ว return
                        return await File.ReadAllTextAsync(fullPath);
                    }
                    catch
                    {
                        // กันกรณี path เพี้ยนหรือ permission
                        return "ERR:VALUE_FILE_NOTFOUND";
                    }
                }
            }

            // หา appName ไม่เจอ
            git.Cleanup();
            return "ERR:APP_VALUE_NOTFOUND";
        }

        public async Task<string> GetCurrentAppDefaultConfig(string orgId, GitUtil git, string appName)
        {
            var result = await GetFileContent(orgId, git, appName, "values.yaml");
            return result;
        }

        public async Task<string> GetCurrentAppCustomConfig(string orgId, GitUtil git, string appName)
        {
            var result = await GetFileContent(orgId, git, appName, "values-local.yaml");
            return result;
        }
    }
}
