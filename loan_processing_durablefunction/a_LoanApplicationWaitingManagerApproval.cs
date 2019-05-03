using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace loan_processing_durablefunction
{
    public static class o_LoanApplicationWaitingManagerApproval
    {
        [FunctionName("o_LoanApplicationWaitingManagerApproval")]
        public static async Task<object> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            string reason = "Unknown";
            bool isLoanApproved = false;
            if (!double.TryParse(System.Environment.GetEnvironmentVariable("TimeOut_Sec", System.EnvironmentVariableTarget.Process), out double timeout)) throw new System.Exception("Timeout not defined correctly");
            var loanapp = context.GetInput<LoanApplication>();

            if (!context.IsReplaying)
                 log.LogWarning("About to call loan applicant 'Send Email Manual Approval' process");
                await context.CallActivityAsync("a_SendEmail_ManualApproval", new ApprovalInfo()
                {
                    OrchestrationId = context.InstanceId,
                    Reason = $"Existing Customer (Name:{loanapp.name}) doesn't have clean history in bank, review required",
                    loanApplication = loanapp
                });
            
            using (var cts = new CancellationTokenSource())
            {
                var timeoutAt = context.CurrentUtcDateTime.AddSeconds(timeout);
                var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                var approvalTask = context.WaitForExternalEvent<string>("ManagerApprovalResult");

                var winner = await Task.WhenAny(approvalTask, timeoutTask);
                if (winner == approvalTask)
                {
                    var approvalResult = approvalTask.Result;
                    cts.Cancel(); // The timeout task should be canceled 
                    if (approvalResult == "Approved")
                    {
                        reason = $"However, Loan has been approved by manager."; 
                        isLoanApproved = true;
                    }
                    else
                    {
                        reason = $"Loan has been rejected by manager.";
                        isLoanApproved = false;
                    }
                }
                else
                {
                    reason = "Bank Manager didn't looked into your application and timed out. Please call Bank customer care for further details.";
                    isLoanApproved = false;
                }
            }
            return new
            {
                Approved = isLoanApproved,
                Reason = reason
            };
        }
    }
}