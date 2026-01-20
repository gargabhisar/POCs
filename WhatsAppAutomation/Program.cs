using ClosedXML.Excel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WhatsAppBulkSender;

class Program
{
    static void Main()
    {
        string excelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "whatsapp_Bronze_Package_Authors.xlsx");
        int minDelaySeconds = 10;
        int maxDelaySeconds = 20;
        Random random = new();

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
        Thread.Sleep(random.Next(15000, 20000));

        using var workbook = new XLWorkbook(excelPath);
        var sheet = workbook.Worksheet(1);
        var rows = sheet.RangeUsed().RowsUsed().Skip(1);

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

Warm greetings from *InkQuills Publishing House*! 📚✨

We’re delighted to welcome you onboard and thank you for choosing InkQuills to publish your book under our *Bronze Publishing Package*. It was a pleasure connecting with you at the *New Delhi World Book Fair 2026*, and we truly appreciate the trust you’ve placed in us.

This message confirms that we have received your _advance payment of ₹1,000_, marking the official start of your publishing journey with us.

We will now be waiting for your _manuscript in typed (digital) format_ so that we can begin the publishing process and take things forward smoothly.

Once again, welcome to the *InkQuills family*. We look forward to working closely with you and bringing your book to life.

Warm regards,
Abhisar Garg
Director
InkQuills Publishing House";

}