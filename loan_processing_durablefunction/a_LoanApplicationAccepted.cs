using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace loan_processing_durablefunction
{
    public static class a_LoanApplicationAccepted
    {
        [FunctionName("a_LoanApplication_Accepted")]
        public static object LoanApplicationAccepted([ActivityTrigger] LoanApplication lapp, ILogger log)
        {
            bool IsApplicationAccepted = false;
            string sAcceptationMsg = string.Empty;
            try
            {
                log.LogInformation($"Application Validation process started for applicant: {lapp.name}.");

                if (!string.IsNullOrEmpty(lapp.name)) { IsApplicationAccepted = true; }
                else { IsApplicationAccepted = false; sAcceptationMsg += @"Loan applicant name is required\n\r"; }

                if (DateTime.TryParse(lapp.dateofbirth.ToString(), out DateTime dummyDate)) { IsApplicationAccepted = IsApplicationAccepted && true; }
                else { IsApplicationAccepted = false; sAcceptationMsg += @"Loan applicant data of Birth is required to accept the loan application\n\r"; }

                if (!string.IsNullOrEmpty(lapp.taxfileno)) { IsApplicationAccepted = IsApplicationAccepted && true; }
                else { IsApplicationAccepted = false; sAcceptationMsg += @"To accept your Loan application your tax file number is needed\n\r"; }

                double dummyLoanAmt;
                if (double.TryParse(lapp.loanamount.ToString(), out dummyLoanAmt))
                {
                    if (dummyLoanAmt > 0)
                        IsApplicationAccepted = IsApplicationAccepted && true;
                    else { IsApplicationAccepted = false; sAcceptationMsg += @"Loan Amount can't be a -ev amount\n\r"; }
                }
                else { IsApplicationAccepted = false; sAcceptationMsg += @"Loan Amount is required to accept the loan application\n\r"; }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                sAcceptationMsg = ex.Message;
                IsApplicationAccepted = false;
            }
            return
                new { LoanApplicationIsAccepted = IsApplicationAccepted, Message = sAcceptationMsg };
        }
    }
}