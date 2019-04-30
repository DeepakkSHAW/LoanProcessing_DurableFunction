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
    public static class o_LoanApplication
    {
        [FunctionName("o_LoanApplication")]
        public static async Task<object> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            var tasks = new Task<object>[3];
            dynamic validationResults;
            dynamic IsloanApplicationAccepted;
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
                    //var sRule = "Age";
                    //object oToValidate = new { aLoanApplication = loanapp, BRE =  sRule };

                    if (!context.IsReplaying)
                            log.LogWarning("About to call loan application validation function in parallel - fan-out/fan-in");                    
                    tasks[0] = context.CallActivityAsync<object>("a_LoanApplicationValidation_Age", loanapp);
                    tasks[1] = context.CallActivityAsync<object>("a_LoanApplicationValidation_tfn", loanapp);
                    tasks[2] = context.CallActivityAsync<object>("a_LoanApplicationValidation_lAmt", loanapp);
                    validationResults = await Task.WhenAll(tasks);

                    //** Check whether loan application validates the business rules **//
                    for (int i = 0; i < tasks.Length; i++)
                        if (!validationResults[i].LoanApplicationIsValidated.ToObject<bool>())
                            throw new Exception(validationResults[i].Message.ToObject<string>());

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

                //await
                //    ctx.CallActivityAsync<string>("A_Cleanup",
                //        new[] { transcodedLocation, thumbnailLocation, withIntroLocation });

                return new
                {
                    Error = "Failed to process loan application",
                    Message = e.Message
                };
            }
                return new
            {
                ApplicationAccepted = IsloanApplicationAccepted,
                ApplicationValidation = validationResults,
                ExisitingCutomer = "Yes",
                WithIntro = "withIntroLocation"
            };

        }
    }
}