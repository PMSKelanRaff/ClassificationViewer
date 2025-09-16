using System.Globalization;
using CsvHelper;

namespace ClassificationViewer
{
    public partial class ClassificationViewerForm : Form
    {
        private List<string> imageFiles = new List<string>();
        private int currentIndex = 0;
        private CsvHelperClass csvHelper = new CsvHelperClass();

        //Matching
        private readonly string[] surfaceOptions = { "HRA", "SMA", "SD", "HFS", "Concrete", "Unknown", "Ramps" };
        private CsvRecord currentMatch;
        private bool hasUnsavedChanges = false;

        //block navigation
        private int currentBlockIndex = 0;
        private List<(double Start, double End, string Surface, string Treatment)> fileBlocks;


        public ClassificationViewerForm()
        {
            InitializeComponent();
        }

        //Buttons
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {

            using (var fbd = new FolderBrowserDialog())
            {
                // Set default path
                fbd.SelectedPath = @"\\PQ05758\2024-Backup\RSP Imagery 2024\RSP TII Network Survey Imagery 2024\WE20240608\N51D224A\N51D224A_ROW";

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    imageFiles = Directory.GetFiles(fbd.SelectedPath, "*.JPG")
                                          .Concat(Directory.GetFiles(fbd.SelectedPath, "*.png"))
                                          .ToList();

                    currentIndex = 0;

                    // Display the first image immediately
                    if (imageFiles.Count > 0)
                        DisplayImage();
                }
            }
        }

        private void btnSelectCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                ofd.InitialDirectory = @"C:\Users\KelanRafferty\Desktop\prediction_mismatch_report.csv";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    csvHelper.LoadCsv(ofd.FileName);
                    MessageBox.Show("CSV loaded successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;
            currentIndex = (currentIndex + 1) % imageFiles.Count;
            DisplayImage();
        }

        private void btnBigNext_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;
            currentIndex = (currentIndex + 100) % imageFiles.Count;
            DisplayImage();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;
            currentIndex = (currentIndex - 1 + imageFiles.Count) % imageFiles.Count;
            DisplayImage();
        }

        private void btnBigPrevious_Click(object sender, EventArgs e)
        {
            if (imageFiles.Count == 0) return;
            currentIndex = (currentIndex - 100 + imageFiles.Count) % imageFiles.Count;
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

            using (var form = new BulkUpdateForm(surfaceOptions, fileRecords, start, end))
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
            NavigateToBlock(next: true);
        }

        private void btnPreviousBlock_Click_1(object sender, EventArgs e)
        {
            NavigateToBlock(next: false);
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

                    lines = new string[]
                    {
                $"{currentMatch.Filename1} : {currentMatch.MinOfChFrom}-{currentMatch.MaxOfChTo}",
                $"Model : {surface}",
                $"Manual : {manual}"
                    };
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
                    r.PredictionMatch = (r.SurfaceType == r.MapTreatment && !string.IsNullOrEmpty(r.SurfaceType)) ? "True" : "False";
                }

                DisplayImage(); // redraw overlay
            }
        }

        private void comboMapTreatment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentMatch != null && comboMapTreatment.SelectedItem != null)
            {
                string newValue = comboMapTreatment.SelectedItem.ToString();

                // Update only the MapTreatment of all rows that match this image
                var matches = csvHelper.FindMatches(imageFiles[currentIndex]);
                foreach (var r in matches)
                {
                    r.MapTreatment = newValue;
                    // Update prediction_match if SurfaceType and MapTreatment match
                    r.PredictionMatch = (r.SurfaceType == r.MapTreatment && !string.IsNullOrEmpty(r.MapTreatment)) ? "True" : "False";
                }

                DisplayImage(); // redraw overlay
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
        private void NavigateToBlock(bool next)
        {
            if (imageFiles.Count == 0) return;

            double? currentDistance = GetDistanceFromFilename(imageFiles[currentIndex]);
            if (currentDistance == null) return;

            // Extract base filename for CSV match
            string currentFile = Path.GetFileNameWithoutExtension(imageFiles[currentIndex]).Split(' ')[0];

            var fileRecords = csvHelper.Records
                .Where(r => r.Filename1 == currentFile)
                .OrderBy(r => r.MinOfChFrom)
                .ToList();

            if (!fileRecords.Any()) return;

            var blocks = BulkUpdateForm.GetStrictBlocks(fileRecords);
            if (blocks.Count == 0) return;

            int blockIndex = blocks.FindIndex(b => currentDistance >= b.Start && currentDistance <= b.End);

            if (blockIndex == -1)
            {
                blockIndex = next ? -1 : blocks.Count;
            }

            int targetIndex = next ? blockIndex + 1 : blockIndex - 1;

            if (targetIndex < 0 || targetIndex >= blocks.Count)
            {
                MessageBox.Show(next ? "No next block found." : "No previous block found.");
                return;
            }

            var targetBlock = blocks[targetIndex];

            double tolerance = 0.0001;
            var nextIndex = imageFiles.FindIndex(f =>
            {
                double? dist = GetDistanceFromFilename(f);
                return dist != null && dist + tolerance >= targetBlock.Start && dist - tolerance <= targetBlock.End;
            });

            if (nextIndex >= 0)
            {
                currentIndex = nextIndex;
                // Update currentMatch if CSV exists
                currentMatch = fileRecords.FirstOrDefault(r =>
                    Math.Abs(r.MinOfChFrom - targetBlock.Start) < 0.001);
                DisplayImage(); // refresh PictureBox
            }
            else
            {
                MessageBox.Show("No image found for the target block.");
            }
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

    }
}
