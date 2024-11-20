// See https://aka.ms/new-console-template for more information
using System;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using Newtonsoft.Json;

// Path to the Excel file
string excelFilePath = @"Assets/Authors.xlsx";

// Path to save the JSON file
string jsonFilePath = @"Assets/Output.json";

// Convert Excel to JSON
var dataTable = new DataTable();

using (var workbook = new XLWorkbook(excelFilePath))
{
    // Access the first worksheet
    var worksheet = workbook.Worksheet(1);

    // Read header row
    var headerRow = worksheet.FirstRowUsed();
    foreach (var cell in headerRow.Cells())
    {
        dataTable.Columns.Add(cell.GetValue<string>());
    }

    // Read the rest of the rows
    foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip header
    {
        var rowData = dataTable.NewRow();
        for (int col = 1; col <= dataTable.Columns.Count; col++)
        {
            rowData[col - 1] = row.Cell(col).GetValue<string>();
        }
        dataTable.Rows.Add(rowData);
    }
}

string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);

// Save JSON to a text file
File.WriteAllText(jsonFilePath, json);

Console.WriteLine($"JSON has been saved to: {jsonFilePath}");