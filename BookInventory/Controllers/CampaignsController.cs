using BookInventory.Repositories;
using BookInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Controllers
{
    public class CampaignsController : Controller
    {
        private readonly CampaignRepository _campaignRepo;
        private readonly CampaignRecipientRepository _recipientRepo;
        private readonly EnquiryRepository _enquiryRepo;
        private readonly WhatsAppService _whatsAppService;
        private readonly WhatsAppTemplateService _whatsAppTemplateService;

        public CampaignsController(
            CampaignRepository campaignRepo,
            CampaignRecipientRepository recipientRepo,
            EnquiryRepository enquiryRepo,
            WhatsAppService whatsAppService,
            WhatsAppTemplateService whatsAppTemplateService)
        {
            _campaignRepo = campaignRepo;
            _recipientRepo = recipientRepo;
            _enquiryRepo = enquiryRepo;
            _whatsAppService = whatsAppService;
            _whatsAppTemplateService = whatsAppTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Templates = await _whatsAppTemplateService.GetTemplatesAsync();
            ViewBag.Enquiries = _enquiryRepo.GetAll();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            string campaignName,
            string templateName,
            List<string> selectedNumbers)
        {
            if (string.IsNullOrWhiteSpace(campaignName) ||
                string.IsNullOrWhiteSpace(templateName) ||
                selectedNumbers == null || !selectedNumbers.Any())
            {
                ViewBag.Error = "All fields are required";
                ViewBag.Enquiries = _enquiryRepo.GetAll();
                return View();
            }

            // 1️⃣ Create campaign
            var campaign = await _campaignRepo.CreateAsync(
                campaignName,
                templateName
            );

            await _campaignRepo.UpdateStatusAsync(campaign.Id, "Sending");

            // 2️⃣ Send messages
            foreach (var mobile in selectedNumbers)
            {
                var result = await _whatsAppService.SendTemplateAsync(
                    mobile,
                    templateName
                );

                await _recipientRepo.InsertAsync(new Models.CampaignRecipient
                {
                    CampaignId = campaign.Id,
                    PhoneNumber = mobile,
                    WaMessageId = result.WaMessageId,
                    DeliveryStatus = result.Status,
                    SentAt = DateTime.UtcNow
                });

                await Task.Delay(1200); // safe rate-limit
            }

            await _campaignRepo.UpdateStatusAsync(campaign.Id, "Completed");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var campaigns = await _campaignRepo.GetAllAsync();
            return View(campaigns);
        }
    }
}
