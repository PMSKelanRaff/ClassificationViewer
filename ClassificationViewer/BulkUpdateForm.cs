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
        public BulkUpdateForm(string[] surfaceOptions, List<CsvRecord> fileRecords)
        {
            InitializeComponent();

            // Populate the surface/map treatment choices
            comboSurfaceType.Items.AddRange(surfaceOptions);
            comboMapTreatment.Items.AddRange(surfaceOptions);

            // Populate start/end distances from this file’s records
            foreach (var r in fileRecords)
            {
                string label = $"{r.MinOfChFrom:0.00} → {r.MaxOfChTo:0.00}";
                comboStart.Items.Add(new DistanceOption(label, r.MinOfChFrom));
                comboEnd.Items.Add(new DistanceOption(label, r.MaxOfChTo));
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

        private class DistanceOption
        {
            public string Label { get; }
            public double Value { get; }

            public DistanceOption(string label, double value)
            {
                Label = label;
                Value = value;
            }

            public override string ToString() => Label;
        }
    }
}
