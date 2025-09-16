namespace ClassificationViewer
{
    partial class ClassificationViewerForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pictureBox1 = new PictureBox();
            btnPrevious = new Button();
            btnNext = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog2 = new FolderBrowserDialog();
            openFileDialog1 = new OpenFileDialog();
            btnSelectFolder = new Button();
            btnSelectCSV = new Button();
            btnBigNext = new Button();
            btnBigPrevious = new Button();
            lbl_Model = new Label();
            lbl_Manual = new Label();
            comboMapTreatment = new ComboBox();
            comboSurfaceType = new ComboBox();
            btnSaveChanges = new Button();
            btnBulkUpdate = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            button1 = new Button();
            button2 = new Button();
            btnNextBlock = new Button();
            btnPreviousBlock = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ControlDark;
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1033, 548);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // btnPrevious
            // 
            btnPrevious.Location = new Point(808, 566);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(75, 23);
            btnPrevious.TabIndex = 1;
            btnPrevious.Text = "<";
            btnPrevious.UseVisualStyleBackColor = true;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnNext
            // 
            btnNext.Location = new Point(889, 566);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(75, 23);
            btnNext.TabIndex = 2;
            btnNext.Text = ">";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnSelectFolder
            // 
            btnSelectFolder.Location = new Point(12, 567);
            btnSelectFolder.Name = "btnSelectFolder";
            btnSelectFolder.Size = new Size(91, 23);
            btnSelectFolder.TabIndex = 3;
            btnSelectFolder.Text = "Select Folder";
            btnSelectFolder.UseVisualStyleBackColor = true;
            btnSelectFolder.Click += btnSelectFolder_Click;
            // 
            // btnSelectCSV
            // 
            btnSelectCSV.Location = new Point(109, 567);
            btnSelectCSV.Name = "btnSelectCSV";
            btnSelectCSV.Size = new Size(75, 23);
            btnSelectCSV.TabIndex = 4;
            btnSelectCSV.Text = "Select CSV";
            btnSelectCSV.UseVisualStyleBackColor = true;
            btnSelectCSV.Click += btnSelectCsv_Click;
            // 
            // btnBigNext
            // 
            btnBigNext.Location = new Point(970, 566);
            btnBigNext.Name = "btnBigNext";
            btnBigNext.Size = new Size(75, 23);
            btnBigNext.TabIndex = 5;
            btnBigNext.Text = ">>";
            btnBigNext.UseVisualStyleBackColor = true;
            btnBigNext.Click += btnBigNext_Click;
            // 
            // btnBigPrevious
            // 
            btnBigPrevious.Location = new Point(727, 566);
            btnBigPrevious.Name = "btnBigPrevious";
            btnBigPrevious.Size = new Size(75, 23);
            btnBigPrevious.TabIndex = 6;
            btnBigPrevious.Text = "<<";
            btnBigPrevious.UseVisualStyleBackColor = true;
            btnBigPrevious.Click += btnBigPrevious_Click;
            // 
            // lbl_Model
            // 
            lbl_Model.AutoSize = true;
            lbl_Model.Location = new Point(190, 566);
            lbl_Model.Name = "lbl_Model";
            lbl_Model.Size = new Size(84, 15);
            lbl_Model.TabIndex = 8;
            lbl_Model.Text = "Model Rating :";
            // 
            // lbl_Manual
            // 
            lbl_Manual.AutoSize = true;
            lbl_Manual.Location = new Point(350, 567);
            lbl_Manual.Name = "lbl_Manual";
            lbl_Manual.Size = new Size(93, 15);
            lbl_Manual.TabIndex = 9;
            lbl_Manual.Text = "Manual Rating : ";
            // 
            // comboMapTreatment
            // 
            comboMapTreatment.FormattingEnabled = true;
            comboMapTreatment.Items.AddRange(new object[] { "HRA", "SMA", "SD", "HFS", "Concrete", "Unknown", "Ramps" });
            comboMapTreatment.Location = new Point(439, 566);
            comboMapTreatment.Name = "comboMapTreatment";
            comboMapTreatment.Size = new Size(74, 23);
            comboMapTreatment.TabIndex = 10;
            comboMapTreatment.SelectedIndexChanged += comboMapTreatment_SelectedIndexChanged;
            // 
            // comboSurfaceType
            // 
            comboSurfaceType.FormattingEnabled = true;
            comboSurfaceType.Items.AddRange(new object[] { "HRA", "SMA", "SD", "HFS", "Concrete", "Unknown", "Ramps" });
            comboSurfaceType.Location = new Point(280, 567);
            comboSurfaceType.Name = "comboSurfaceType";
            comboSurfaceType.Size = new Size(64, 23);
            comboSurfaceType.TabIndex = 11;
            comboSurfaceType.SelectedIndexChanged += comboSurfaceType_SelectedIndexChanged;
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Location = new Point(633, 566);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(88, 23);
            btnSaveChanges.TabIndex = 12;
            btnSaveChanges.Text = "Save";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // btnBulkUpdate
            // 
            btnBulkUpdate.Location = new Point(519, 566);
            btnBulkUpdate.Name = "btnBulkUpdate";
            btnBulkUpdate.Size = new Size(86, 23);
            btnBulkUpdate.TabIndex = 13;
            btnBulkUpdate.Text = "Block Update";
            btnBulkUpdate.UseVisualStyleBackColor = true;
            btnBulkUpdate.Click += btnBulkUpdate_Click;
            // 
            // timer1
            // 
            timer1.Tick += TestTimerTick;
            // 
            // button1
            // 
            button1.Location = new Point(889, 595);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 14;
            button1.Text = "Play";
            button1.UseVisualStyleBackColor = true;
            button1.Click += play_Click;
            // 
            // button2
            // 
            button2.Location = new Point(808, 595);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 15;
            button2.Text = "Pause";
            button2.UseVisualStyleBackColor = true;
            button2.Click += pause_Click;
            // 
            // btnNextBlock
            // 
            btnNextBlock.Location = new Point(970, 595);
            btnNextBlock.Name = "btnNextBlock";
            btnNextBlock.Size = new Size(75, 23);
            btnNextBlock.TabIndex = 16;
            btnNextBlock.Text = "Next Block";
            btnNextBlock.UseVisualStyleBackColor = true;
            btnNextBlock.Click += btnNextBlock_Click;
            // 
            // btnPreviousBlock
            // 
            btnPreviousBlock.Location = new Point(727, 595);
            btnPreviousBlock.Name = "btnPreviousBlock";
            btnPreviousBlock.Size = new Size(75, 23);
            btnPreviousBlock.TabIndex = 17;
            btnPreviousBlock.Text = "Prev Block";
            btnPreviousBlock.UseVisualStyleBackColor = true;
            btnPreviousBlock.Click += btnPreviousBlock_Click_1;
            // 
            // ClassificationViewerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDarkDark;
            ClientSize = new Size(1057, 622);
            Controls.Add(btnPreviousBlock);
            Controls.Add(btnNextBlock);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(btnBulkUpdate);
            Controls.Add(btnSaveChanges);
            Controls.Add(comboSurfaceType);
            Controls.Add(comboMapTreatment);
            Controls.Add(lbl_Manual);
            Controls.Add(lbl_Model);
            Controls.Add(btnBigPrevious);
            Controls.Add(btnBigNext);
            Controls.Add(btnSelectCSV);
            Controls.Add(btnSelectFolder);
            Controls.Add(btnNext);
            Controls.Add(btnPrevious);
            Controls.Add(pictureBox1);
            Name = "ClassificationViewerForm";
            Text = "Classification Viewer Form";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnPrevious;
        private Button btnNext;
        private FolderBrowserDialog folderBrowserDialog1;
        private FolderBrowserDialog folderBrowserDialog2;
        private OpenFileDialog openFileDialog1;
        private Button btnSelectFolder;
        private Button btnSelectCSV;
        private Button btnBigNext;
        private Button btnBigPrevious;
        private Label lbl_Model;
        private Label lbl_Manual;
        private ComboBox comboMapTreatment;
        private ComboBox comboSurfaceType;
        private Button btnSaveChanges;
        private Button btnBulkUpdate;
        private System.Windows.Forms.Timer timer1;
        private Button button1;
        private Button button2;
        private Button btnNextBlock;
        private Button btnPreviousBlock;
    }
}
