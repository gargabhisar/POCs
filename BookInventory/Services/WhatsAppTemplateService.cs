using BookInventory.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookInventory.Services
{
    public class WhatsAppTemplateService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public WhatsAppTemplateService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    config["WhatsApp:AccessToken"]);
        }

        public async Task<List<WhatsAppTemplateDto>> GetTemplatesAsync()
        {
            var wabaId = _config["WhatsApp:WabaId"];

            var url = $"https://graph.facebook.com/v22.0/{wabaId}/message_templates" + "?fields=name,status,language,category&limit=100";

            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            var templates = new List<WhatsAppTemplateDto>();

            using var doc = JsonDocument.Parse(body);

            if (!doc.RootElement.TryGetProperty("data", out var data))
                return templates;

            foreach (var item in data.EnumerateArray())
            {
                templates.Add(new WhatsAppTemplateDto
                {
                    Name = item.GetProperty("name").GetString(),
                    Status = item.GetProperty("status").GetString(),
                    Language = item.GetProperty("language").GetString(),
                    Category = item.GetProperty("category").GetString()
                });
            }

            return templates;
        }
    }
}
