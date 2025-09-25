namespace ClassificationViewer
{
    partial class BulkUpdateForm
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
            lblStart = new Label();
            lblEnd = new Label();
            label3 = new Label();
            comboSurfaceType = new ComboBox();
            comboMapTreatment = new ComboBox();
            lblSurface = new Label();
            lblMap = new Label();
            btn_Apply = new Button();
            btn_Cancel = new Button();
            comboStart = new ComboBox();
            comboEnd = new ComboBox();
            comboSecondMapTreatment = new ComboBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // lblStart
            // 
            lblStart.AutoSize = true;
            lblStart.Location = new Point(15, 50);
            lblStart.Name = "lblStart";
            lblStart.Size = new Size(41, 15);
            lblStart.TabIndex = 0;
            lblStart.Text = "From :";
            // 
            // lblEnd
            // 
            lblEnd.AutoSize = true;
            lblEnd.Location = new Point(15, 88);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new Size(26, 15);
            lblEnd.TabIndex = 1;
            lblEnd.Text = "To :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label3.Location = new Point(162, 9);
            label3.Name = "label3";
            label3.Size = new Size(89, 21);
            label3.TabIndex = 4;
            label3.Text = "Change All";
            // 
            // comboSurfaceType
            // 
            comboSurfaceType.FormattingEnabled = true;
            comboSurfaceType.Items.AddRange(new object[] { "HRA", "SMA", "SD", "HFS", "Concrete", "Unknown", "Ramps" });
            comboSurfaceType.Location = new Point(289, 44);
            comboSurfaceType.Name = "comboSurfaceType";
            comboSurfaceType.Size = new Size(121, 23);
            comboSurfaceType.TabIndex = 5;
            // 
            // comboMapTreatment
            // 
            comboMapTreatment.FormattingEnabled = true;
            comboMapTreatment.Items.AddRange(new object[] { "HRA", "SMA", "SD", "HFS", "Concrete", "Unknown", "Ramps" });
            comboMapTreatment.Location = new Point(289, 77);
            comboMapTreatment.Name = "comboMapTreatment";
            comboMapTreatment.Size = new Size(121, 23);
            comboMapTreatment.TabIndex = 6;
            // 
            // lblSurface
            // 
            lblSurface.AutoSize = true;
            lblSurface.Location = new Point(227, 47);
            lblSurface.Name = "lblSurface";
            lblSurface.Size = new Size(47, 15);
            lblSurface.TabIndex = 7;
            lblSurface.Text = "Model :";
            // 
            // lblMap
            // 
            lblMap.AutoSize = true;
            lblMap.Location = new Point(227, 80);
            lblMap.Name = "lblMap";
            lblMap.Size = new Size(56, 15);
            lblMap.TabIndex = 8;
            lblMap.Text = "Manual : ";
            // 
            // btn_Apply
            // 
            btn_Apply.Location = new Point(335, 141);
            btn_Apply.Name = "btn_Apply";
            btn_Apply.Size = new Size(75, 23);
            btn_Apply.TabIndex = 9;
            btn_Apply.Text = "Apply Changes";
            btn_Apply.UseVisualStyleBackColor = true;
            btn_Apply.Click += btnApply_Click;
            // 
            // btn_Cancel
            // 
            btn_Cancel.Location = new Point(254, 141);
            btn_Cancel.Name = "btn_Cancel";
            btn_Cancel.Size = new Size(75, 23);
            btn_Cancel.TabIndex = 10;
            btn_Cancel.Text = "Cancel";
            btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // comboStart
            // 
            comboStart.FormattingEnabled = true;
            comboStart.Location = new Point(62, 47);
            comboStart.Name = "comboStart";
            comboStart.Size = new Size(121, 23);
            comboStart.TabIndex = 11;
            // 
            // comboEnd
            // 
            comboEnd.FormattingEnabled = true;
            comboEnd.Location = new Point(61, 85);
            comboEnd.Name = "comboEnd";
            comboEnd.Size = new Size(121, 23);
            comboEnd.TabIndex = 12;
            // 
            // comboSecondMapTreatment
            // 
            comboSecondMapTreatment.FormattingEnabled = true;
            comboSecondMapTreatment.Items.AddRange(new object[] { "HRA", "SMA", "SD", "HFS", "Concrete", "Unknown", "Ramps" });
            comboSecondMapTreatment.Location = new Point(289, 112);
            comboSecondMapTreatment.Name = "comboSecondMapTreatment";
            comboSecondMapTreatment.Size = new Size(121, 23);
            comboSecondMapTreatment.TabIndex = 13;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(218, 115);
            label1.Name = "label1";
            label1.Size = new Size(65, 15);
            label1.TabIndex = 14;
            label1.Text = "Manual 2 : ";
            // 
            // BulkUpdateForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(430, 177);
            Controls.Add(label1);
            Controls.Add(comboSecondMapTreatment);
            Controls.Add(comboEnd);
            Controls.Add(comboStart);
            Controls.Add(btn_Cancel);
            Controls.Add(btn_Apply);
            Controls.Add(lblMap);
            Controls.Add(lblSurface);
            Controls.Add(comboMapTreatment);
            Controls.Add(comboSurfaceType);
            Controls.Add(label3);
            Controls.Add(lblEnd);
            Controls.Add(lblStart);
            Name = "BulkUpdateForm";
            Text = "BulkUpdateForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblStart;
        private Label lblEnd;
        private Label label3;
        private ComboBox comboSurfaceType;
        private ComboBox comboMapTreatment;
        private Label lblSurface;
        private Label lblMap;
        private Button btn_Apply;
        private Button btn_Cancel;
        private ComboBox comboStart;
        private ComboBox comboEnd;
        private ComboBox comboSecondMapTreatment;
        private Label label1;
    }
}