using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BookInventory.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public WhatsAppService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    config["WhatsApp:AccessToken"]);
        }

        public async Task<(int Status, string Response)>
            SendTemplateAsync(string mobile, string templateName)
        {
            var url =
                $"https://graph.facebook.com/v22.0/" +
                $"{_config["WhatsApp:PhoneNumberId"]}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = $"91{mobile}",
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = "en" }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();

            return ((int)response.StatusCode, body);
        }
    }
}
