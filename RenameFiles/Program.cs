// See https://aka.ms/new-console-template for more information
//using System.Text.RegularExpressions;

//string folderPath = @"C:\Users\abhis\Desktop\All Covers"; // Replace with your folder path

//// Get all files in the folder
//string[] files = Directory.GetFiles(folderPath);

//// Define a regex to match files with serial numbers at the beginning
//Regex regex = new Regex(@"^(\d+)(_.*)?");

//// Loop through each file and rename it if it matches the pattern
//foreach (string file in files)
//{
//    string fileName = Path.GetFileName(file);
//    Match match = regex.Match(fileName);

//    if (match.Success)
//    {
//        int serialNumber = int.Parse(match.Groups[1].Value);
//        string newSerialNumber = serialNumber.ToString("D4");
//        string newFileName = newSerialNumber + fileName.Substring(match.Groups[1].Value.Length);
//        string newFilePath = Path.Combine(folderPath, newFileName);

//        // Rename the file
//        File.Move(file, newFilePath);
//    }
//}

//Console.WriteLine("Files renamed successfully.");
string folderPath = @"H:\My Drive\Pics\Abhisar's New Life Events\Malaysia\New folder"; // Change this to your folder path
string outputFile = Path.Combine(folderPath, "file_list.txt"); // Output file in the same folder

try
{
    // Get all file names (without path), sort them alphabetically
    string[] fileNames = Directory.GetFiles(folderPath)
                                  .Select(Path.GetFileName)
                                  .OrderBy(name => name)
                                  .ToArray();

    // Write sorted file names to the output text file
    File.WriteAllLines(outputFile, fileNames);

    Console.WriteLine("Sorted file names have been written to " + outputFile);
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}