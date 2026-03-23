using Its.PleaseProtect.Api.Models;
using Its.PleaseProtect.Api.Utils;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Its.PleaseProtect.Api.Services
{
    public class ApplicationService : BaseService, IApplicationService
    {
        private readonly string dataPlaneUrl = "http://gitea-http.gitea.svc.cluster.local:3000/local/data-plane.git";
        private readonly string dataPlaneBranch = "main";
         private readonly string dataPlaneDraftBranch = "draft";

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

        private async Task<MApplication> GetFileContent(string orgId, 
            GitUtil git, 
            string appName, 
            string fileName, 
            List<MApplication> apps, 
            bool withCleanup)
        {
            //var apps = await GetApplications(orgId, git, false);

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
                            app.Content = "ERR:VALUE_FILE_NOTFOUND";
                            return app;
                        }

                        // อ่านไฟล์แล้ว return
                        app.Content = await File.ReadAllTextAsync(fullPath);
                        return app;
                    }
                    catch
                    {
                        // กันกรณี path เพี้ยนหรือ permission
                        app.Content = "ERR:VALUE_FILE_NOTFOUND";
                        return app;
                    }
                }
            }

            // หา appName ไม่เจอ
            if (withCleanup)
            {
                git.Cleanup();
            }

            var tmp = new MApplication()
            {
                Content = "ERR:APP_VALUE_NOTFOUND",
            };

            return tmp; 
        }

        public async Task<string> GetCurrentAppDefaultConfig(string orgId, GitUtil git, string appName)
        {
            var apps = await GetApplications(orgId, git, false);
            var app = await GetFileContent(orgId, git, appName, "values.yaml", apps, true);

            return app.Content!;
        }

        public async Task<string> GetCurrentAppCustomConfig(string orgId, GitUtil git, string appName)
        {
            var apps = await GetApplications(orgId, git, false);
            var app = await GetFileContent(orgId, git, appName, "values-local.yaml", apps, true);

            return app.Content!;
        }

        private async Task<List<MApplication>> GetApplicationsDraft(string orgId, GitUtil git, bool withCleanup)
        {
            var workingDir = git.GetWorkingDir();

            var result = new List<MApplication>();
            var deserializer = new DeserializerBuilder().Build();

            try
            {
                await git.CloneWithBranchAsync(dataPlaneUrl, dataPlaneDraftBranch);
                await git.PullAsync(dataPlaneDraftBranch);

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

        public async Task<string> GetDraftAppCustomConfig(string orgId, GitUtil git, string appName)
        {
            var apps = await GetApplicationsDraft(orgId, git, false);
            var app = await GetFileContent(orgId, git, appName, "values-local.yaml", apps, true);

            return app.Content!;
        }

        public async Task<string> SaveDraftAppCustomConfig(string orgId, GitUtil git, string appName, string content)
        {
            var customValueFile = "values-local.yaml";

            var apps = await GetApplicationsDraft(orgId, git, false);
            var app = await GetFileContent(orgId, git, appName, customValueFile, apps, false);

            var fullPath = Path.Combine(app.Directory!, app.Path!, customValueFile);
            var relativePath = Path.Combine(app.Path!, customValueFile);
            var previousContent = app.Content;

            // 1. ✅ validate YAML
            try
            {
                var deserializer = new DeserializerBuilder().Build();
                using var reader = new StringReader(content);
                deserializer.Deserialize<object>(reader);
            }
            catch (YamlException ex)
            {
                return $"ERR:INVALID_YAML - {ex.Message}";
            }
            catch
            {
                return "ERR:INVALID_YAML";
            }

            try
            {
                // 2. ✅ เขียนไฟล์ (overwrite)
                await File.WriteAllTextAsync(fullPath, content);

                // 3. ✅ add + commit + push
                await git.RunGitAsync($"add {relativePath}");
                await git.RunGitAsync($"commit -m \"Update {appName} custom config\"");
                await git.PushAsync(dataPlaneDraftBranch);
            }
            catch (Exception e)
            {
                // 🔥 rollback ถ้า fail
                if (previousContent != null)
                {
                    await File.WriteAllTextAsync(fullPath, previousContent);
                }

                return $"ERR:SAVE_FAILED:{e.Message}:{relativePath}";
            }

            // 4. ✅ อ่านไฟล์กลับมายืนยันว่าเป็นของใหม่จริง
            var savedContent = await File.ReadAllTextAsync(fullPath);

            return savedContent;
        }

        public async Task<string> MergeDraftAppCustomConfig(string orgId, GitUtil git, string appName)
        {
            var draftBranch = dataPlaneDraftBranch; // เช่น "draft"

            try
            {
                // 1. ✅ clone repo ใหม่ (clean state)
                await git.CloneAsync(dataPlaneUrl);

                // 3. ✅ fetch branch ล่าสุด
                await git.RunGitAsync("fetch --all");

                // 4. ✅ checkout main + pull
                await git.RunGitAsync($"checkout {dataPlaneBranch}");
                await git.PullAsync(dataPlaneBranch);

                // 5. ✅ ensure draft ล่าสุด
                await git.RunGitAsync($"checkout {draftBranch}");
                await git.PullAsync(draftBranch);

                // 6. ✅ merge draft -> main
                await git.RunGitAsync($"checkout {dataPlaneBranch}");
                await git.RunGitAsync($"merge {draftBranch}");

                // 7. ✅ push main
                await git.PushAsync(dataPlaneBranch);

                return "OK";
            }
            catch (Exception ex)
            {
                return $"ERR:MERGE_FAILED - {ex.Message}";
            }
            finally
            {
                // 8. ✅ cleanup temp dir
                git.Cleanup();
            }
        }
    }
}
