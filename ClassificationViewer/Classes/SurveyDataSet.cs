using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassificationViewer.Classes
{
    internal class SurveyDataSet
    {
        // Folder containing images
        public string Folder { get; private set; }

        public string CsvPath { get; private set; }

        public List<string> ImageFiles { get; set; } = new List<string>();

        public CsvHelperClass CsvData { get; private set; } = new CsvHelperClass();

        // Optional: blocks if you have them per dataset
        //public List<Block> Blocks { get; set; } = new List<Block>();

        public SurveyDataSet(string folder, string csvPath)
        {
            Folder = folder;
            CsvPath = csvPath;

            if (!string.IsNullOrEmpty(csvPath))
                // MODIFIED LINE: Pass the folder to LoadCsv
                CsvData.LoadCsv(csvPath, Folder);
        }

        // Optional helper to clear dataset
        public void Clear()
        {
            ImageFiles.Clear();
            CsvData = new CsvHelperClass();
        }
    }
}
