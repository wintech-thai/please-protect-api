namespace Its.PleaseProtect.Api.Utils;

using System.Diagnostics;

public class GitUtil
{
    private readonly string _workingDir;
    private readonly string _user;
    private readonly string _password;

    public GitUtil(string workingDir)
    {
        _workingDir = ValidateAndPreparePath(workingDir);

        _user = Environment.GetEnvironmentVariable("GIT_USER") ?? "";
        _password = Environment.GetEnvironmentVariable("GIT_PASSWORD") ?? "";
    }

    // ✅ validate + create dir
    private string ValidateAndPreparePath(string path)
    {
        var fullPath = Path.GetFullPath(path);

        // 🔥 กัน path escape เช่น ../../
        if (!fullPath.StartsWith("/tmp"))
        {
            throw new Exception("Working directory must be under /tmp");
        }

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }

    private async Task RunGitAsync(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = args,
            WorkingDirectory = _workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi)!;

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Git error: {error}");
        }
    }

    private string InjectCredential(string repoUrl)
    {
        var uri = new Uri(repoUrl);

        if (uri.Scheme != "http" && uri.Scheme != "https")
            return repoUrl;

        var builder = new UriBuilder(uri)
        {
            UserName = _user,
            Password = _password
        };

        return builder.Uri.ToString();
    }

    public async Task CloneAsync(string repoUrl)
    {
        var url = InjectCredential(repoUrl);
        await RunGitAsync($"clone {url} .");
    }

    public Task CreateBranchAsync(string branch)
        => RunGitAsync($"checkout -b {branch}");

    public Task DeleteBranchAsync(string branch)
        => RunGitAsync($"branch -D {branch}");

    public Task PullAsync(string branch)
        => RunGitAsync($"pull origin {branch}");

    public Task PushAsync(string branch)
        => RunGitAsync($"push origin {branch}");

    public async Task MergeAsync(string sourceBranch, string targetBranch)
    {
        await RunGitAsync($"checkout {targetBranch}");
        await RunGitAsync($"merge {sourceBranch}");
    }

    public void Cleanup()
    {
        if (_workingDir.StartsWith("/tmp") && Directory.Exists(_workingDir))
        {
            Directory.Delete(_workingDir, true);
        }
    }

    public string GetWorkingDir()
    {
        return _workingDir;
    }
}
