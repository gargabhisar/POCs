using ClosedXML.Excel;
using System.Net.Http;

Console.OutputEncoding = System.Text.Encoding.UTF8;

const string ExcelFilePath = "books.xlsx";
const string OutputFolder = "output";
const string BaseQrApi = "https://api.qrserver.com/v1/create-qr-code/";

// QR CONFIG (as per your URL)
const string Size = "1000x1000";
const string Ecc = "H";
const string Color = "001559";           // InkQuills dark blue
const string BgColor = "255-255-255";
const int Margin = 16;

Directory.CreateDirectory(OutputFolder);

using HttpClient httpClient = new();

using var workbook = new XLWorkbook(ExcelFilePath);
var worksheet = workbook.Worksheet(1);

var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // skip header

foreach (var row in rows)
{
    string bookName = row.Cell(1).GetString().Trim();
    string url = row.Cell(2).GetString().Trim();

    if (string.IsNullOrWhiteSpace(bookName))
        continue;

    if (string.IsNullOrWhiteSpace(url) || url.Equals("N/A", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"⏭️ Skipped (URL N/A): {bookName}");
        continue;
    }

    // Make filename safe
    string safeFileName = string.Join("_",
        bookName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

    string qrUrl =
        $"{BaseQrApi}?" +
        $"size={Size}" +
        $"&data={Uri.EscapeDataString(url)}" +
        $"&margin={Margin}" +
        $"&ecc={Ecc}" +
        $"&color={Color}" +
        $"&bgcolor={BgColor}";

    try
    {
        byte[] qrBytes = await httpClient.GetByteArrayAsync(qrUrl);

        string filePath = Path.Combine(OutputFolder, $"{safeFileName}.png");
        await File.WriteAllBytesAsync(filePath, qrBytes);

        Console.WriteLine($"✅ QR generated: {safeFileName}.png");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed for: {bookName}");
        Console.WriteLine(ex.Message);
    }
}

Console.WriteLine("🎉 All QR codes generated successfully.");
