using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simple_MT940_Checker
{
    public partial class View : Form, I_MyView
    {
        static Dictionary<string, Encoding> Encodings = new Dictionary<string, Encoding> {
            { "Automatic", Encoding.Default },
            {"UTF8", Encoding.UTF8 },
            {"ASCII", Encoding.ASCII },
            {"Unicode", Encoding.Unicode }
        };

        Encoding Prefered_Encoding = Encoding.Default;
        string LastFileName = "";

        public View()
        {
            InitializeComponent();
        }


        private void Try_OpenFile(string fileName) 
        {
            try {                
                var file_extension = fileName.Split('.').Last().ToLower();

                if (!Enum.TryParse(file_extension, out Model.FileExtensions ext)) {
                    Show_ErrorMsg($"Can not open file of type '{file_extension}'.\r\n" +
                        "Only files of the following types are allowed: " +
                        string.Join(", ", Enum.GetNames(typeof(Model.FileExtensions))));
                    LastFileName = "";
                    return;
                }
                else {
                    LastFileName = fileName;
                    Model.Check_File(fileName, ext, this, Prefered_Encoding);
                }
            }
            catch (Exception ex) {
                LastFileName = "";
                ShowAndSave_ErrorMsg($"File could not be opened ({ex.Message}).", ex);
            }
        }


        public void Show_ValidationResults(List<(string transactionRef, string transactionDescr, List<string> validationErrors)> faulty_records) 
        {
            dataGridView_ValidationResults.Visible = true;
            dataGridView_ValidationResults.Rows.Clear();
            dataGridView_ValidationResults.Columns.Clear();

            var columns = dataGridView_ValidationResults.Columns;
            var rows = dataGridView_ValidationResults.Rows;

            columns.Add("Transaction reference", "Transaction reference");
            columns[0].ToolTipText = "The transaction reference. Needs to be unique.";
            columns[0].Width = 150;

            columns.Add("Description", "Description");
            columns[1].ToolTipText = "The transaction description.";
            columns[1].Width = 210;

            columns.Add("Validation errors", "Validation errors"); 
            columns[2].ToolTipText = "The reasons why this transaction is not valid.";
            columns[2].Width = 217;

            rows.Add(faulty_records.Sum(tr=>tr.validationErrors.Count));

            int rowIdx = 0;
            foreach (var record in faulty_records) {
                rows[rowIdx].Cells[0].Value = record.transactionRef;
                rows[rowIdx].Cells[1].Value = record.transactionDescr;
                foreach(var error in record.validationErrors)
                    rows[rowIdx++].Cells[2].Value = error;
            }
            
            this.dataGridView_ValidationResults.Update();
        }



        #region EventHandlers

        private void View_DragDrop(object sender, DragEventArgs e)
        {
            try {
                var fileName = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                Try_OpenFile(fileName);
            }
            catch (Exception ex) {
                ShowAndSave_ErrorMsg($"An unexpected error occured ({ex.Message}).", ex);            
            }
        }

        private void View_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Copy;
            }
            else {
                e.Effect = DragDropEffects.None;
            }
        }


        private void button_OpenFile_Click(object sender, EventArgs e)
        {
            try { 
                var openFileDialog1 = new OpenFileDialog();

                openFileDialog1.Filter = "CSV files (*.csv)|*.csv|XML files (*.xml)|*.xml";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                    if (openFileDialog1.FileName != null) {
                        Try_OpenFile(openFileDialog1.FileName);
                    }
                }
            }
            catch (Exception ex) {
                Show_ErrorMsg($"An unexpected error occured ({ex.Message}). Please contact your system administrator for further assistance.\nDetails have been saved to: \n'{Environment.CurrentDirectory}/ERR.txt'");
            }
        }

        #endregion

        #region HelperMethods

        /// <summary>
        /// Shows a message to the user
        /// </summary>
        public void Show_ErrorMsg(string msg, string title = "Error")
        {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a message to the user and saves the full errordescription to a file in the current workingdirectory.
        /// </summary>
        public void ShowAndSave_ErrorMsg(string msg, Exception ex, string title = "Error", bool exit=false)
        {
            File.WriteAllText(".\\ERR.txt", $"Unexpected error ocurred at (UTC){DateTime.UtcNow}:\r\n\r\n{ex.ToString()}");
            MessageBox.Show(msg + $"\n\nPlease contact your system administrator for further assistance.\nDetails have been saved to: \n\n'{Environment.CurrentDirectory}\\ERR.txt'", title, MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (exit)
                Environment.Exit(1);
        }

        #endregion

        private void comboBox_encoding_SelectedValueChanged(object sender, EventArgs e)
        {
            Prefered_Encoding = Encodings[(string)comboBox_encoding.Items[comboBox_encoding.SelectedIndex]];
            if (LastFileName != "")
                Try_OpenFile(LastFileName);
        }
    }
}
