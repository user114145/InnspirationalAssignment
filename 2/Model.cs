using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace _2
{
    public static class Model
    {

        public enum FileExtensions { xml, csv }

        delegate List<(string transactionRef, string transactionDescr)> FileCheckFunction(string content);

        static Dictionary<FileExtensions, FileCheckFunction> CheckFunctions_per_FileType = new Dictionary<FileExtensions, FileCheckFunction> {
            { FileExtensions.xml, Check_XML_File }, // XML files 
            { FileExtensions.csv, Check_CSV_File }, // CSV files
        };

        private static Dictionary<string, (string name, int length)> _IbanLength_per_Country;
        static Dictionary<string, (string name, int length)> IbanLength_per_Country {
            get { if (_IbanLength_per_Country == null) {
                    _IbanLength_per_Country = Load_IbanLength_per_Country();
                }
                return _IbanLength_per_Country;
            }
            set {
                _IbanLength_per_Country = value;
            }
        }


        public static void Check_File(string filePath, FileExtensions ext)
        {
            string file_content;
            try {
                file_content = File.ReadAllText(filePath);
            }
            catch (Exception ex) {
                View.ShowAndSave_ErrorMsg($"File could not be opened ({ex.Message})", ex);
                return;
            }

            if (!CheckFunctions_per_FileType.ContainsKey(ext)) {
                View.ShowAndSave_ErrorMsg($"Handling of filetype '{ext.ToString()}' is not implemented.", new Exception($"Missing functionality: checkfuntion for '{ext.ToString()}' files."));
                return;
            }

            var checkFunction = CheckFunctions_per_FileType[ext];

            var faultyRecords = checkFunction(file_content);
            
        }


        private static List<(string transactionRef, string transactionDescr)> Check_XML_File(string content) 
        {
            throw new NotImplementedException("XML here");

        }


        private static List<(string transactionRef, string transactionDescr)> Check_CSV_File(string content)
        {
            throw new NotImplementedException("CSV here");

        }


        #region HelperFunctons




        /// <summary>
        /// Checks a given IBAN code using the steps described on https://en.wikipedia.org/wiki/International_Bank_Account_Number#Validating_the_IBAN
        /// It does not do any country-specific checks, other than the lengt per country-code.
        /// 
        /// Returns true if all checks passed.
        /// Throws Validation_Exception(reason) if not accepted.
        /// </summary>
        public static bool IsValid_IBAN(string iban) 
        {
            string substitute_chars_for_digits(string input) 
            {
                var chars_substituted_for_nrs = "";
                foreach (var c in input.ToUpper()) {
                    if (int.TryParse($"{c}", out int _))
                        chars_substituted_for_nrs += $"{c}";
                    else {
                        if (!CharNumber.ContainsKey(c))
                            throw new Validation_Exception($"IBAN contains illigal character: '{c}'");
                        chars_substituted_for_nrs += CharNumber[c];
                    }
                }

                return chars_substituted_for_nrs;
            }

            iban = iban.Replace(" ", "").ToUpper();

            // Check length
            var country_code = iban.Substring(0, 2);
            if (iban.Length != IbanLength_per_Country[country_code].length)
                throw new Validation_Exception($"IBAN is of wrong length({iban.Length}) for country {IbanLength_per_Country[country_code].name}");


            // Generic modulo check
            var iban_rearranged = iban.Substring(4) + iban.Substring(0, 4);
            var iban_chars_substituted_for_nr = substitute_chars_for_digits(iban_rearranged);
            var remainder = BigModulo(iban_chars_substituted_for_nr, 97);

            if (remainder != 1)
                throw new Validation_Exception("Not a valid IBAN code.");


            // IBAN check digits
            if (!int.TryParse(iban.Substring(2, 2), out int checkDigits))
                throw new Validation_Exception("Checkdigits are not numeric.");
            
            iban_rearranged = iban.Substring(4) + iban.Substring(0, 2) + "00";
            iban_chars_substituted_for_nr = substitute_chars_for_digits(iban_rearranged);
            remainder = BigModulo(iban_chars_substituted_for_nr, 97);
            
            if(98 - remainder != checkDigits)
                throw new Validation_Exception("Checkdigits not valid.");

            return true;
        }



        public static int BigModulo(string bigNumber, int devisor) 
        {
            var maxLength = Math.Max(devisor.ToString().Length, 7) + 1;

            if (bigNumber.Length > maxLength) {
                if (!int.TryParse(bigNumber.Substring(0, maxLength), out int partial_number))
                    throw new Validation_Exception($"Substring '{bigNumber}' contains non-numeric characters.");

                var remainder = partial_number % devisor;
                return BigModulo(remainder.ToString() + bigNumber.Substring(maxLength), devisor);
            }
            else {
                if (!int.TryParse(bigNumber, out int number))
                    throw new Validation_Exception($"String '{bigNumber}' contains non-numeric characters.");

                return number % devisor;
            }            
        }



        private static Dictionary<string, (string name, int length)> Load_IbanLength_per_Country()
        {
            var result = new Dictionary<string, (string name, int length)>();

            try {
                foreach (var line in File.ReadAllLines("./IbanLength_per_Country.csv")) {
                    var words = line.Split('\t');
                    result.Add(words[2], (name: words[0], length: int.Parse(words[1])));
                }

            }
            catch (Exception err) {
                View.ShowAndSave_ErrorMsg("Could not load reference file 'IbanLength_per_Country.csv'", err, exit:true);
            }

            return result;
        }


        static Dictionary<char, string> CharNumber = new Dictionary<char, string> {
            {'A', "10"},
            {'B', "11"},
            {'C', "12"},
            {'D', "13"},
            {'E', "14"},
            {'F', "15"},
            {'G', "16"},
            {'H', "17"},
            {'I', "18"},
            {'J', "19"},
            {'K', "20"},
            {'L', "21"},
            {'M', "22"},
            {'N', "23"},
            {'O', "24"},
            {'P', "25"},
            {'Q', "26"},
            {'R', "27"},
            {'S', "28"},
            {'T', "29"},
            {'U', "30"},
            {'V', "31"},
            {'W', "32"},
            {'X', "33"},
            {'Y', "34"},
            {'Z', "35"},
        };


        public class Validation_Exception : Exception
        {
            public Validation_Exception(string message) : base(message)
            {}
        }

        #endregion





    }
}
