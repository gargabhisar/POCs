using BookInventory.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;

namespace BookInventory.Controllers
{
    [Route("whatsapp-webhook")]
    public class WhatsAppWebhookController : Controller
    {
        private readonly IMongoCollection<WhatsAppResponseLog> _logs;
        private const string VERIFY_TOKEN = "inkquills_verify_token";

        public WhatsAppWebhookController(IMongoDatabase database)
        {
            _logs = database.GetCollection<WhatsAppResponseLog>("WhatsAppResponseLogs");
        }

        // ===============================
        // META VERIFICATION (GET)
        // ===============================
        [HttpGet]
        public IActionResult Verify()
        {
            var mode = Request.Query["hub_mode"].ToString();
            var token = Request.Query["hub_verify_token"].ToString();
            var challenge = Request.Query["hub_challenge"].ToString();

            if (mode == "subscribe" && token == VERIFY_TOKEN)
            {
                return Content(challenge, "text/plain");
            }

            return Forbid();
        }

        // ===============================
        // RECEIVE EVENTS (POST)
        // LOG TO FILE + UPDATE MONGODB
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] JsonElement payload)
        {
            // 1️⃣ Log raw payload to file (always)
            try
            {
                var raw = JsonSerializer.Serialize(payload);
                System.IO.File.AppendAllText(
                    Path.Combine(AppContext.BaseDirectory, "whatsapp-webhook.log"),
                    DateTime.UtcNow + " " + raw + Environment.NewLine
                );
            }
            catch
            {
                // Ignore logging failures
            }

            try
            {
                var value = payload
                    .GetProperty("entry")[0]
                    .GetProperty("changes")[0]
                    .GetProperty("value");

                // 2️⃣ Ignore incoming user messages (for now)
                if (value.TryGetProperty("messages", out _))
                {
                    return Ok();
                }

                // 3️⃣ Process delivery/read statuses
                if (!value.TryGetProperty("statuses", out var statuses))
                {
                    return Ok();
                }

                var status = statuses[0];

                var waMessageId = status.GetProperty("id").GetString();
                var deliveryStatus = status.GetProperty("status").GetString();

                if (string.IsNullOrWhiteSpace(waMessageId))
                    return Ok();

                var update = Builders<WhatsAppResponseLog>.Update
                    .Set(x => x.DeliveryStatus, deliveryStatus)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);

                await _logs.UpdateOneAsync(
                    x => x.WaMessageId == waMessageId,
                    update
                );
            }
            catch
            {
                // Never fail webhook
            }

            return Ok();
        }
    }
}
