using System.Globalization;
using ClassificationViewer.Classes;
using CsvHelper;

namespace ClassificationViewer
{
    public partial class ClassificationViewerForm : Form
    {
        // Current folder & image
        private List<string> imageFiles = new List<string>();
        private int currentIndex = 0;
        private CsvHelperClass csvHelper = new CsvHelperClass();

        //Matching
        private readonly string[] surfaceOptions = { "HRA", "SMA", "SD", "HFS", "Concrete", "Unknown", "Ramps" };
        private CsvRecord currentMatch;
        private bool hasUnsavedChanges = false;

        // Block navigation
        private int currentBlockIndex = 0;
        private List<(double Start, double End, string Surface, string Treatment, string SecondTreatment)> currentBlocks;


        public ClassificationViewerForm()
        {
            InitializeComponent();
            SetPredefinedFoldersAndCsvs();
            InitializeDataSets();
        }

        //Buttons
        private void btnSelectCsvAndFolder_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                ofd.InitialDirectory = @"C:\Users\KelanRafferty\Desktop";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                // Load CSV
                csvHelper.LoadCsv(ofd.FileName);
                MessageBox.Show("CSV loaded successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Group CSV records by folder
                var recordsByFolder = csvHelper.Records
                    .Where(r => !string.IsNullOrWhiteSpace(r.ROW_folder))
                    .GroupBy(r => r.ROW_folder)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                // Clear current images/blocks
                imageFiles = new List<string>();
                currentBlocks.Clear();
                currentBlockIndex = 0;

                // Load images from each folder
                foreach (var kvp in recordsByFolder)
                {
                    string folderPath = kvp.Key;
                    var records = kvp.Value;

                    if (!Directory.Exists(folderPath))
                    {
                        MessageBox.Show($"Folder does not exist:\n{folderPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    var folderImages = Directory.GetFiles(folderPath, "*.JPG")
                        .Concat(Directory.GetFiles(folderPath, "*.PNG"))
                        .OrderBy(f =>
                        {
                            double? d = GetDistanceFromFilename(f);
                            return d ?? double.MaxValue;
                        })
                        .ToList();

                    imageFiles.AddRange(folderImages);

                    // Ensure records are ordered by chainage (lowest to highest)
                    records = records.OrderBy(r => r.MinOfChFrom).ToList();

                    // Create merged logical blocks for this folder
                    var folderBlocks = BulkUpdateForm.GetStrictBlocks(records);
                    currentBlocks.AddRange(folderBlocks); // assuming Block can wrap a CsvRecord
                }

                if (imageFiles.Count == 0)
                {
                    MessageBox.Show("No images found in any CSV folder.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                currentIndex = 0;
                DisplayImage();

                // Move to the first block immediately
                currentBlockIndex = 0;
                MoveToBlock(currentBlockIndex);

                hasUnsavedChanges = false;
                btnSaveChanges.Enabled = true;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;

            currentIndex++;

            if (currentIndex >= imageFiles.Count)
            {
                // End of current dataset
                if (currentDataSetIndex + 1 < surveyDataSets.Count)
                {
                    var result = MessageBox.Show(
                        "End of dataset reached. Load next dataset?",
                        "Next Dataset",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        currentDataSetIndex++;
                        ActivateDataSet(currentDataSetIndex);
                        return;
                    }
                }

                currentIndex = imageFiles.Count - 1; // stay on last image
            }

            DisplayImage();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;

            currentIndex--;

            if (currentIndex < 0)
            {
                if (currentDataSetIndex > 0)
                {
                    var result = MessageBox.Show(
                        "Beginning of dataset reached. Load previous dataset?",
                        "Previous Dataset",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        currentDataSetIndex--;
                        ActivateDataSet(currentDataSetIndex);
                        currentIndex = 0; // start at first image of previous dataset
                        return;
                    }
                }

                currentIndex = 0; // stay at first image
            }

            DisplayImage();
        }

        private void btnBigNext_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;

            currentIndex += 100;

            if (currentIndex >= imageFiles.Count)
            {
                if (currentDataSetIndex + 1 < surveyDataSets.Count)
                {
                    var result = MessageBox.Show(
                        "End of dataset reached. Load next dataset?",
                        "Next Dataset",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        currentDataSetIndex++;
                        ActivateDataSet(currentDataSetIndex);
                        return;
                    }
                }

                currentIndex = imageFiles.Count - 1;
            }

            DisplayImage();
        }

        private void btnBigPrevious_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;

            currentIndex -= 100;

            if (currentIndex < 0)
            {
                if (currentDataSetIndex > 0)
                {
                    var result = MessageBox.Show(
                        "Beginning of dataset reached. Load previous dataset?",
                        "Previous Dataset",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        currentDataSetIndex--;
                        ActivateDataSet(currentDataSetIndex);
                        currentIndex = 0;
                        return;
                    }
                }

                currentIndex = 0;
            }

            DisplayImage();
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            csvHelper.SaveCsv(true);
            hasUnsavedChanges = false;
            btnSaveChanges.Enabled = false;
            MessageBox.Show("CSV changes saved successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnBulkUpdate_Click(object sender, EventArgs e)
        {
            if (currentMatch == null)
            {
                MessageBox.Show("No CSV match found for the current image.");
                return;
            }

            var fileRecords = csvHelper.Records
                .Where(r => r.Filename1 == currentMatch.Filename1)
                .OrderBy(r => r.MinOfChFrom)
                .ToList();

            // Decide which field to use as reference (SurfaceType or MapTreatment)
            // You could even let the user pick with a toggle.
            var (start, end) = FindBlockForCurrentRecord(currentMatch);

            using (var form = new BulkUpdateForm(csvHelper, surfaceOptions, fileRecords, start, end))
            {
                // Preselect defaults in the form
                form.PreselectRange(start, end);

                if (form.ShowDialog() == DialogResult.OK)
                {
                    double blockStart = form.StartDistance;
                    double blockEnd = form.EndDistance;

                    if (!string.IsNullOrEmpty(form.SelectedSurfaceType))
                        csvHelper.BulkUpdateSurfaceType(blockStart, blockEnd, form.SelectedSurfaceType);

                    if (!string.IsNullOrEmpty(form.SelectedMapTreatment))
                        csvHelper.BulkUpdateMapTreatment(blockStart, blockEnd, form.SelectedMapTreatment);

                    hasUnsavedChanges = true;
                    btnSaveChanges.Enabled = true;
                }
            }
        }

        private void play_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void pause_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void btnNextBlock_Click(object sender, EventArgs e)
        {
            NavigateToNextBlock();
        }

        private void btnPreviousBlock_Click(object sender, EventArgs e)
        {
            NavigateToPreviousBlock();
        }

        private void BtnLoadPredefined_Click(object sender, EventArgs e)
        {
            SetPredefinedFoldersAndCsvs();
            InitializeDataSets();
        }


        //Events
        private void DisplayImage()
        {
            if (imageFiles.Count == 0) return;

            string file = imageFiles[currentIndex];

            // Dispose previous image to avoid memory leaks
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            // Load image safely
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                pictureBox1.Image = Image.FromStream(fs);
            }

            // Find the first CSV match
            var matches = csvHelper.FindMatches(file);
            currentMatch = matches.FirstOrDefault();

            // Update combobox selections if match found
            if (currentMatch != null)
            {
                comboSurfaceType.SelectedItem = surfaceOptions.Contains(currentMatch.SurfaceType)
                    ? currentMatch.SurfaceType
                    : "Unknown";

                comboMapTreatment.SelectedItem = surfaceOptions.Contains(currentMatch.MapTreatment)
                    ? currentMatch.MapTreatment
                    : "Unknown";
            }
            else
            {
                comboSurfaceType.SelectedItem = null;
                comboMapTreatment.SelectedItem = null;
            }

            // Draw overlay
            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                Font font = new Font("Arial", 10, FontStyle.Bold);
                SolidBrush textBrush = new SolidBrush(Color.Red);
                SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)); // semi-transparent black

                float x = 10;
                float y = 10;
                float lineHeight = 40;

                string[] lines;

                if (currentMatch != null)
                {
                    string surface = comboSurfaceType.SelectedItem?.ToString() ?? currentMatch.SurfaceType;
                    string manual = comboMapTreatment.SelectedItem?.ToString() ?? currentMatch.MapTreatment;
                    string extra = currentMatch?.SecondMapTreatment ?? "None";

                    lines = new string[]
                    {
                $"{currentMatch.Filename1} : {currentMatch.MinOfChFrom}-{currentMatch.MaxOfChTo}",
                $"Model : {surface}",
                $"Manual : {manual}"
                    };
                    // Add third line only if not None
                    if (!string.IsNullOrEmpty(extra) && extra != "None")
                    {
                        lines = lines.Concat(new string[] { $"Manual 2 : {extra}" }).ToArray();
                    }
                }
                else
                {
                    lines = new string[] { "No CSV match found" };
                }

                // Calculate background rectangle size
                float maxWidth = lines.Max(line => g.MeasureString(line, font).Width);
                float rectHeight = lineHeight * lines.Length + 10;

                // Draw background rectangle
                g.FillRectangle(backgroundBrush, x - 5, y - 5, maxWidth + 10, rectHeight);

                // Draw text lines
                for (int i = 0; i < lines.Length; i++)
                {
                    g.DrawString(lines[i], font, textBrush, new PointF(x, y + i * lineHeight));
                }
            }

            pictureBox1.Refresh();
        }

