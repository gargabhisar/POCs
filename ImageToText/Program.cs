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

    private static readonly string MasterDocxPath = Path.Combine(OutputFolder, "All-Hindi-Texts.docx");

    static void Main()
    {
        Directory.CreateDirectory(InputFolder);
        Directory.CreateDirectory(OutputFolder);

        // Locate tessdata: we prefer env var, else TesseractPath/tessdata
        string? tessdataDir = GetTessdataDir();
        if (string.IsNullOrWhiteSpace(tessdataDir) || !Directory.Exists(tessdataDir))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  Could not find tessdata. Set TESSDATA_PREFIX to the parent of 'tessdata' or set TesseractPath.");
            Console.ResetColor();
        }

        var images = Directory.EnumerateFiles(InputFolder)
            .Where(IsImageFile)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (images.Count == 0)
        {
            Console.WriteLine($"No images found in {InputFolder}");
            return;
        }

        Console.WriteLine($"🖼 Found {images.Count} image(s). Building a single DOCX...\n");

        using var engine = CreateEngine(tessdataDir);

        // Create master DOCX
        using var doc = WordprocessingDocument.Create(MasterDocxPath, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new Document(new Body());

        // Set document defaults for Hindi-friendly fonts and complex script handling
        EnsureDocDefaults(main);
        EnsureBasicStyles(main);

        int idx = 0;
        foreach (var path in images)
        {
            idx++;
            var fileName = Path.GetFileName(path);

            try
            {
                using var img = Pix.LoadFromFile(path);
                using var page = engine.Process(img);
                var text = page.GetText()?.Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    using var page2 = engine.Process(img, PageSegMode.SingleBlock);
                    text = page2.GetText()?.Trim();
                }
                if (string.IsNullOrWhiteSpace(text))
                    text = "(कोई स्पष्ट पाठ नहीं मिला।)";

                // Append section to master doc
                AppendHeading(main, fileName);
                AppendHindiParagraphs(main, Normalize(text));

                // Page break between items (except after last)
                if (idx < images.Count)
                    InsertPageBreak(main);

                Console.WriteLine($"✅ {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {fileName} failed: {ex.Message}");
            }
        }

        main.Document.Save();
        Console.WriteLine($"\n🎉 All done! → {MasterDocxPath}");
    }

    // ---------- Tesseract ----------
    private static TesseractEngine CreateEngine(string? tessdataDir)
    {
        return tessdataDir is not null
            ? new TesseractEngine(tessdataDir, Languages, EngineMode.Default)
            : new TesseractEngine(@"./tessdata_not_used_if_env_or_path", Languages, EngineMode.Default);
    }

    private static string? GetTessdataDir()
    {
        var env = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
        if (!string.IsNullOrWhiteSpace(env))
            return Path.Combine(env, "tessdata");

        if (!string.IsNullOrWhiteSpace(TesseractPath))
            return Path.Combine(TesseractPath!, "tessdata");

        var win1 = @"C:\Program Files\Tesseract-OCR\tessdata";
        if (Directory.Exists(win1)) return win1;

        var win2 = @"C:\Program Files (x86)\Tesseract-OCR\tessdata";
        if (Directory.Exists(win2)) return win2;

        var macArm = "/opt/homebrew/share/tessdata";
        if (Directory.Exists(macArm)) return macArm;

        var macIntel = "/usr/local/share/tessdata";
        if (Directory.Exists(macIntel)) return macIntel;

        var linux1 = "/usr/share/tesseract-ocr/4.00/tessdata";
        if (Directory.Exists(linux1)) return linux1;

        var linux2 = "/usr/share/tessdata";
        if (Directory.Exists(linux2)) return linux2;

        return null;
    }

    // ---------- DOCX helpers ----------
    private static void EnsureDocDefaults(MainDocumentPart main)
    {
        if (main.StyleDefinitionsPart == null)
            main.AddNewPart<StyleDefinitionsPart>().Styles = new Styles();

        var styles = main.StyleDefinitionsPart.Styles!;
        if (styles.DocDefaults == null)
        {
            styles.DocDefaults = new DocDefaults(
                new RunPropertiesDefault(
                    new RunPropertiesBaseStyle(
                        new RunFonts { Ascii = "Nirmala UI", HighAnsi = "Nirmala UI", ComplexScript = "Mangal" },
                        new FontSize { Val = "24" } // 12pt default
                    )
                ),
                new ParagraphPropertiesDefault(new ParagraphProperties(new BiDi()))
            );
        }
    }

    private static void EnsureBasicStyles(MainDocumentPart main)
    {
        if (main.StyleDefinitionsPart == null)
            main.AddNewPart<StyleDefinitionsPart>().Styles = new Styles();

        var styles = main.StyleDefinitionsPart.Styles!;
        if (styles.Elements<Style>().Any(s => s.StyleId == "Heading1")) return;

        styles.AppendChild(
            new Style(
                new StyleName() { Val = "heading 1" },
                new BasedOn() { Val = "Normal" },
                new UIPriority() { Val = 9 },
                new PrimaryStyle(),
                new StyleParagraphProperties(
                    new KeepNext(), new KeepLines(),
                    new SpacingBetweenLines() { After = "200" }
                ),
                new StyleRunProperties(
                    new Bold(),
                    new RunFonts { Ascii = "Nirmala UI", HighAnsi = "Nirmala UI", ComplexScript = "Mangal" },
                    new FontSize() { Val = "32" } // 16pt
                )
            )
            { Type = StyleValues.Paragraph, StyleId = "Heading1" }
        );
    }

    private static void AppendHeading(MainDocumentPart main, string headingText)
    {
        var pPr = new ParagraphProperties(new ParagraphStyleId() { Val = "Heading1" });
        var run = new Run(new Text(headingText));
        var para = new Paragraph(pPr, run);
        main.Document.Body!.AppendChild(para);
    }

    private static void AppendHindiParagraphs(MainDocumentPart main, IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            var run = new Run(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
            var para = new Paragraph(run)
            {
                ParagraphProperties = new ParagraphProperties(new BiDi())
            };
            main.Document.Body!.AppendChild(para);
        }
    }

    private static void InsertPageBreak(MainDocumentPart main)
    {
        var breakPara = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
        main.Document.Body!.AppendChild(breakPara);
    }

    private static IEnumerable<string> Normalize(string s)
    {
        s ??= "";
        s = s.Replace("\r\n", "\n");
        s = s.Replace("-\n", "");  // fix hyphen linebreaks
        return s.Split('\n');
    }

    // ---------- misc ----------
    private static bool IsImageFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".webp" or ".bmp" or ".tif" or ".tiff";
    }
}
