
namespace Simple_MT940_Checker
{
    partial class View
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
            if (disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(View));
            this.button_OpenFile = new System.Windows.Forms.Button();
            this.label_OpenFile = new System.Windows.Forms.Label();
            this.dataGridView_ValidationResults = new System.Windows.Forms.DataGridView();
            this.comboBox_encoding = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ValidationResults)).BeginInit();
            this.SuspendLayout();
            // 
            // button_OpenFile
            // 
            this.button_OpenFile.Location = new System.Drawing.Point(256, 82);
            this.button_OpenFile.Name = "button_OpenFile";
            this.button_OpenFile.Size = new System.Drawing.Size(99, 30);
            this.button_OpenFile.TabIndex = 0;
            this.button_OpenFile.Text = "Open file";
            this.button_OpenFile.UseVisualStyleBackColor = true;
            this.button_OpenFile.Click += new System.EventHandler(this.button_OpenFile_Click);
            // 
            // label_OpenFile
            // 
            this.label_OpenFile.AutoSize = true;
            this.label_OpenFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_OpenFile.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label_OpenFile.Location = new System.Drawing.Point(30, 25);
            this.label_OpenFile.Name = "label_OpenFile";
            this.label_OpenFile.Size = new System.Drawing.Size(549, 24);
            this.label_OpenFile.TabIndex = 1;
            this.label_OpenFile.Text = "Open a file by dragging it here, or using the button below.";
            // 
            // dataGridView_ValidationResults
            // 
            this.dataGridView_ValidationResults.AllowUserToAddRows = false;
            this.dataGridView_ValidationResults.AllowUserToDeleteRows = false;
            this.dataGridView_ValidationResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_ValidationResults.Location = new System.Drawing.Point(12, 187);
            this.dataGridView_ValidationResults.Name = "dataGridView_ValidationResults";
            this.dataGridView_ValidationResults.ReadOnly = true;
            this.dataGridView_ValidationResults.RowHeadersWidth = 10;
            this.dataGridView_ValidationResults.Size = new System.Drawing.Size(589, 251);
            this.dataGridView_ValidationResults.TabIndex = 2;
            this.dataGridView_ValidationResults.Visible = false;
            // 
            // comboBox_encoding
            // 
            this.comboBox_encoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_encoding.FormattingEnabled = true;
            this.comboBox_encoding.Items.AddRange(new object[] {
            "Automatic",
            "UTF8",
            "Unicode",
            "ASCII"});
            this.comboBox_encoding.Location = new System.Drawing.Point(246, 151);
            this.comboBox_encoding.Name = "comboBox_encoding";
            this.comboBox_encoding.Size = new System.Drawing.Size(121, 21);
            this.comboBox_encoding.TabIndex = 3;
            this.comboBox_encoding.SelectedValueChanged += new System.EventHandler(this.comboBox_encoding_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(185, 154);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Encoding:";
            // 
            // View
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_encoding);
            this.Controls.Add(this.dataGridView_ValidationResults);
            this.Controls.Add(this.label_OpenFile);
            this.Controls.Add(this.button_OpenFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "View";
            this.Text = "Simple MT940 Validator";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.View_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.View_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ValidationResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OpenFile;
        private System.Windows.Forms.Label label_OpenFile;
        private System.Windows.Forms.DataGridView dataGridView_ValidationResults;
        private System.Windows.Forms.ComboBox comboBox_encoding;
        private System.Windows.Forms.Label label1;
    }
}

