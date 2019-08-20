using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntEvents
{
    public class SampleEvent
    {
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
        public string From { get; set; }
        public string Description { get; set; }
    }
}
