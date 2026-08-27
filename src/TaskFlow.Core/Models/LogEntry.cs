using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Core.Models
{
    public class LogEntry
    {
        public string? Id {  get; set; }
        public string? TraceId { get; set; }
        public string? Method {  get; set; }
        public string? Path { get; set; }
        public string? BaseUrl { get; set; }
        public int? ResponpseCode { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public DateTimeOffset Date { get; set; }
        public long ElapsedTimeMs { get; set; }
    }
}
