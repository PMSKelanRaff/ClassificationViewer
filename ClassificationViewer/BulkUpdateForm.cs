using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassificationViewer
{
    public partial class BulkUpdateForm : Form
    {
        public BulkUpdateForm(string[] surfaceOptions, List<CsvRecord> fileRecords, double blockStart, double blockEnd)
        {
            InitializeComponent();

            // Populate SurfaceType and MapTreatment choices
            comboSurfaceType.Items.AddRange(surfaceOptions);
            comboMapTreatment.Items.AddRange(surfaceOptions);

            // Populate start/end ComboBoxes with distances
            foreach (var r in fileRecords)
            {
                string label = $"{r.MinOfChFrom:0.00} → {r.MaxOfChTo:0.00}";
                comboStart.Items.Add(new DistanceOption(label, r.MinOfChFrom));
                comboEnd.Items.Add(new DistanceOption(label, r.MaxOfChTo));
            }

            // Preselect the block in start/end ComboBoxes
            PreselectRange(blockStart, blockEnd);

            // Prepopulate SurfaceType and MapTreatment based on the first record in the block
            var firstInBlock = fileRecords.FirstOrDefault(r => Math.Abs(r.MinOfChFrom - blockStart) < 0.001);
            if (firstInBlock != null)
            {
                if (surfaceOptions.Contains(firstInBlock.SurfaceType))
                    comboSurfaceType.SelectedItem = firstInBlock.SurfaceType;

                if (surfaceOptions.Contains(firstInBlock.MapTreatment))
                    comboMapTreatment.SelectedItem = firstInBlock.MapTreatment;
            }
        }

        public double StartDistance { get; private set; }
        public double EndDistance { get; private set; }
        public string SelectedSurfaceType { get; private set; }
        public string SelectedMapTreatment { get; private set; }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (comboStart.SelectedItem is not DistanceOption startOpt ||
                comboEnd.SelectedItem is not DistanceOption endOpt)
            {
                MessageBox.Show("Please select both a start and end distance.");
                return;
            }

            StartDistance = startOpt.Value;
            EndDistance = endOpt.Value;
            SelectedSurfaceType = comboSurfaceType.SelectedItem?.ToString();
            SelectedMapTreatment = comboMapTreatment.SelectedItem?.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }

        public void PreselectRange(double start, double end)
        {
            foreach (var item in comboStart.Items)
            {
                if (item is DistanceOption opt && Math.Abs(opt.Value - start) < 0.001)
                {
                    comboStart.SelectedItem = item;
                    break;
                }
            }

            foreach (var item in comboEnd.Items)
            {
                if (item is DistanceOption opt && Math.Abs(opt.Value - end) < 0.001)
                {
                    comboEnd.SelectedItem = item;
                    break;
                }
            }
        }

        public static List<(double Start, double End, string Surface, string Treatment)> GetStrictBlocks(List<CsvRecord> records)
        {
            var blocks = new List<(double Start, double End, string Surface, string Treatment)>();
            if (!records.Any()) return blocks;

            double blockStart = records[0].MinOfChFrom;
            double blockEnd = records[0].MaxOfChTo;
            string surface = records[0].SurfaceType;
            string treatment = records[0].MapTreatment;

            foreach (var r in records.Skip(1))
            {
                if (r.SurfaceType == surface && r.MapTreatment == treatment)
                {
                    blockEnd = r.MaxOfChTo;
                }
                else
                {
                    blocks.Add((blockStart, blockEnd, surface, treatment));
                    blockStart = r.MinOfChFrom;
                    blockEnd = r.MaxOfChTo;
                    surface = r.SurfaceType;
                    treatment = r.MapTreatment;
                }
            }

            blocks.Add((blockStart, blockEnd, surface, treatment));
            return blocks;
        }

        private class DistanceOption
        {
            public string Label { get; }
            public double Value { get; }

            public DistanceOption(string label, double value)
            {
                Label = label;
                Value = value;
            }

            public override string ToString()
            {
                return Label; // this will be shown in the ComboBox
            }
        }
    }
}
