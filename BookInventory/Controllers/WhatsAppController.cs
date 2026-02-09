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

        public WhatsAppController(EnquiryRepository repo, MongoLogRepository mongoLogRepository,WhatsAppService whatsAppService)
        {
            _mongoLogRepository = mongoLogRepository;
            _whatsAppService = whatsAppService;
            _repo= repo;
        }

        [HttpGet]
        public IActionResult Send()
        {
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

                var (status, response) =
                    await _whatsAppService.SendTemplateAsync(e.Mobile, templateName);

                await _mongoLogRepository.SaveAsync(new WhatsAppResponseLog
                {
                    Mobile = e.Mobile,
                    TemplateName = templateName,
                    HttpStatus = status,
                    Response = response,
                    SentAt = DateTime.UtcNow
                });

                await Task.Delay(1500); // rate-limit safety
            }

            ViewBag.Success = "Messages sent successfully";
            return View();
        }
    }
}
