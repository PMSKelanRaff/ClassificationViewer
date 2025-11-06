namespace ClassificationViewer.Forms
{
    partial class FolderSelectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            comboBoxFolders = new ComboBox();
            btnOkay = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // comboBoxFolders
            // 
            comboBoxFolders.FormattingEnabled = true;
            comboBoxFolders.Location = new Point(12, 43);
            comboBoxFolders.Name = "comboBoxFolders";
            comboBoxFolders.Size = new Size(220, 23);
            comboBoxFolders.TabIndex = 0;
            // 
            // btnOkay
            // 
            btnOkay.Location = new Point(157, 72);
            btnOkay.Name = "btnOkay";
            btnOkay.Size = new Size(75, 23);
            btnOkay.TabIndex = 1;
            btnOkay.Text = "Select";
            btnOkay.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(76, 72);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // FolderSelectForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(292, 180);
            Controls.Add(btnCancel);
            Controls.Add(btnOkay);
            Controls.Add(comboBoxFolders);
            Name = "FolderSelectForm";
            Text = "FolderSelectForm";
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBoxFolders;
        private Button btnOkay;
        private Button btnCancel;
    }
}