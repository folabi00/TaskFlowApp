using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Core.Commons
{
    public class BaseResponse
    {
        //public int ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public object? Result { get; set; }

    }
    public class BaseNullResponse
    {
        public string ResponseMessage { get; set; } = "Something went wrong";
        public object? Result { get; set; } = null;
    }
}
