using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace loan_processing_durablefunction
{
    public static class o_LoanApplication
    {
        [FunctionName("o_LoanApplication")]
        public static async Task<object> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            string CustomerHistrory = string.Empty;
            bool isLoanApproved = false;
            var tasks = new List<object>();
            dynamic validationResults;
            dynamic IsloanApplicationAccepted;
            dynamic IsExistingApplicant;
            dynamic IsCustomerHistroryCleaned;
            dynamic IsCustomerCreditHistroryCleaned;
            dynamic returnvalue;

            var rtOptions = new RetryOptions(
                                firstRetryInterval: TimeSpan.FromSeconds(5),
                                maxNumberOfAttempts: 3)
            { Handle = ex => ex.InnerException is InvalidOperationException };// .Message == "Monitoring failed."} ;
            //{Handle = ex => ex.InnerException is InvalidOperationException}, //- currently not possible #84
            rtOptions.BackoffCoefficient = 2.0;

            try
            {
                var loanapp = context.GetInput<LoanApplication>();

                if (!context.IsReplaying)
                    log.LogWarning("About to call loan application accepted activity function");
                IsloanApplicationAccepted = await context.CallActivityAsync<object>("a_LoanApplication_Accepted", loanapp);

                    //**Loan Application accepted**//
                    if (IsloanApplicationAccepted.LoanApplicationIsAccepted.ToObject<bool>())
                    {
                    //**Once application accepted check application is validated against Baseness Rules.**//
                    //**Such as age, loan amount etc.**//
                    if (!context.IsReplaying)
                            log.LogWarning("About to call loan application validation function in parallel - fan-out/fan-in");
                    #region ////** Validation Approach: 1 **//
                    //string[] BRules = { "Age", "TFN", "LoanAmt" };
                    //var BRETasks = new List<Task<ReturnMessage>>();

                    //foreach (string bre in BRules)
                    //{
                    //    var task = context.CallActivityAsync<ReturnMessage>("a_LoanApplication_BREValidation",
                    //        new BusinessRules() { Rule = bre, LoanApplication = loanapp });
                    //     BRETasks.Add(task);
                    //}
                    //validationResults = await Task.WhenAll(BRETasks);

                    ////** Check whether loan application validates the business rules **//
                    //for (int i = 0; i < validationResults?.Length; i++)
                    //    if (!validationResults[i]?.IsSuccess)
                    //        throw new Exception(validationResults[i]?.Message);
                    #endregion

                    //** Validation Approach: 2 using sub Orchestration **//
                    var v = await context.CallSubOrchestratorAsync<object>("o_LoanApplicationValidation", loanapp);
                    validationResults = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(v.ToString());
                    foreach (var item in validationResults)
                    {
                        //if (!validationResults[i].LoanApplicationIsValidated.ToObject<bool>())
                        //    throw new Exception(validationResults[i].Message.ToObject<string>());
                        if (!(bool)item["LoanApplicationIsValidated"])
                            throw new Exception((string)item["Message"]);
                    }

                    //**Check loan applicant is existing customer if yes get the customer id**//
                    if (!context.IsReplaying)
                        log.LogWarning("About to call loan applicant validation process");
                    
                    //since it's a db call, intermittent networking issue may recover with retry mechanize
                    IsExistingApplicant = await context.CallActivityWithRetryAsync<object>(
                        "a_LoanApplicantCheck", rtOptions,loanapp);

                    if (!context.IsReplaying)
                        log.LogWarning($"Bank account exists: {IsExistingApplicant.IsExists}");

                    if((bool)IsExistingApplicant["IsExists"])
                    {
                        //**get the customer id and check the bank history**//
                        //If Bank History is good loan application approves otherwise send email 
                        //to manager for manual review to approve or reject the loan application
                        if (!context.IsReplaying)
                            log.LogWarning("About to call loan applicant Bank History check process");
                        IsCustomerHistroryCleaned = await context.CallActivityWithRetryAsync<bool>(
                        "a_LoanApplicantBankHistory", rtOptions, IsExistingApplicant["CUSTID"]);

                        if (IsCustomerHistroryCleaned)
                        {
                            CustomerHistrory = $"Loan applicant (CustID:{IsExistingApplicant.CUSTID}) bank history has been validated.";
                            isLoanApproved = IsCustomerHistroryCleaned;
                        }
                        else
                        {
                            CustomerHistrory = $"Loan applicant bank history is not clean.. ";
                            //Send email to bank manager to do manual review of loan application
                            returnvalue = await context.CallSubOrchestratorAsync<object>("o_LoanApplicationWaitingManagerApproval", loanapp);
                            isLoanApproved = returnvalue["Approved"];
                            CustomerHistrory += returnvalue["Reason"];
                        }
                    }
                    else
                    {
                        //** Otherwise check the credit History from external API
                        //If Credit History is good send email to manager for manual review to approve or reject the loan application
                        //If Credit History doesn't support loan application will be rejected 
                        if (!context.IsReplaying)
                            log.LogWarning("About to call loan applicant Credit History check process");
                        IsCustomerCreditHistroryCleaned = await context.CallActivityWithRetryAsync<bool>(
                        "a_LoanApplicantCreditHistory", rtOptions, loanapp);
                        if (IsCustomerCreditHistroryCleaned)
                        {
                            CustomerHistrory = $"Loan applicant {loanapp.name}, credit history has been validated, but not a customer of this bank.";
                            //Send email to manger to review the loan application manually
                            //Send email to bank manager to do manual review of loan application
                            returnvalue = await context.CallSubOrchestratorAsync<object>("o_LoanApplicationWaitingManagerApproval", loanapp);
                            isLoanApproved = returnvalue["Approved"];
                            CustomerHistrory += returnvalue["Reason"];
                        }
                        else
                        {
                            CustomerHistrory = $"Loan applicant credit check validation failed.";
                            isLoanApproved = false;
                        }
                    }
                }
                else
                    {
                        throw new Exception(IsloanApplicationAccepted.Message.ToObject<string>());
                    }
            }
            catch (Exception e)
            {
                if (!context.IsReplaying)
                    log.LogError($"Caught an error from an activity: {e.Message}");

                //await context.CallActivityAsync<string>("a_CleanupActicity",new[] { Loanapplication, CreditHitory, LoggedPersonalDetails });

                return new
                {
                    Error = "Failed to process loan application",
                    Message = e.Message
                };
            }
            if (!context.IsReplaying)
                log.LogDebug("loan applicant Orchestrator completed successfully");
            return new
            {
                ApplicationAccepted = IsloanApplicationAccepted,
                ApplicationValidation = validationResults,
                ExisitingCutomer = IsExistingApplicant,
                CustomerHistrory = CustomerHistrory,
                IsLoanApproved = isLoanApproved == true ? "Loan has approved" : "Loan application rejected"
                };

        }
    }
}