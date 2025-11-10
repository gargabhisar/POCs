using System;
using System.IO;
using SkiaSharp;

class Program
{
    static void Main()
    {
        Console.Write("Enter 13-digit ISBN: ");
        string isbn = Console.ReadLine()?.Trim() ?? "";

        if (isbn.Length != 13 || !long.TryParse(isbn, out _))
        {
            Console.WriteLine("Invalid ISBN. It must be 13 digits.");
            return;
        }

        Console.Write("Enter bar color (Hex Code, e.g., #0f3245): ");
        string hexColor = Console.ReadLine()?.Trim() ?? "#000000";

        if (!hexColor.StartsWith("#")) hexColor = "#" + hexColor;
        SKColor barColor;
        try
        {
            barColor = SKColor.Parse(hexColor);
        }
        catch
        {
            Console.WriteLine("Invalid hex color. Using default black.");
            barColor = SKColors.Black;
        }

        Console.Write("Transparent background? (y/n): ");
        bool transparent = Console.ReadLine()?.Trim().ToLower() == "y";

        Console.Write("Enter output file name (without extension): ");
        string outputName = Console.ReadLine()?.Trim() ?? "isbn_barcode";

        string outputPath = $"{outputName}.png";

        GenerateIsbn13Barcode(isbn, barColor, transparent, outputPath);

        Console.WriteLine($"✅ Barcode generated: {Path.GetFullPath(outputPath)}");
    }

    static void GenerateIsbn13Barcode(string isbn, SKColor barColor, bool transparentBackground, string outputPath)
    {
        int barWidth = 2;
        int height = 120;
        int margin = 20;
        int textHeight = 20;
        int width = isbn.Length * 7 * barWidth + margin * 2;

        using var surface = SKSurface.Create(new SKImageInfo(width, height + margin + textHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;

        // Background
        if (!transparentBackground)
            canvas.Clear(SKColors.White);
        else
            canvas.Clear(SKColors.Transparent);

        // Draw barcode pattern
        DrawIsbn13(canvas, isbn, barColor, barWidth, height, margin);

        // Draw text (ISBN below bars)
        using (var paint = new SKPaint
        {
            Color = barColor,
            TextSize = textHeight - 2,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial")
        })
        {
            canvas.DrawText(isbn, width / 2, height + margin + textHeight - 2, paint);
        }

        canvas.Flush();

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }

    static void DrawIsbn13(SKCanvas canvas, string isbn, SKColor barColor, int barWidth, int height, int margin)
    {
        // Simple EAN-13 pattern generator (not fully encoded, but visually correct)
        string pattern = GetIsbn13Pattern(isbn);

        using var paint = new SKPaint
        {
            Color = barColor,
            Style = SKPaintStyle.Fill
        };

        int x = margin;
        foreach (char c in pattern)
        {
            if (c == '1')
                canvas.DrawRect(x, margin, barWidth, height, paint);
            x += barWidth;
        }
    }

    static string GetIsbn13Pattern(string isbn)
    {
        // NOTE: This is a simplified EAN-13 bar sequence (for visual/barcode scan representation).
        // For production-grade barcodes, use ZXing or similar libraries.
        string left = isbn.Substring(1, 6);
        string right = isbn.Substring(7);
        string pattern = "101"; // left guard

        foreach (char c in left)
            pattern += GetDigitPattern(c, true);

        pattern += "01010"; // center guard

        foreach (char c in right)
            pattern += GetDigitPattern(c, false);

        pattern += "101"; // right guard
        return pattern;
    }

    static string GetDigitPattern(char digit, bool leftSide)
    {
        string[] leftPatterns = {
            "0001101","0011001","0010011","0111101","0100011",
            "0110001","0101111","0111011","0110111","0001011"
        };
        string[] rightPatterns = {
            "1110010","1100110","1101100","1000010","1011100",
            "1001110","1010000","1000100","1001000","1110100"
        };
        int d = digit - '0';
        return leftSide ? leftPatterns[d] : rightPatterns[d];
    }
}
