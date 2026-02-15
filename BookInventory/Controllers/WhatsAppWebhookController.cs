using BookInventory.Data;
using BookInventory.Models;
using BookInventory.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Text.Json;

namespace BookInventory.Controllers
{
    [Route("whatsapp-webhook")]
    public class WhatsAppWebhookController : Controller
    {        
        private const string VERIFY_TOKEN = "inkquills_verify_token";
        private readonly IMongoCollection<WhatsAppResponseLog> _logs;
        private readonly ConversationRepository _conversationRepo;
        private readonly MessageRepository _messageRepo;

        public WhatsAppWebhookController(MongoContext context, ConversationRepository conversationRepo, MessageRepository messageRepo)
        {
            _logs = context.Database.GetCollection<WhatsAppResponseLog>("WhatsAppResponseLogs");
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
        }

        // ===============================
        // META VERIFICATION (GET)
        // ===============================
        [HttpGet]
        public IActionResult Verify()
        {
            var mode = Request.Query["hub.mode"].ToString();
            var token = Request.Query["hub.verify_token"].ToString();
            var challenge = Request.Query["hub.challenge"].ToString();

            if (mode == "subscribe" && token == VERIFY_TOKEN)
            {
                return Content(challenge, "text/plain");
            }

            return StatusCode(403);
        }

        // ===============================
        // RECEIVE EVENTS (POST)
        // STORE INBOUND + UPDATE DELIVERY
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] JsonElement payload)
        {
            try
            {
                // 1️⃣ entry[]
                if (!payload.TryGetProperty("entry", out var entry) || entry.GetArrayLength() == 0)
                    return Ok();

                var changes = entry[0].GetProperty("changes");
                if (changes.GetArrayLength() == 0)
                    return Ok();

                var value = changes[0].GetProperty("value");

                // ===============================
                // 2️⃣ INBOUND USER MESSAGES
                // ===============================
                if (value.TryGetProperty("messages", out var messages))
                {
                    var message = messages[0];

                    var text =
                        message.TryGetProperty("text", out var txt)
                            ? txt.GetProperty("body").GetString()
                            : "[unsupported message]";

                    var from = message.GetProperty("from").GetString();

                    var contactName =
                        value.TryGetProperty("contacts", out var contacts)
                            ? contacts[0].GetProperty("profile").GetProperty("name").GetString()
                            : null;

                    // 🔑 Conversation
                    var conversation =
                        await _conversationRepo.GetOrCreateAsync(from, contactName);

                    // 📩 Store IN message
                    await _messageRepo.InsertAsync(new Message
                    {
                        ConversationId = conversation.Id,
                        Direction = "IN",
                        MessageType = "text",
                        Text = text,
                        Timestamp = DateTime.UtcNow
                    });

                    // 🔄 Update conversation preview
                    await _conversationRepo.UpdateLastMessageAsync(
                        conversation.Id,
                        text,
                        "IN"
                    );

                    return Ok();
                }

                // ===============================
                // 3️⃣ DELIVERY / READ STATUS
                // ===============================
                if (value.TryGetProperty("statuses", out var statuses))
                {
                    var status = statuses[0];

                    var waMessageId = status.GetProperty("id").GetString();
                    var deliveryStatus = status.GetProperty("status").GetString();

                    if (!string.IsNullOrWhiteSpace(waMessageId) &&
                        !string.IsNullOrWhiteSpace(deliveryStatus))
                    {
                        await _messageRepo.UpdateStatusAsync(
                            waMessageId,
                            deliveryStatus
                        );
                    }

                    return Ok();
                }
            }
            catch
            {
                // 🔒 Webhooks must NEVER crash
            }

            return Ok();
        }
    }
}
