using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Core.Enums
{
    public enum TaskStatus
    {
        Initiated,
        InProgress,
        //Paused,
        Completed,
        Null

    }
    public enum TaskCompletionStatus
    {
        Null,
        Successful,
        Abandoned,
        Failed
    }
    
}
