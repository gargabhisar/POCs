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

        public async Task<WhatsAppSendResult> SendTemplateAsync(string mobile, string templateName)
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

            string waMessageId = null;

            // 🔑 Safely extract wamid (if present)
            try
            {
                using var doc = JsonDocument.Parse(body);
                waMessageId = doc.RootElement.GetProperty("messages")[0].GetProperty("id").GetString();
            }
            catch
            {
                // Not all responses contain wamid (errors, throttling, etc.)
            }

            return new WhatsAppSendResult
            {
                HttpStatus = (int)response.StatusCode,
                RawResponse = body,
                WaMessageId = waMessageId,
                Status = response.IsSuccessStatusCode ? "sent" : "failed"
            };
        }

        public async Task<WhatsAppSendResult> SendTextAsync(string phoneNumber, string text)
        {
            var url =
                $"https://graph.facebook.com/v22.0/" +
                $"{_config["WhatsApp:PhoneNumberId"]}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "text",
                text = new
                {
                    body = text
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();

            string waMessageId = null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                waMessageId =
                    doc.RootElement
                       .GetProperty("messages")[0]
                       .GetProperty("id")
                       .GetString();
            }
            catch { }

            return new WhatsAppSendResult
            {
                HttpStatus = (int)response.StatusCode,
                RawResponse = body,
                WaMessageId = waMessageId,
                Status = response.IsSuccessStatusCode ? "sent" : "failed"
            };
        }
    }

    // ===============================
    // RESULT DTO (IMPORTANT)
    // ===============================
    public class WhatsAppSendResult
    {
        public int HttpStatus { get; set; }
        public string RawResponse { get; set; }
        public string WaMessageId { get; set; }
        public string Status { get; set; } // sent | failed
    }
}