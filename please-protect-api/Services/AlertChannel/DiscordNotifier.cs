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
        var color = data.Status == "firing"
            ? 16711680   // 🔴 red
            : 65280;     // 🟢 green

        var embeds = data.Alerts.Select(alert =>
        {
            alert.Labels.TryGetValue("alertname", out var name);
            alert.Annotations.TryGetValue("summary", out var summary);
            alert.Labels.TryGetValue("instance", out var instance);

            return new
            {
                title = name ?? "Alert",
                description =
                    $"**Summary:** {summary}\n" +
                    $"**Instance:** {instance}\n" +
                    $"**Started:** {alert.StartsAt:yyyy-MM-dd HH:mm:ss}",
                color = color
            };
        }).Take(10); // Discord limit 10 embeds

        var payload = new
        {
            username = "AlertManager",
            content = $"🚨 **{data.Status.ToUpper()}** ({data.Alerts.Count} alerts)",
            embeds = embeds
        };

        var json = JsonSerializer.Serialize(payload);

        await _http.PostAsync(
            _webhookUrl,
            new StringContent(json, Encoding.UTF8, "application/json")
        );
    }
}
