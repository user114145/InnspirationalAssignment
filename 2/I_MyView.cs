using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_MT940_Checker
{
    public interface I_MyView
    {

        /// <summary>
        /// Shows the validation results to the user.
        /// </summary>
        void Show_ValidationResults(List<(string transactionRef, string transactionDescr, List<string> validationErrors)> faulty_records);


        /// <summary>
        /// Shows a message to the user
        /// </summary>
        void Show_ErrorMsg(string msg, string title = "Error");

        /// <summary>
        /// Shows a message to the user and saves the full errordescription to a file in the current workingdirectory.
        /// </summary>
        void ShowAndSave_ErrorMsg(string msg, Exception ex, string title = "Error", bool exit = false);
    }
}
