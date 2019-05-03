using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace loan_processing_durablefunction
{
    public static class o_LoanApplicationValidation
    {
        [FunctionName("o_LoanApplicationValidation")]
        public static async Task<object> RunValidaterOrchestrator(
        [OrchestrationTrigger] DurableOrchestrationContext context,
        ILogger log)
        {
            var tasks = new Task<object>[3];
            dynamic validationResults;
            var loanapp = context.GetInput<LoanApplication>();
            tasks[0] = context.CallActivityAsync<object>("a_LoanApplicationValidation_Age", loanapp);
            tasks[1] = context.CallActivityAsync<object>("a_LoanApplicationValidation_tfn", loanapp);
            tasks[2] = context.CallActivityAsync<object>("a_LoanApplicationValidation_lAmt", loanapp);
            validationResults = await Task.WhenAll(tasks);
            return validationResults;
        }
    }
}