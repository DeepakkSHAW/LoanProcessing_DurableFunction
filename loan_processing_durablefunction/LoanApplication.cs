using System;
using System.Collections.Generic;
using System.Text;

namespace loan_processing_durablefunction
{
    public class LoanApplication : System.Attribute
    {
        //[]
        public string name { get; set; }
        public string lastname { get; set; }
        public DateTime dateofbirth { get; set; }
        public string taxfileno { get; set; }
        public double loanamount { get; set; }
    }
}
