using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq; // <-- Make sure Linq is included
using System.Text;
using System.Windows.Forms;

namespace ClassificationViewer.Classes
{
    public class CsvHelperClass
    {
        private List<CsvRecord> records = new List<CsvRecord>();
        private string loadedCsvPath;

        public IReadOnlyList<CsvRecord> Records => records;

        // ✅ MODIFIED: Added 'folderToLoad' parameter
        public void LoadCsv(string csvPath, string folderToLoad = null)
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
                            SecondMapTreatment = parts.Length > 12 ? parts[12] : "None"
                        };

                        // If a folder is specified, only add records that match it.
                        if (folderToLoad != null)
                        {
                            if (!string.IsNullOrEmpty(record.ROW_folder) &&
                                string.Equals(record.ROW_folder.TrimEnd('\\'), folderToLoad.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                            {
                                records.Add(record); // Add only if it matches
                            }
                        }
                        else
                        {
                            records.Add(record); // Otherwise, add all (for the initial load in ClassificationViewerForm)
                        }
                    }
                }

                Console.WriteLine($"Loaded {records.Count} CSV records from {csvPath}" + (folderToLoad != null ? $" for folder {Path.GetFileName(folderToLoad)}" : ""));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSV:\n{ex.Message}");
                Console.WriteLine($"Error loading CSV: {ex}");
            }
        }

        
        public void SaveCsv(bool saveAsUpdatedFile = false)
        {
            if (string.IsNullOrEmpty(loadedCsvPath))
            {
                MessageBox.Show("Cannot save: No CSV path loaded.");
                return;
            }
            if (!records.Any())
            {
                Console.WriteLine("No records in memory to save.");
                return; // Nothing to save
            }

            // All records in memory *should* be for the same folder now.
            string currentDatasetFolder = records.First().ROW_folder;
            if (string.IsNullOrEmpty(currentDatasetFolder))
            {
                MessageBox.Show("Save failed: Records in memory have no ROW_folder assigned.");
                return; // Can't determine unique filename
            }

            // Create output directory
            string outputDir = Path.Combine(Path.GetDirectoryName(loadedCsvPath), "Updated");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string outputPath;
            if (saveAsUpdatedFile)
            {
                // New logic: Save a separate file for this dataset (folder)
                string originalFileName = Path.GetFileNameWithoutExtension(loadedCsvPath);
                string folderName = new DirectoryInfo(currentDatasetFolder).Name; // Gets the "N51D224A_ROW" part
                string newFileName = $"{originalFileName}_{folderName}.csv"; // e.g., "N53D2ML_N51D224A_ROW.csv"
                outputPath = Path.Combine(outputDir, newFileName);
            }
            else
            {
                // Fallback to overwriting original (less common)
                outputPath = loadedCsvPath;
            }

            // Get the header from the *original* file
            string header;
            try
            {
                using (var reader = new StreamReader(loadedCsvPath, Encoding.UTF8, true))
                {
                    header = reader.ReadLine() ?? "";
                }
                if (string.IsNullOrEmpty(header))
                {
                    Console.WriteLine("Save failed: Could not read header from original file.");
                    MessageBox.Show("Save failed: Could not read header from original file.");
                    return;
                }

                // Fix header if SecondMapTreatment is missing (optional but robust)
                var headerParts = header.Split(',').ToList();
                if (headerParts.Count == 12)
                {
                    header += ",SecondMapTreatment";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading header from original CSV:\n{ex.Message}");
                return;
            }

            // Write the new file with the header + *only* our in-memory records
            try
            {
                using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8)) // false = overwrite
                {
                    writer.WriteLine(header);
                    foreach (var record in records)
                    {
                        // Must match the exact order from LoadCsv
                        string[] parts = new string[13];
                        parts[0] = record.RID;
                        parts[1] = record.SU;
                        parts[2] = record.WE;
                        parts[3] = record.Filename1;
                        parts[4] = record.ROW_folder;
                        parts[5] = record.MinOfChFrom.ToString(CultureInfo.InvariantCulture);
                        parts[6] = record.MaxOfChTo.ToString(CultureInfo.InvariantCulture);
                        parts[7] = record.SurfaceType;
                        parts[8] = record.MapTreatment;
                        parts[9] = record.PredictionMatch;
                        parts[10] = record.ImagesFoundInRange;
                        parts[11] = record.NumImagesInRange.ToString();
                        parts[12] = record.SecondMapTreatment ?? "None";

                        writer.WriteLine(string.Join(",", parts));
                    }
                }
                Console.WriteLine($"Saved {records.Count} records to {outputPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving new CSV file:\n{ex.Message}");
                Console.WriteLine($"Error saving CSV: {ex}");
            }
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



        //bulk updating logic
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




    }
}
