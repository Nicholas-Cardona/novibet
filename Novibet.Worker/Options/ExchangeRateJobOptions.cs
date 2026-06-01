using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Novibet.Worker.Options;
public class ExchangeRateJobOptions
{
    public int StartDelaySeconds { get; set; }
    public int IntervalSeconds { get; set; }    
}