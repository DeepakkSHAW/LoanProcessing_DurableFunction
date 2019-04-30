using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace loan_processing_durablefunction
{
    public static class s_LoanApplication
    {
        [FunctionName("s_LoanApplication")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            bool IsValidLoanApplication = false;
            log.LogInformation("C# HTTP trigger function processed a loan request.");
            LoanApplication loanApplication = new LoanApplication();
            try
            {
                //string requestBody = await new StreamReader(req.Content.Body).ReadToEndAsync();
                dynamic requestBody = await req.Content.ReadAsAsync<object>();

                //***Deserialize Json to a loan application Object directly***//
                var intrem = JsonConvert.SerializeObject(requestBody);
                loanApplication = JsonConvert.DeserializeObject<LoanApplication>(intrem);

                //dummy value for a loan application
                //loanApplication = new LoanApplication { name = "Julia", lastname = "Dicosta", dateofbirth = DateTime.Now.AddYears(-20), loanamount = 200.20, taxfileno = "TAX001" };
                if (loanApplication != null)
                {
                    log.LogInformation($"About to start orchestration for {loanApplication}");
                    var orchestrationId = await starter.StartNewAsync("o_LoanApplication", loanApplication);
                    log.LogInformation($"Started orchestration with ID = '{orchestrationId}'.");
                    return starter.CreateCheckStatusResponse(req, orchestrationId);
                }
                else
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Loan application incomplete..");

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}