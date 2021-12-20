using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Simple_MT940_Checker
{
    public static class Model
    {

        public enum FileExtensions { xml, csv }
        public enum ColumnName { Reference, Account_Number, Description, Start_Balance, Mutation, End_Balance };

        delegate List<(string transactionRef, string transactionDescr, List<string> validationErrors)> FileCheckFunction(StreamReader reader);

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


        #region Main functionality

        public static void Check_File(string filePath, FileExtensions ext)
        {
            StreamReader streamReader;
            try {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (BufferedStream bs = new BufferedStream(fs))
                        streamReader = new StreamReader(bs);                
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
            try {
                var faultyRecords = checkFunction(streamReader);
            }
            catch (Exception ex) {
                View.ShowAndSave_ErrorMsg($"Could not validate file: {ex.Message}.", ex);
            }
        }


        private static List<(string transactionRef, string transactionDescr, List<string> validationErrors)> Check_XML_File(StreamReader reader) 
        {
            throw new NotImplementedException("XML here");

        }


        private static List<(string transactionRef, string transactionDescr, List<string> validationErrors)> Check_CSV_File(StreamReader reader)
        {
            var file_validation_result = new List<( string transactionRef, 
                                                    string transactionDescr, 
                                                    List<string> validationErrors)>();
            string line = reader.ReadLine();

            if (line == null)
                throw new Validation_Exception("File is empty.");

            var colNrs = new Dictionary<ColumnName, int>();
            int nColls = 0;

            // Check columnNames
            foreach (var word in line.Split(','))
                if (Enum.TryParse(word.Trim(' ').Replace(' ', '_'), out ColumnName column))
                    colNrs.Add(column, nColls++);
                else
                    throw new Exception($"Unknown columnname '{word}'");

            foreach (ColumnName expected_colname in Enum.GetValues(typeof(ColumnName)))
                if (!colNrs.ContainsKey(expected_colname))
                    throw new Exception($"File does not contain expected column '{expected_colname.ToString().Replace('_', ' ')}'.");

            // Check records
            var transactionChecker = new TransactionChecker();

            while ((line = reader.ReadLine()) != null) {
                var words = Array.ConvertAll(line.Split(','), (w)=>w.Trim(' '));

                var transaction = new TransactionChecker.Transaction {
                    Reference = words[colNrs[ColumnName.Reference]],
                    IBAN = words[colNrs[ColumnName.Account_Number]],
                    Description = words[colNrs[ColumnName.Description]],
                    Start_Balance = words[colNrs[ColumnName.Start_Balance]],
                    Mutation = words[colNrs[ColumnName.Mutation]],
                    End_Balance = words[colNrs[ColumnName.End_Balance]],
                };

                var validation = transactionChecker.Is_Valid_Transaction(transaction);

                if (!validation.valid)
                    file_validation_result.Add((    transactionRef: transaction.Reference,
                                                    transactionDescr: transaction.Description,
                                                    validationErrors: validation.reasons));
            }

            return file_validation_result;
        }

        #endregion


        #region Helper Classes


        



        /// <summary>
        /// Keeps a list of transaction references
        /// </summary>
        class TransactionChecker
        {
            HashSet<int> TransactionReferences;
            Dictionary<string, double> AccountBalaces;

            
            public TransactionChecker() {
                TransactionReferences = new HashSet<int>();
                AccountBalaces = new Dictionary<string, double>();
            }

            /// <summary>
            /// Checks for a given transaction whether:    <br/><br/>
            /// 1. transactionreference is numeric    <br/>
            /// 2. transactionreference is unique within the file    <br/>
            /// 3. IBAN code is technically correct    <br/>
            /// 4. start balance is numeric    <br/>
            /// 5. mutation is valid (addition or substraction)    <br/>
            /// 6. end balance is numeric    <br/>
            /// 7. end balance is the result of applying the mutation to the start balance    <br/>
            /// 8. start balance matches end balance of previous transaction for that IBAN    <br/>
            /// </summary>
            /// <returns>Whether all prerequisits are met, and a list of reasons if not.</returns>
            public (bool valid, List<string> reasons) Is_Valid_Transaction(Transaction transaction) 
            {
                var reasons = new List<string>();
                bool valid = true;
                bool can_check_mutationResult = true;
                bool can_use_EndBalance = true;

                // 1. transactionreference is numeric
                if (!int.TryParse(transaction.Reference, out int transactionReference)) {
                    valid = false;
                    reasons.Add("Transaction reference is not numeric.");
                }

                // 2. transactionreference is unique within the file
                if (!IsUnique_Reference(transactionReference)) {
                    valid = false;
                    reasons.Add("Transaction reference is not unique.");
                }

                // 3. IBAN code is technically correct
                try { IsValid_IBAN(transaction.IBAN); } 
                catch (Validation_Exception ex){
                    valid = false;
                    reasons.Add(ex.Message);
                }

                // 4. start balance is numeric
                if (!double.TryParse(transaction.Start_Balance, out double start_balace)) {
                    valid = false;
                    can_check_mutationResult = false;
                    reasons.Add("Start balance is not numeric.");
                }

                // 5. mutation is valid (addition or substraction)
                // assuming local systems region settings match those of document origin
                double mutation_amount = 0;
                var match = Regex.Match(transaction.Mutation, @"^ *([+-]) *([0-9.,]+) *$");
                if (!match.Success || !double.TryParse(match.Groups[2].Value, out mutation_amount)) {
                    valid = false;
                    can_check_mutationResult = false;
                    reasons.Add("Mutation is invalid.");
                }

                // 6. end balance is numeric
                if (!double.TryParse(transaction.End_Balance, out double end_balace)) {
                    valid = false;
                    can_check_mutationResult = false;
                    can_use_EndBalance = false;
                    reasons.Add("End balance is not numeric.");
                }

                // 7. end balance is the result of applying the mutation to the start balance
                if (can_check_mutationResult)
                    if ((match.Groups[1].Value == "+" && start_balace + mutation_amount != end_balace)
                        || (match.Groups[1].Value == "-" && start_balace - mutation_amount != end_balace)) 
                    {
                        valid = false;
                        can_use_EndBalance = false;
                        reasons.Add("Applying mutation to Start balace does not result in End balance.");
                    }

                // 8. start balance matches end balance of previous transaction for that IBAN
                if (AccountBalaces.ContainsKey(transaction.IBAN)) {
                    if (start_balace != AccountBalaces[transaction.IBAN]) {
                        valid = false;
                        reasons.Add("Start balance of transaction does not match previous End balace for this account.");
                    }
                }
                else if (can_use_EndBalance)
                    AccountBalaces.Add(transaction.IBAN, end_balace);

                return (valid, reasons);
            }

            /// <summary>
            /// Checks whether the transaction reference is unique.
            /// </summary>
            bool IsUnique_Reference(int reference)
            {
                // This would be the place to make a call to a database that keeps track of all transactions.
                // I would suggest an internal webservice that offers a restfull API over HTTP.
                // Transaction history could be stored as a hash of the transaction details,
                // indexed by reference number, to comply with GDPR by not unnecessary storing sensitive information.

                // For now only checking within the active batch.
                return TransactionReferences.Add(reference);
            }


            /// <summary>
            /// Contains all information of a single transaction.
            /// </summary>
            public struct Transaction
            {
                public string Reference;
                public string IBAN;
                public string Description;
                public string Start_Balance;
                public string Mutation;
                public string End_Balance;
            }

        }

        #endregion


        #region Helper Functons


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
