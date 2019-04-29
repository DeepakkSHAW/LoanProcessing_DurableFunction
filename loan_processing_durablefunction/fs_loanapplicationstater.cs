using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace loan_processing_durablefunction
{
    public static class fs_loanapplicationstater
    {
        [FunctionName("LoanApplicationstater")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            bool IsValidLoanApplication = false;
            log.LogInformation("C# HTTP trigger function processed a request.");
            LoanApplication loanApplication = new LoanApplication();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            //***Deserialize Json to a loan application Object directly***//
            loanApplication = JsonConvert.DeserializeObject<LoanApplication>(requestBody);
            if (!string.IsNullOrEmpty(loanApplication.name))
            {
                IsValidLoanApplication = true;
            }
            //return IsValidLoanApplication.name != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            return (ActionResult)new OkObjectResult($"Hello, {loanApplication.name}");
        }
    }
}
