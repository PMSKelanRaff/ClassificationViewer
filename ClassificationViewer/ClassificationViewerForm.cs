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


        public ClassificationViewerForm()
        {
            InitializeComponent();
        }

        //Buttons
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
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

        private void btnBulkUpdate_Click(object sender, EventArgs e)
        {
            if (currentMatch == null)
            {
                MessageBox.Show("No CSV match found for the current image.");
                return;
            }

            // Get all records for the same filename
            var fileRecords = csvHelper.Records
                .Where(r => r.Filename1 == currentMatch.Filename1)
                .OrderBy(r => r.MinOfChFrom) // sort so distances are in order
                .ToList();

            using (var form = new BulkUpdateForm(surfaceOptions, fileRecords))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    double start = form.StartDistance;
                    double end = form.EndDistance;
                    string surfaceType = form.SelectedSurfaceType;
                    string mapTreatment = form.SelectedMapTreatment;

                    if (!string.IsNullOrEmpty(surfaceType))
                        csvHelper.BulkUpdateSurfaceType(start, end, surfaceType);

                    if (!string.IsNullOrEmpty(mapTreatment))
                        csvHelper.BulkUpdateMapTreatment(start, end, mapTreatment);

                    MessageBox.Show("Bulk update applied successfully!", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

    }
}
