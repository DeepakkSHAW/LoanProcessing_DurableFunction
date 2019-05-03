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
    public static class a_ManagerApprovalEmail
    {
        [FunctionName("a_SendEmail_ManualApproval")]
        public static void ManualApproval_ExistingCustomer([ActivityTrigger]
        ApprovalInfo approvalInfo,
        [Table("TLoanApprovals", Connection = "AzureWebJobsStorage")] out Approval TApproval,

        ILogger log)
        {
            log.LogInformation($"Request for manual loan approval for existing customer name {approvalInfo.loanApplication.name}.");
            var approvalCode = Guid.NewGuid().ToString("N");
            TApproval = new Approval
            {
                PartitionKey = "LoanManualApproval",
                RowKey = approvalCode,
                OrchestrationId = approvalInfo.OrchestrationId
            };

            var approverEmail = System.Environment.GetEnvironmentVariable("ApproverEmail", EnvironmentVariableTarget.Process);
            var senderEmail = System.Environment.GetEnvironmentVariable("SenderEmail", EnvironmentVariableTarget.Process);
            var host = System.Environment.GetEnvironmentVariable("Host",EnvironmentVariableTarget.Process);
            var subject = approvalInfo.Reason;
            log.LogWarning($"Sending approval request for {approvalInfo.Reason}");

            var functionAddress = $"{host}/api/ManagerReviewLoanApproval/{approvalCode}";
            var approvedLink = functionAddress + "?result=Approved";
            var rejectedLink = functionAddress + "?result=Rejected";
            var body = $"Please review loan request for {approvalInfo.loanApplication.name}<br>"
                               + $"<a href=\"{approvedLink}\">Approve</a><br>"
                               + $"<a href=\"{rejectedLink}\">Reject</a>";
            //var content = new Content("text/html", body);
            //message = new Mail(senderEmail, subject, approverEmail, content);
            log.LogError(approvedLink);
            log.LogWarning(body);
            Task.Delay(1000);
        }


    }
}