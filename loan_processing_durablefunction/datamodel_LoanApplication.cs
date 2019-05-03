using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace loan_processing_durablefunction
{
    public class LoanApplication : System.Attribute
    {
        [Required(ErrorMessage = "Name is required for loan application")]
        public string name { get; set; }
        public string lastname { get; set; }
        [Required()]
        public DateTime dateofbirth { get; set; }
        [Required()]
        public string taxfileno { get; set; }
        [Required()]
        public double loanamount { get; set; }
    }
    public class LoanApplicant
    {   // applicant lookup
        public bool IsExists { get; set; }
        public string CUSTID { get; set; }
        public LoanApplication LoanApplication { get; set; }
    }
    public class BusinessRules
    {
        public string Rule { get; set; }
        public LoanApplication LoanApplication { get; set; }
    }
    public class ReturnMessage
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class ApprovalInfo
    {
        public string OrchestrationId { get; set; }
        public string Reason { get; set; }
        public LoanApplication loanApplication {get; set;}
    }
    public class Approval
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string OrchestrationId { get; set; }
    }
}