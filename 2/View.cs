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

namespace _2
{
    public partial class View : Form
    {


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
                    return;
                }
                else {
                    Model.Check_File(fileName, ext);
                }
            }
            catch (Exception ex) {
                ShowAndSave_ErrorMsg($"File could not be opened ({ex.Message}).", ex);
            }
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
        public static void Show_ErrorMsg(string msg, string title = "Error")
        {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a message to the user and saves the full errordescription to a file in the current workingdirectory.
        /// </summary>
        public static void ShowAndSave_ErrorMsg(string msg, Exception ex, string title = "Error", bool exit=false)
        {
            File.WriteAllText(".\\ERR.txt", $"Unexpected error ocurred at (UTC){DateTime.UtcNow}:\r\n\r\n{ex.ToString()}");
            MessageBox.Show(msg + $"\n\nPlease contact your system administrator for further assistance.\nDetails have been saved to: \n\n'{Environment.CurrentDirectory}\\ERR.txt'", title, MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (exit)
                Environment.Exit(1);
        }

        #endregion
    }
}
