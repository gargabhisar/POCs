using BookInventory.Models;
using BookInventory.Repositories;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class WhatsAppController : Controller
    {
        private readonly MongoLogRepository _mongoLogRepository;
        private readonly WhatsAppService _whatsAppService;
        private readonly EnquiryRepository _repo;
        private readonly WhatsAppTemplateService _whatsAppTemplateService;
        private readonly ConversationRepository _conversationRepo;
        private readonly MessageRepository _messageRepo;

        public WhatsAppController(EnquiryRepository repo, MongoLogRepository mongoLogRepository, WhatsAppService whatsAppService,
            WhatsAppTemplateService whatsAppTemplateService, ConversationRepository conversationRepo,
            MessageRepository messageRepo)
        {
            _repo = repo;
            _mongoLogRepository = mongoLogRepository;
            _whatsAppService = whatsAppService;
            _whatsAppTemplateService = whatsAppTemplateService;
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Send()
        {
            var templates = await _whatsAppTemplateService.GetTemplatesAsync();

            ViewBag.Templates = templates;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                ViewBag.Error = "Template name is required";
                return View();
            }

            var enquiries = _repo.GetAll();

            foreach (var e in enquiries)
            {
                if (string.IsNullOrWhiteSpace(e.Mobile))
                    continue;

                var result = await _whatsAppService.SendTemplateAsync(e.Mobile, templateName);

                // 1️⃣ Conversation
                var conversation =
                    await _conversationRepo.GetOrCreateAsync($"91{e.Mobile}");

                // 2️⃣ Store message
                await _messageRepo.InsertAsync(new Message
                {
                    ConversationId = conversation.Id,
                    Direction = "OUT",
                    MessageType = "template",
                    Text = templateName,
                    TemplateName = templateName,
                    WaMessageId = result.WaMessageId,
                    DeliveryStatus = "sent",
                    Timestamp = DateTime.UtcNow
                });

                // 3️⃣ Update conversation preview
                await _conversationRepo.UpdateLastMessageAsync(
                    conversation.Id,
                    templateName,
                    "OUT"
                );

                // (Optional) keep old log for now
                await _mongoLogRepository.SaveAsync(new WhatsAppResponseLog
                {
                    Mobile = e.Mobile,
                    TemplateName = templateName,
                    HttpStatus = result.HttpStatus,
                    Response = result.RawResponse,
                    WaMessageId = result.WaMessageId, // 🔑 IMPORTANT
                    SentAt = DateTime.UtcNow
                });

                await Task.Delay(1500); // rate-limit safety
            }

            ViewBag.Success = "Messages sent successfully";
            return View();
        }
    }
}