        private void comboSurfaceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentMatch != null && comboSurfaceType.SelectedItem != null)
            {
                string newValue = comboSurfaceType.SelectedItem.ToString();

                // Update only the SurfaceType of all rows that match this image
                var matches = csvHelper.FindMatches(imageFiles[currentIndex]);
                foreach (var r in matches)
                {
                    r.SurfaceType = newValue;

                    // Update prediction_match if SurfaceType and MapTreatment match
                    r.PredictionMatch = (r.SurfaceType == r.MapTreatment && !string.IsNullOrEmpty(r.SurfaceType))
                        ? "True" : "False";
                }

                // Mark changes so Save button works again
                hasUnsavedChanges = true;
                btnSaveChanges.Enabled = true;

                DisplayImage(); // redraw overlay
            }
        }

        private void comboMapTreatment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentMatch != null && comboMapTreatment.SelectedItem != null)
            {
                string newValue = comboMapTreatment.SelectedItem.ToString();

                var matches = csvHelper.FindMatches(imageFiles[currentIndex]);
                foreach (var r in matches)
                {
                    r.MapTreatment = newValue;

                    // Update prediction_match if SurfaceType and MapTreatment match
                    r.PredictionMatch = (r.SurfaceType == r.MapTreatment && !string.IsNullOrEmpty(r.SurfaceType))
                        ? "True" : "False";
                }

                // Mark changes so Save button works again
                hasUnsavedChanges = true;
                btnSaveChanges.Enabled = true;

                DisplayImage();
            }
        }

        private void comboSecondMapTreatment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentMatch != null && comboSecondMapTreatment.SelectedItem != null)
            {
                string newValue = comboSecondMapTreatment.SelectedItem.ToString();

                var matches = csvHelper.FindMatches(imageFiles[currentIndex]);
                foreach (var r in matches)
                {
                    r.SecondMapTreatment = newValue;
                }

                hasUnsavedChanges = true;
                btnSaveChanges.Enabled = true;
                DisplayImage();
            }
        }

        private void TestTimerTick(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;
            currentIndex = (currentIndex + 1) % imageFiles.Count;
            DisplayImage();
        }

        private (double Start, double End) FindBlockForCurrentRecord(CsvRecord current)
        {
            if (current == null) return (0, 0);

            // Get all records for the same file, ordered by MinOfChFrom
            var fileRecords = csvHelper.Records
                .Where(r => r.Filename1 == current.Filename1)
                .OrderBy(r => r.MinOfChFrom)
                .ToList();

            if (!fileRecords.Any()) return (current.MinOfChFrom, current.MaxOfChTo);

            // Initialize current block
            double blockStart = fileRecords[0].MinOfChFrom;
            double blockEnd = fileRecords[0].MaxOfChTo;
            string currentSurface = fileRecords[0].SurfaceType;
            string currentTreatment = fileRecords[0].MapTreatment;

            foreach (var r in fileRecords.Skip(1))
            {
                // Strict: block continues only if both SurfaceType and MapTreatment match
                if (r.SurfaceType == currentSurface && r.MapTreatment == currentTreatment)
                {
                    blockEnd = r.MaxOfChTo;
                }
                else
                {
                    // If current record is within the previous block, return it
                    if (current.MinOfChFrom >= blockStart && current.MaxOfChTo <= blockEnd)
                        return (blockStart, blockEnd);

                    // Start new block
                    blockStart = r.MinOfChFrom;
                    blockEnd = r.MaxOfChTo;
                    currentSurface = r.SurfaceType;
                    currentTreatment = r.MapTreatment;
                }
            }

            // If current record is in the last block
            if (current.MinOfChFrom >= blockStart && current.MaxOfChTo <= blockEnd)
                return (blockStart, blockEnd);

            // Fallback: just return current record
            return (current.MinOfChFrom, current.MaxOfChTo);
        }


        // Core navigation logic

        private void LoadBlocksForCurrentDataset()
        {
            if (csvHelper.Records.Count == 0) return;

            // Group CSV records by folder
            var recordsByFolder = csvHelper.Records
                .Where(r => !string.IsNullOrWhiteSpace(r.ROW_folder))
                .GroupBy(r => r.ROW_folder, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.MinOfChFrom).ToList());

            currentBlocks = new List<(double Start, double End, string Surface, string Treatment, string SecondTreatment)>();

            // Iterate through each folder's records
            foreach (var kvp in recordsByFolder)
            {
                var folderRecords = kvp.Value;
                var folderBlocks = BulkUpdateForm.GetStrictBlocks(folderRecords); // returns tuples

                currentBlocks.AddRange(folderBlocks);
            }

            // Sort blocks globally by start distance
            currentBlocks = currentBlocks.OrderBy(b => b.Start).ToList();

            currentBlockIndex = 0;

            // Move to first image of first block
            if (currentBlocks.Count > 0)
            {
                var firstBlock = currentBlocks[0];
                currentIndex = imageFiles.FindIndex(f =>
                {
                    double? d = GetDistanceFromFilename(f);
                    return d != null && d >= firstBlock.Start && d <= firstBlock.End;
                });
                DisplayImage();
            }
        }


        private void NavigateToNextBlock()
        {
            if (currentBlocks == null || currentBlocks.Count == 0) return;

            currentBlockIndex++;

            if (currentBlockIndex >= currentBlocks.Count)
            {
                // Move to next dataset
                if (currentDataSetIndex + 1 < surveyDataSets.Count)
                {
                    currentDataSetIndex++;
                    ActivateDataSet(currentDataSetIndex);

                    // Start at first block of new dataset
                    currentBlockIndex = 0;
                    MoveToBlock(currentBlockIndex);
                }
                else
                {
                    // Stay on last block of last dataset
                    currentBlockIndex = currentBlocks.Count - 1;
                }

                return;
            }

            MoveToBlock(currentBlockIndex);
        }

        private void NavigateToPreviousBlock()
        {
            if (currentBlocks == null || currentBlocks.Count == 0) return;

            currentBlockIndex--;

            if (currentBlockIndex < 0)
            {
                // Move to previous dataset
                if (currentDataSetIndex > 0)
                {
                    currentDataSetIndex--;
                    ActivateDataSet(currentDataSetIndex);

                    // Start at last block of previous dataset
                    currentBlockIndex = currentBlocks.Count - 1;
                    MoveToBlock(currentBlockIndex);
                }
                else
                {
                    // Stay on first block of first dataset
                    currentBlockIndex = 0;
                }

                return;
            }

            MoveToBlock(currentBlockIndex);
        }


        private void MoveToBlock(int blockIndex)
        {
            if (currentBlocks == null || blockIndex < 0 || blockIndex >= currentBlocks.Count)
                return;

            var block = currentBlocks[blockIndex];

            // Try to find an image within the block range
            currentIndex = imageFiles.FindIndex(f =>
            {
                double? d = GetDistanceFromFilename(f);
                return d != null && d >= block.Start && d <= block.End;
            });

            if (currentIndex == -1)
            {
                currentIndex = imageFiles.FindIndex(f =>
                {
                    double? d = GetDistanceFromFilename(f);
                    return d != null && Math.Abs(d.Value - block.Start) < 0.5;
                });

                // If still not found, just stay at first image
                if (currentIndex == -1)
                    currentIndex = 0;
            }

            if (imageFiles.Count == 0 || currentIndex < 0 || currentIndex >= imageFiles.Count)
                return;

            string currentFile = Path.GetFileNameWithoutExtension(imageFiles[currentIndex]).Split(' ')[0];

            var fileRecords = csvHelper.Records
                .Where(r => r.Filename1 == currentFile)
                .OrderBy(r => r.MinOfChFrom)
                .ToList();

            currentMatch = fileRecords.FirstOrDefault(r =>
                Math.Abs(r.MinOfChFrom - block.Start) < 0.001);

            DisplayImage();
        }


        private double? GetDistanceFromFilename(string imageFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(imageFile);
            // Expecting "N51D224A    7.805 1"
            var parts = fileName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double distance))
                return distance;

            return null;
        }

        //preloading datasets
        private List<SurveyDataSet> surveyDataSets = new List<SurveyDataSet>();
        private int currentDataSetIndex = 0; // which dataset is active
        private int currentImageIndex = 0;   // which image in current dataset

        private string[] predefinedFolders;
        private string csvFolder;
        private string[] csvFiles;

        private void SetPredefinedFoldersAndCsvs()
        {
            predefinedFolders = new string[]
            {
        @"\\PQ05758\2024-Backup\RSP Imagery 2024\WE20240608\N51D224A\N51D224A_ROW",
        @"\\PQ05758\2024-Backup\RSP Imagery 2024\WE20240615\N52D124B\N52D124B_ROW",
        @"\\PQ05758\2024-Backup\RSP Imagery 2024\WE20240831\N73D224A\N73D224A_ROW",
        @"\\PQ05758\2024-Backup\RSP Imagery 2024\WE20240914\N54D124A\N54D124A_ROW",
        @"\\PQ05758\2024-Backup\RSP Imagery 2024\WE20240615\N55D224A\N55D224A_ROW"
            };

            csvFolder = @"R:\David P\Training\Kelan\Surface Classification Editor 2.0\Data\CSV";
            csvFiles = Directory.Exists(csvFolder) ? Directory.GetFiles(csvFolder, "*.csv") : Array.Empty<string>();
        }

        private void InitializeDataSets()
        {
            surveyDataSets.Clear();

            string csvFolder = @"R:\David P\Training\Kelan\Surface Classification Editor 2.0\Data\CSV";
            if (!Directory.Exists(csvFolder))
            {
                MessageBox.Show($"CSV folder not found: {csvFolder}");
                return;
            }

            string[] csvFiles = Directory.GetFiles(csvFolder, "*.csv");
            if (csvFiles.Length == 0)
            {
                MessageBox.Show("No CSV files found.");
                return;
            }

            foreach (var csvPath in csvFiles)
            {
                var csvData = new CsvHelperClass();
                csvData.LoadCsv(csvPath);

                // Get all unique folders from this CSV
                var foldersInCsv = csvData.Records
                                          .Select(r => r.ROW_folder)
                                          .Distinct()
                                          .Where(f => !string.IsNullOrEmpty(f))
                                          .ToList();

                foreach (var folder in foldersInCsv)
                {
                    if (!Directory.Exists(folder))
                    {
                        Console.WriteLine($"Skipping missing folder: {folder}");
                        continue;
                    }

                    // Get all images in this folder
                    var images = Directory.GetFiles(folder, "*.JPG")
                                          .Concat(Directory.GetFiles(folder, "*.PNG"))
                                          .OrderBy(f => f)
                                          .ToList();

                    if (images.Count == 0)
                    {
                        Console.WriteLine($"No images found in folder: {folder}");
                        continue;
                    }

                    surveyDataSets.Add(new SurveyDataSet(folder, csvPath)
                    {
                        ImageFiles = images
                    });

                    Console.WriteLine($"Loaded {images.Count} images + {csvData.Records.Count} CSV records from {Path.GetFileName(folder)}");
                }
            }

            if (surveyDataSets.Count > 0)
                ActivateDataSet(0);
            else
                MessageBox.Show("No datasets loaded!");
        }

        private void ActivateDataSet(int index)
        {
            if (index < 0 || index >= surveyDataSets.Count) return;

            // Before switching, check for unsaved changes
            if (hasUnsavedChanges && csvHelper != null)
            {
                var saveResult = MessageBox.Show(
                    "You have unsaved CSV changes. Do you want to save before loading the next dataset?",
                    "Save Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (saveResult == DialogResult.Yes)
                {
                    csvHelper.SaveCsv(true); // same as btnSaveChanges logic
                    hasUnsavedChanges = false;
                    btnSaveChanges.Enabled = false;
                    MessageBox.Show("CSV changes saved successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            currentDataSetIndex = index;
            var set = surveyDataSets[index];

            imageFiles = set.ImageFiles;
            csvHelper = set.CsvData;
            currentIndex = 0;

            LoadBlocksForCurrentDataset();

            MessageBox.Show($"Loaded dataset {index + 1}/{surveyDataSets.Count}\nFolder: {Path.GetFileName(set.Folder)}\nCSV: {Path.GetFileName(set.CsvPath)}");
        }

    }
}
