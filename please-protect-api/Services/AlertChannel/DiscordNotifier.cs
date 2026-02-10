using System.Text;
using System.Text.Json;

namespace Its.PleaseProtect.Api.Services;

public class DiscordNotifier
{
    private readonly HttpClient _http;
    private readonly string _webhookUrl;

    public DiscordNotifier(HttpClient http, string url)
    {
        _http = http;
        _webhookUrl = url;
    }

    public async Task SendAsync(AlertmanagerWebhook data)
    {
        var colorValue = data.Status == "firing"
            ? 16711680   // 🔴 red
            : 65280;     // 🟢 green

        var embedsData = data.Alerts.Select(alert =>
        {
            alert.Labels.TryGetValue("alertname", out var name);
            alert.Annotations.TryGetValue("summary", out var summary);
            alert.Labels.TryGetValue("instance", out var instance);

            var labelsText = string.Join("\n",
                alert.Labels.Select(l => $"**{l.Key}:** {l.Value}")
            );

            return new
            {
                title = name ?? "Alert",
                description =
                    $"**Summary:** {summary}\n" +
                    labelsText + "\n" +
                    $"**Started:** {alert.StartsAt:yyyy-MM-dd HH:mm:ss}",
                color = colorValue
            };
        }).Take(10); // Discord limit 10 embeds

        var summary = data.Alerts != null && data.Alerts.Count > 0 && data.Alerts[0].Annotations.ContainsKey("summary") ? data.Alerts[0].Annotations["summary"] : "No Summary";
        
        var payload = new
        {
            username = "AlertManager",
            content = $"🚨 **[{data.Status.ToUpper()}:{data.Alerts!.Count}]** {summary}",
            embeds = embedsData
        };

        var json = JsonSerializer.Serialize(payload);

        await _http.PostAsync(
            _webhookUrl,
            new StringContent(json, Encoding.UTF8, "application/json")
        );
    }
}
