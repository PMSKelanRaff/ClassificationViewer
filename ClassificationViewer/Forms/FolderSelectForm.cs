using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassificationViewer.Forms
{
    public partial class FolderSelectForm : Form
    {
        public string SelectedFolder { get; private set; }

        public FolderSelectForm(List<string> folders)
        {
            InitializeComponent();
            comboBoxFolders.DataSource = folders.OrderBy(f => f).ToList();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectedFolder = comboBoxFolders.SelectedItem?.ToString();
            if (SelectedFolder == null)
            {
                MessageBox.Show("Please select a folder.");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
