using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ClassificationViewer.Classes
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
                            SU = parts[1],
                            WE = parts[2],
                            Filename1 = parts[3],
                            ROW_folder = parts[4],
                            MinOfChFrom = double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var min) ? min : 0,
                            MaxOfChTo = double.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var max) ? max : 0,
                            SurfaceType = parts[7],
                            MapTreatment = parts[8],
                            PredictionMatch = parts[9],
                            ImagesFoundInRange = parts[10],
                            NumImagesInRange = int.TryParse(parts[11], out var n) ? n : 0,
                            // ✅ Load extra column if exists
                            SecondMapTreatment = parts.Length > 12 ? parts[12] : "None"
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

        public void SaveCsv(bool saveAsUpdatedFile = false)
        {
            if (string.IsNullOrEmpty(loadedCsvPath)) return;

            // Create output folder if needed
            string outputDir = Path.Combine(Path.GetDirectoryName(loadedCsvPath), "Updated");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string outputPath = saveAsUpdatedFile
                ? Path.Combine(outputDir, Path.GetFileName(loadedCsvPath))
                : loadedCsvPath;

            // Load all lines from original CSV
            var allLines = File.ReadAllLines(loadedCsvPath).ToList();
            if (allLines.Count == 0) return;

            string header = allLines[0];
            var dataLines = allLines.Skip(1).ToList();

            // Build a lookup of currently loaded records
            var updatedLookup = records
                .GroupBy(r => $"{r.ROW_folder}|{r.MinOfChFrom:0.000}|{r.MaxOfChTo:0.000}")
                .ToDictionary(g => g.Key, g => g.Last());

            // Update only matching lines
            for (int i = 0; i < dataLines.Count; i++)
            {
                var parts = dataLines[i].Split(',');
                if (parts.Length < 12) continue;

                string folder = parts[4].Trim();
                if (!double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double min)) continue;
                if (!double.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out double max)) continue;

                string key = $"{folder}|{min:0.000}|{max:0.000}";

                if (updatedLookup.TryGetValue(key, out var record))
                {
                    // Replace only the fields that have been updated
                    parts[7] = record.SurfaceType;
                    parts[8] = record.MapTreatment;
                    parts[12] = record.SecondMapTreatment ?? "None";
                    parts[9] = record.PredictionMatch;
                    parts[10] = record.ImagesFoundInRange;
                    parts[11] = record.NumImagesInRange.ToString();

                    dataLines[i] = string.Join(",", parts);
                }
            }

            // Write all lines back
            File.WriteAllLines(outputPath, new[] { header }.Concat(dataLines), Encoding.UTF8);
            Console.WriteLine($"Saved updated records to {outputPath}");
        }



        private string NormalizeBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "False";
            return value.Trim().Equals("True", StringComparison.OrdinalIgnoreCase) ? "True" : "False";
        }

        public List<CsvRecord> FindMatches(string imageFile)
        {
            double? distance = GetDistanceFromFilename(imageFile);
            if (distance == null) return new List<CsvRecord>();

            string imageFolder = Path.GetDirectoryName(imageFile) ?? "";

            return records
                .Where(r => !string.IsNullOrEmpty(r.ROW_folder)
                            && string.Equals(r.ROW_folder.TrimEnd('\\'), imageFolder.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)
                            && distance >= r.MinOfChFrom
                            && distance < r.MaxOfChTo) // Upper bound exclusive
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

        public void BulkUpdateSurfaceType(double startDistance, double endDistance, string newSurfaceType)
        {
            foreach (var r in records)
            {
                if (r.MinOfChFrom >= startDistance && r.MaxOfChTo <= endDistance)
                {
                    r.SurfaceType = newSurfaceType;
                    // Optional: update prediction match if desired
                    r.PredictionMatch = r.SurfaceType == r.MapTreatment && !string.IsNullOrEmpty(r.SurfaceType) ? "True" : "False";
                }
            }
        }

        public void BulkUpdateMapTreatment(double startDistance, double endDistance, string newMapTreatment)
        {
            foreach (var r in records)
            {
                if (r.MinOfChFrom >= startDistance && r.MaxOfChTo <= endDistance)
                {
                    r.MapTreatment = newMapTreatment;
                    // Optional: update prediction match
                    r.PredictionMatch = r.SurfaceType == r.MapTreatment && !string.IsNullOrEmpty(r.MapTreatment) ? "True" : "False";
                }
            }
        }

        public void BulkUpdateSecondMapTreatment(double startDistance, double endDistance, string newSecondTreatment)
        {
            foreach (var r in records)
            {
                // Only update records in the selected distance range
                if (r.MinOfChFrom >= startDistance && r.MaxOfChTo <= endDistance)
                {
                    r.SecondMapTreatment = newSecondTreatment;
                }
            }
        }

        //





    }
}
