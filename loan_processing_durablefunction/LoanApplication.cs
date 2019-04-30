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

    public class Customer : LoanApplication
    {
        public string CUSTID { get; set; }
        public LoanApplication LoanApplication { get; set; }
    }
}
