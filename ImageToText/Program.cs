using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Tesseract;

class Program
{
    // Put your images in ./input
    private static readonly string InputFolder = Path.GetFullPath("C:\\Users\\abhis\\Desktop\\All Pages - Images");
    // DOCX will be written to ./output
    private static readonly string OutputFolder = Path.GetFullPath("C:\\Users\\abhis\\Desktop\\All Pages - Text");

    // If Tesseract is in PATH, leave null.
    // Otherwise set to e.g. @"C:\Program Files\Tesseract-OCR"
    private static readonly string? TesseractPath = @"C:\Program Files (x86)\Tesseract-OCR\tessdata";

    // Tesseract language(s). Hindi is "hin".
    // You can add "+eng" to improve mixed-language text: "hin+eng"
    private const string Languages = "hin";

    static void Main()
    {
        Directory.CreateDirectory(InputFolder);
        Directory.CreateDirectory(OutputFolder);

        // Locate tessdata: we prefer env var, else TesseractPath/tessdata
        string? tessdataDir = GetTessdataDir();
        if (string.IsNullOrWhiteSpace(tessdataDir) || !Directory.Exists(tessdataDir))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  Could not find tessdata. Set TESSDATA_PREFIX env var OR set TesseractPath in code.");
            Console.WriteLine("    Example Windows tessdata: C:\\Program Files\\Tesseract-OCR\\tessdata");
            Console.ResetColor();
        }

        var imageFiles = Directory.EnumerateFiles(InputFolder)
            .Where(IsImageFile)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (imageFiles.Count == 0)
        {
            Console.WriteLine($"No images found in {InputFolder}");
            return;
        }

        Console.WriteLine($"🖼 Found {imageFiles.Count} image(s). Starting Hindi OCR...\n");

        // Create OCR engine
        using var engine = CreateEngine(tessdataDir);

        foreach (var path in imageFiles)
        {
            var baseName = Path.GetFileNameWithoutExtension(path);
            var safeName = Sanitize(baseName);
            var outPath = Path.Combine(OutputFolder, $"{safeName}.docx");

            try
            {
                using var img = Pix.LoadFromFile(path);
                using var page = engine.Process(img);

                // Try both raw text and layout-aware text (hOCR is HTML; we just want text)
                var text = page.GetText()?.Trim();

                // Fallback: try single-block mode for dense text
                if (string.IsNullOrWhiteSpace(text))
                {
                    using var page2 = engine.Process(img, PageSegMode.SingleBlock);
                    text = page2.GetText()?.Trim();
                }

                if (string.IsNullOrWhiteSpace(text))
                    text = "(कोई स्पष्ट पाठ नहीं मिला।)";

                CreateHindiDocx(outPath, text);

                Console.WriteLine($"✅ {Path.GetFileName(path)}  →  {outPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {Path.GetFileName(path)} failed: {ex.Message}");
            }
        }

        Console.WriteLine("\n🎉 Done.");
    }

    // ——— Tesseract engine setup ———
    private static TesseractEngine CreateEngine(string? tessdataDir)
    {
        // If tessdataDir is null, Tesseract will rely on PATH/TESSDATA_PREFIX.
        // EngineMode.Default picks the best available (usually LSTM).
        return tessdataDir is not null
            ? new TesseractEngine(tessdataDir, Languages, EngineMode.Default)
            : new TesseractEngine(@"./tessdata_not_used_if_env_or_path", Languages, EngineMode.Default);
    }

    private static string? GetTessdataDir()
    {
        // Highest priority: TESSDATA_PREFIX env var
        var env = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
        if (!string.IsNullOrWhiteSpace(env))
            return Path.Combine(env, "tessdata").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // If user set TesseractPath above
        if (!string.IsNullOrWhiteSpace(TesseractPath))
            return Path.Combine(TesseractPath!, "tessdata");

        // Common defaults (Windows)
        var winDefault = @"C:\Program Files\Tesseract-OCR\tessdata";
        if (Directory.Exists(winDefault)) return winDefault;

        // Homebrew default (Apple Silicon)
        var macArm = "/opt/homebrew/Cellar/tesseract/5.0.0/share/tessdata";
        if (Directory.Exists(macArm)) return macArm;

        // Homebrew (Intel mac)
        var macIntel = "/usr/local/Cellar/tesseract/5.0.0/share/tessdata";
        if (Directory.Exists(macIntel)) return macIntel;

        // Linux common
        var linux1 = "/usr/share/tesseract-ocr/4.00/tessdata";
        if (Directory.Exists(linux1)) return linux1;

        var linux2 = "/usr/share/tessdata";
        if (Directory.Exists(linux2)) return linux2;

        return null;
    }

    // ——— Word (.docx) writer with Hindi font ———
    private static void CreateHindiDocx(string outputPath, string bodyText)
    {
        using var doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new Document(new Body());

        // Define a default run properties (Hindi-compatible font like Mangal or Nirmala UI)
        var runProps = new RunProperties(
            new RunFonts() { Ascii = "Nirmala UI", HighAnsi = "Nirmala UI", ComplexScript = "Mangal" }, // CS for Devanagari
            new FontSize() { Val = "24" } // 12 pt (OpenXML uses half-points)
        );

        foreach (var line in NormalizeLines(bodyText))
        {
            var run = new Run(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
            run.PrependChild(runProps.CloneNode(true));
            var para = new Paragraph(run);
            // Enable complex script for the paragraph
            para.ParagraphProperties = new ParagraphProperties(new BiDi()); // supports right-to-left/complex scripts
            main.Document.Body!.AppendChild(para);
        }

        main.Document.Save();
    }

    private static IEnumerable<string> NormalizeLines(string s)
    {
        s ??= "";
        s = s.Replace("\r\n", "\n");
        // Optional: merge hyphenated line breaks. Comment this out if you want raw OCR lines.
        s = s.Replace("-\n", "");
        return s.Split('\n');
    }

    // ——— Helpers ———
    private static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".webp" or ".bmp" or ".tif" or ".tiff";
    }

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
