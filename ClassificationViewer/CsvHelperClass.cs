using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ClassificationViewer;

namespace ClassificationViewer
{
    public class CsvHelperClass
    {
        private List<CsvRecord> records = new List<CsvRecord>();
        private string loadedCsvPath;

        public IReadOnlyList<CsvRecord> Records => records;

        public void LoadCsv(string csvPath)
        {
            if (!File.Exists(csvPath))
            {
                MessageBox.Show($"CSV file not found or inaccessible:\n{csvPath}");
                Console.WriteLine($"CSV file not found or inaccessible: {csvPath}");
                return;
            }

            records.Clear();
            loadedCsvPath = csvPath;

            try
            {
                using (var reader = new StreamReader(csvPath, Encoding.UTF8, true)) // auto-detect BOM
                {
                    string headerLine = reader.ReadLine(); // skip header
                    if (headerLine == null)
                    {
                        Console.WriteLine("CSV is empty!");
                        return;
                    }

                    int lineNumber = 1;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        lineNumber++;

                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // Split by COMMA (DO NOT CHANGE TO TAB!!!)
                        var parts = line.Split(',');
                        if (parts.Length < 12)
                        {
                            Console.WriteLine($"Skipping malformed line {lineNumber}: {line}");
                            continue;
                        }

                        for (int i = 0; i < parts.Length; i++)
                            parts[i] = parts[i].Trim();

                        var record = new CsvRecord
                        {
                            RID = parts[0],
                            SU = parts[1],   // string to preserve original format
                            WE = parts[2],
                            Filename1 = parts[3],
                            ROW_folder = parts[4],
                            MinOfChFrom = double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var min) ? min : 0,
                            MaxOfChTo = double.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var max) ? max : 0,
                            SurfaceType = parts[7],
                            MapTreatment = parts[8],
                            PredictionMatch = parts[9],          // keep as string "True"/"False"
                            ImagesFoundInRange = parts[10],      // keep as string "True"/"False"
                            NumImagesInRange = int.TryParse(parts[11], out var n) ? n : 0
                        };

                        records.Add(record); // <-- only add once
                    }
                }

                Console.WriteLine($"Loaded {records.Count} CSV records from {csvPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSV:\n{ex.Message}");
                Console.WriteLine($"Error loading CSV: {ex}");
            }
        }

        public void SaveCsv()
        {
            if (string.IsNullOrEmpty(loadedCsvPath))
            {
                Console.WriteLine("No CSV path loaded — nothing to save.");
                return;
            }

            using (var writer = new StreamWriter(loadedCsvPath, false, Encoding.UTF8))
            {
                // Write header exactly as original
                writer.WriteLine("RID\tSU\tWE\tFilename1\tROW_folder\tMinOfChFrom\tMaxOfChTo\tSurfaceType\tMapTreatment\tprediction_match\timages_found_in_range\tnum_images_in_range");

                foreach (var record in records)
                {
                    writer.WriteLine($"{record.RID}\t{record.SU}\t{record.WE}\t{record.Filename1}\t{record.ROW_folder}\t" +
                                     $"{record.MinOfChFrom.ToString("0.##", CultureInfo.InvariantCulture)}\t" +
                                     $"{record.MaxOfChTo.ToString("0.##", CultureInfo.InvariantCulture)}\t" +
                                     $"{record.SurfaceType}\t{record.MapTreatment}\t" +
                                     $"{NormalizeBool(record.PredictionMatch)}\t{NormalizeBool(record.ImagesFoundInRange)}\t" +
                                     $"{record.NumImagesInRange}");
                }
            }

            Console.WriteLine($"Saved {records.Count} CSV records back to {loadedCsvPath}");
        }

        private string NormalizeBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "False";
            return (value.Trim().Equals("True", StringComparison.OrdinalIgnoreCase)) ? "True" : "False";
        }

        public List<CsvRecord> FindMatches(string imageFile)
        {
            double? distance = GetDistanceFromFilename(imageFile);

            if (distance == null)
                return new List<CsvRecord>();

            return records
                .Where(r => !string.IsNullOrEmpty(r.ROW_folder)
                            && imageFile.StartsWith(r.ROW_folder, StringComparison.OrdinalIgnoreCase)
                            && distance >= r.MinOfChFrom
                            && distance <= r.MaxOfChTo)
                .ToList();
        }

        private double? GetDistanceFromFilename(string imageFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(imageFile);
            // Expecting "N51D224A    0.010 1"
            var parts = fileName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double distance))
                return distance;

            return null;
        }

    }
}
