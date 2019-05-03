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
    public static class a_LoanApplicant
    {
        [FunctionName("a_LoanApplicantCheck")]
        public static LoanApplicant LoanApplicantCheck([ActivityTrigger] LoanApplication lapp, ILogger log)
        {
            log.LogInformation($"Loan applicant {lapp.name} check has started.");
            string sCustID = string.Empty;
            bool bCustomerFound = false;
            try
            {
                // Check of customer exists in Database
                // If yes get the CustomerID
                sCustID = "8101801";
                bCustomerFound = false;

                if (string.IsNullOrEmpty(lapp.name))
                    throw new InvalidOperationException("can't extract applicant name from loan application");
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw (ex);
            }
            return
                new LoanApplicant
                {
                    IsExists = bCustomerFound,
                    CUSTID = sCustID
                };
        }

        [FunctionName("a_LoanApplicantBankHistory")]
        public static bool LoanApplicantBankHistory([ActivityTrigger] string custid, ILogger log)
        {
            log.LogInformation($"Customer Bank History check has started.");
            string sCustID = string.Empty;
            bool IsCustomerHistroryCleaned = false;
            try
            {
                if (string.IsNullOrEmpty(custid))
                    throw new InvalidOperationException("can't extract customer id from loan application");
                // Check of customer history in Database
                IsCustomerHistroryCleaned = true;

                // simulation of db call
                Task.Delay(5000); 
           }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw (ex);
            }
            return IsCustomerHistroryCleaned;
        }

        [FunctionName("a_LoanApplicantCreditHistory")]
        public static bool LoanApplicantCreditHistory([ActivityTrigger] LoanApplication lapp, ILogger log)
        {
            log.LogInformation($"Customer {lapp.name} Credit History check has started.");
            bool IsCustomerCreditHistroryCleaned = false;
            try
            {
                if (string.IsNullOrEmpty(lapp.name))
                    throw new InvalidOperationException("can't extract applicant name from loan application");
                // Check of customer exists in Database
                // If yes get the CustomerID
                IsCustomerCreditHistroryCleaned = true;
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw (ex);
            }
            return IsCustomerCreditHistroryCleaned;
        }
    }
}