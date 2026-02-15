using BookInventory.Models;
using BookInventory.Repositories;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class InboxController : Controller
    {
        private readonly ConversationRepository _conversationRepo;
        private readonly MessageRepository _messageRepo;
        private readonly WhatsAppService _whatsAppService;

        public InboxController(ConversationRepository conversationRepo, MessageRepository messageRepo, WhatsAppService whatsAppService)
        {
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
            _whatsAppService = whatsAppService;
        }

        // Left panel: conversation list
        public async Task<IActionResult> Index(string conversationId = null)
        {
            var conversations = await _conversationRepo.GetAllAsync();

            ViewBag.Conversations = conversations;

            if (!string.IsNullOrEmpty(conversationId))
            {
                ViewBag.SelectedConversation =
                    await _conversationRepo.GetByIdAsync(conversationId);

                ViewBag.Messages =
                    await _messageRepo.GetByConversationAsync(conversationId);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendReply(string conversationId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return RedirectToAction("Index", new { conversationId });

            var conversation =
                await _conversationRepo.GetByIdAsync(conversationId);

            if (conversation == null)
                return RedirectToAction("Index");

            // 1️⃣ Send via WhatsApp
            var result = await _whatsAppService.SendTextAsync(
                conversation.PhoneNumber,
                message
            );

            // 2️⃣ Store OUT message
            await _messageRepo.InsertAsync(new Message
            {
                ConversationId = conversation.Id,
                Direction = "OUT",
                MessageType = "text",
                Text = message,
                WaMessageId = result.WaMessageId,
                DeliveryStatus = "sent",
                Timestamp = DateTime.UtcNow
            });

            // 3️⃣ Update conversation preview
            await _conversationRepo.UpdateLastMessageAsync(
                conversation.Id,
                message,
                "OUT"
            );

            return RedirectToAction("Index", new { conversationId });
        }

        [HttpGet]
        public async Task<IActionResult> Messages(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                return new EmptyResult();

            var messages =
                await _messageRepo.GetByConversationAsync(conversationId);

            return PartialView("_Messages", messages);
        }
    }
}
