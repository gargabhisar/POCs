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

        public WhatsAppController(EnquiryRepository repo, MongoLogRepository mongoLogRepository,WhatsAppService whatsAppService, WhatsAppTemplateService whatsAppTemplateService)
        {
            _mongoLogRepository = mongoLogRepository;
            _whatsAppService = whatsAppService;
            _repo= repo;
            _whatsAppTemplateService = whatsAppTemplateService;
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
