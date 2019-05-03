using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace loan_processing_durablefunction
{
    public static class a_LoanApplicationValidationed
    {
        [FunctionName("a_LoanApplication_BREValidation")]
        public static ReturnMessage LoanApplicationBREValidation([ActivityTrigger] BusinessRules oBRE, ILogger log)
        {
            bool IsApplicationValidationed = false;
            string sValidationMsg = string.Empty;
            try
            {
                switch (oBRE.Rule)
                {
                    case "Age":
                        sValidationMsg = "Applicant Age validation..";
                        DateTime dob = Convert.ToDateTime(oBRE.LoanApplication.dateofbirth);
                        var age = DateTime.Now.Year - dob.Year;
                        string AgeLimit = System.Environment.GetEnvironmentVariable("AgeLimit", EnvironmentVariableTarget.Process);
                        var ages = AgeLimit.Split(',').Select(Int32.Parse).ToList();
                        var minage = ages[0]; var maxage = ages[1];

                        if (age >= minage && age <= maxage)
                        {
                            IsApplicationValidationed = true;
                        }
                        else
                        {
                            sValidationMsg = string.Format("The Applicant age must be between {0} and {1}.", minage, maxage);
                        }
                        break;

                    case "TFN":
                        sValidationMsg = "Tax File number validation..";
                        string aTaxFilePattern = System.Environment.GetEnvironmentVariable("TaxFileNoPattern", EnvironmentVariableTarget.Process);
                        if (!string.IsNullOrEmpty(oBRE.LoanApplication.taxfileno))
                        {
                            Match match = Regex.Match(oBRE.LoanApplication.taxfileno, aTaxFilePattern, RegexOptions.IgnoreCase);
                            if (match.Success)
                                IsApplicationValidationed = true;
                            else
                                sValidationMsg = string.Format("The Applicant must have a valid tax file no. the provided tax file no {0} is invalid .", oBRE.LoanApplication.taxfileno);
                        }
                        else
                        {
                            sValidationMsg = string.Format("The Applicant must have a valid tax file no. the provided tax file no {0} is invalid .", oBRE.LoanApplication.taxfileno);
                        }
                        break;

                    case "LoanAmt":
                        sValidationMsg = "Applicant requested loan amount validation..";
                        double loanAmountReq = Convert.ToDouble(oBRE.LoanApplication.loanamount);
                        string LoanAmout = System.Environment.GetEnvironmentVariable("LoanAmountThreshold", EnvironmentVariableTarget.Process);
                        var loanAmtThr = LoanAmout.Split('-').Select(double.Parse).ToList();
                        var minAmt = loanAmtThr[0]; var maxAmt = loanAmtThr[1];

                        if (loanAmountReq >= minAmt && loanAmountReq <= maxAmt)
                        {
                            IsApplicationValidationed = true;
                        }
                        else
                        {
                            sValidationMsg = string.Format("The Application loan amount should be between {0} and {1}.", minAmt, maxAmt);
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                sValidationMsg = ex.Message;
                IsApplicationValidationed = false;
            }
            return
                new ReturnMessage
                {
                    Message = sValidationMsg,
                    IsSuccess = IsApplicationValidationed
                };
        }

        [FunctionName("a_LoanApplicationValidation_tfn")]
        public static object LoanApplicationBREValidation_taxfileno([ActivityTrigger] LoanApplication lapp, ILogger log)
        {
            bool IsApplicationAccepted = false;
            string sValidationMsg = "Tax File number validation..";

            //creitera1: applicant should have tax file number and matches the pattern 1298-673-4192
            try
            {
                string aTaxFilePattern = System.Environment.GetEnvironmentVariable("TaxFileNoPattern", EnvironmentVariableTarget.Process);
                if (!string.IsNullOrEmpty(lapp.taxfileno))
                {
                    Match match = Regex.Match(lapp.taxfileno, aTaxFilePattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                        IsApplicationAccepted = true;
                    else
                        sValidationMsg = string.Format("The Applicant must have a valid tax file no. the provided tax file no {0} is invalid .", lapp.taxfileno);
                }
                else
                {
                    sValidationMsg = string.Format("The Applicant must have a valid tax file no. the provided tax file no {0} is invalid .", lapp.taxfileno);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                sValidationMsg = ex.Message;
                IsApplicationAccepted = false;
            }
            return
                new
                {
                    LoanApplicationIsValidated = IsApplicationAccepted,
                    Message = sValidationMsg
                };
        }

        [FunctionName("a_LoanApplicationValidation_Age")]
        public static object LoanApplicationBREValidation_Age([ActivityTrigger] LoanApplication lapp, ILogger log)
        {
            bool IsApplicationAccepted = false;
            string sValidationMsg = "Applicant Age validation..";
            //creitera2: applicant age must be between 21 yrs to 60 yrs
            try
            {
                DateTime dob = Convert.ToDateTime(lapp.dateofbirth);
                var age = DateTime.Now.Year - dob.Year;
                string AgeLimit = System.Environment.GetEnvironmentVariable("AgeLimit", EnvironmentVariableTarget.Process);
                var ages = AgeLimit.Split(',').Select(Int32.Parse).ToList();
                var minage = ages[0];  var maxage = ages[1];

                if (age >= minage && age <= maxage)
                {
                    IsApplicationAccepted = true;
                }
                else
                {
                    sValidationMsg = string.Format("The Applicant age must be between {0} and {1}.", minage, maxage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                sValidationMsg = ex.Message;
                IsApplicationAccepted = false;
            }
            return
                new
                {
                    LoanApplicationIsValidated = IsApplicationAccepted,
                    Message = sValidationMsg
                };
        }

        [FunctionName("a_LoanApplicationValidation_lAmt")]
        public static object LoanApplicationBREValidation_LoanAmt([ActivityTrigger] LoanApplication lapp, ILogger log)
        {
            bool IsApplicationAccepted = false;
            string sValidationMsg = "Applicant requested loan amount validation..";

            //creitera3: minimum loan amount must more than configurable item
            try
            {
                double loanAmountReq = Convert.ToDouble(lapp.loanamount);
                string LoanAmout = System.Environment.GetEnvironmentVariable("LoanAmountThreshold", EnvironmentVariableTarget.Process);
                var loanAmtThr = LoanAmout.Split('-').Select(double.Parse).ToList();
                var minAmt = loanAmtThr[0]; var maxAmt = loanAmtThr[1];

                if (loanAmountReq >= minAmt && loanAmountReq <= maxAmt)
                {
                    IsApplicationAccepted = true;
                }
                else
                {
                    sValidationMsg = string.Format("The Application loan amount should be between {0} and {1}.", minAmt, maxAmt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                sValidationMsg = ex.Message;
                IsApplicationAccepted = false;
            }
            return
                new
                {
                    LoanApplicationIsValidated = IsApplicationAccepted,
                    Message = sValidationMsg
                };
        }
    }
}