using ClosedXML.Excel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WhatsAppBulkSender;

class Program
{
    static void Main()
    {
        string excelPath = "C:/Users/abhis/Downloads/whatsapp_bulk.xlsx";
        int minDelaySeconds = 10;
        int maxDelaySeconds = 20;

        Console.WriteLine("🔧 Setting up ChromeDriver...");

        ChromeOptions options = new();
        options.AddArgument(@"--user-data-dir=C:\whatsapp-profile");
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--remote-debugging-port=9222");

        using IWebDriver driver = new ChromeDriver(options);

        Console.WriteLine("🌐 Opening WhatsApp Web...");
        driver.Navigate().GoToUrl("https://web.whatsapp.com");

        Console.WriteLine("📱 Scan QR if required (15 seconds)...");
        
        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheet(1);
        var rows = sheet.RangeUsed().RowsUsed().Skip(1);

        Random random = new();

        foreach (var row in rows)
        {
            var status = row.Cell(3);
            // ✅ Send ONLY if status is empty / null
            if (!status.IsEmpty() &&
                !string.IsNullOrWhiteSpace(status.GetString()))
            {
                Console.WriteLine($"⏭ Skipping {row.Cell(2).GetString()} | Status: {status.GetString()}");
                continue;
            }

            string phone = row.Cell(1).GetString().Trim();
            string name = row.Cell(2).GetString().Trim();

            string message = MessageTemplate.Replace("{{AuthorName}}", name);
            string url =
                $"https://web.whatsapp.com/send?phone={phone}&text={Uri.EscapeDataString(message)}";

            try
            {
                driver.Navigate().GoToUrl(url);
                Thread.Sleep(random.Next(15000, 20000));

                driver.FindElement(By.XPath("//button[@aria-label='Send']")).Click();
                row.Cell(3).Value = "Sent";

                int delay = random.Next(minDelaySeconds, maxDelaySeconds);
                Console.WriteLine($"✅ Sent to {name} | Waiting {delay}s");
                Thread.Sleep(delay * 1000);
            }
            catch (Exception ex)
            {
                row.Cell(3).Value = "Failed";
                Console.WriteLine($"❌ Failed for {name} | {ex.Message}");
                Thread.Sleep(5000);
            }
        }

        workbook.Save();
        driver.Quit();

        Console.WriteLine("📄 Done! Excel updated.");
    }

    static readonly string MessageTemplate = @"Hello {{AuthorName}}! 👋

*InkQuills Publishing House* will be present at the *_New Delhi World Book Fair 2026_* at *_Bharat Mandapam (10–18 Jan 2026)_* — India’s biggest celebration of books and readers.

This is a wonderful opportunity to *showcase your book to lakhs of readers*, gain visibility at our stall, and engage directly through curated book displays, reader interactions, signing sessions, and creative promotions across the fair and social media.

If you’d like your book to be *featured and highlighted at the fair*, just reply “YES” and we’ll share the available options best suited for your book. Participation is limited and allotted on a first-come basis.

✨ *Even if you choose not to feature your book this time, do drop by our stall* — meet the InkQuills team, connect with fellow authors, and be part of the growing author community.

Looking forward to meeting you at India’s biggest book fair! 📖

— Team InkQuills";
}