// See https://aka.ms/new-console-template for more information
using GenerateQRCode;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

#region Basic QR Code
string qrString = null;

string path = AppDomain.CurrentDomain.BaseDirectory;

while (qrString == null)
{
    Console.WriteLine("Enter your string: ");
    qrString = Console.ReadLine();
}

QRCodeGenerator QrGenerator = new QRCodeGenerator();
QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);

QRCode QrCode = new QRCode(QrCodeInfo);
Bitmap QrBitmap = QrCode.GetGraphic(60, Color.Black, Color.White, true);

string filename = @$"{path}{qrString.RemoveWhitespace()}.png";
QrBitmap.Save(filename, ImageFormat.Png);
#endregion

#region QR Code with text 
//string text = "www.inkquills.in";
//QRCodeGenerator qrGenerator = new QRCodeGenerator();
//QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
//QRCode qrCode = new QRCode(qrCodeData);
//var mainColor = ColorTranslator.FromHtml("#001559");
//Bitmap qrCodeImage = qrCode.GetGraphic(60, mainColor, Color.Transparent, true); // Adjust size as needed

//// Add custom text
//string customText = text;
//using (Graphics graphics = Graphics.FromImage(qrCodeImage))
//{
//    using (Font font = new Font("Arial", 48)) // Adjust font and size as needed
//    {
//        using (Brush brush = new SolidBrush(mainColor))
//        {
//            // Calculate text size and position
//            SizeF textSize = graphics.MeasureString(customText, font);
//            float x = (qrCodeImage.Width - textSize.Width) / 2; // Center horizontally
//            float y = qrCodeImage.Height - (textSize.Height * 2); // Bottom of the image

//            // Draw text
//            graphics.DrawString(customText, font, brush, x, y);
//        }
//    }
//}

//// Save or display the QR code image
//qrCodeImage.Save($"{text}.png", ImageFormat.Png);
//Console.WriteLine("QR code with custom text generated.");
#endregion

#region QR code with Text and Logo
//string text = "www.inkquills.in";
//QRCodeGenerator qrGenerator = new QRCodeGenerator();
//QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
//QRCode qrCode = new QRCode(qrCodeData);
//Bitmap qrCodeImage = qrCode.GetGraphic(20); // Adjust size as needed

//// Add custom text
//string customText = text;
//using (Graphics graphics = Graphics.FromImage(qrCodeImage))
//{
//    using (Font font = new Font("Arial", 12)) // Adjust font and size as needed
//    {
//        using (Brush brush = new SolidBrush(Color.Black))
//        {
//            // Calculate text size and position
//            SizeF textSize = graphics.MeasureString(customText, font);
//            float x = (qrCodeImage.Width - textSize.Width) / 2; // Center horizontally
//            float y = qrCodeImage.Height - textSize.Height; // Bottom of the image

//            // Draw text
//            graphics.DrawString(customText, font, brush, x, y);
//        }
//    }

//    // Add logo
//    string logoPath = "C:\\Users\\abhis\\Desktop\\All\\AI Images\\Logoincircle.png"; // Path to your logo image
//    if (File.Exists(logoPath))
//    {
//        Bitmap logo = new Bitmap(logoPath);
//        // Calculate logo position
//        int logoWidth = qrCodeImage.Width / 4; // Adjust size as needed
//        int logoHeight = qrCodeImage.Height / 4; // Adjust size as needed
//        int x = (qrCodeImage.Width - logoWidth) / 2; // Center horizontally
//        int y = (qrCodeImage.Height - logoHeight) / 2; // Center vertically

//        // Draw logo
//        graphics.DrawImage(logo, x, y, logoWidth, logoHeight);
//    }
//    else
//    {
//        Console.WriteLine("Logo image not found.");
//    }
//}
//// Save or display the QR code image
//qrCodeImage.Save($"{text}.png", ImageFormat.Png);
//Console.WriteLine("QR code with custom text and logo generated.");
#endregion