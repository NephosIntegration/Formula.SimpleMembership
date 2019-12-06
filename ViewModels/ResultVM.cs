using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formula.SimpleMembership
{
    public class ResultVM
    {
        public ResultStatus Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public enum ResultStatus
    {
        Success = 1,
        Error = 2
    }
}
