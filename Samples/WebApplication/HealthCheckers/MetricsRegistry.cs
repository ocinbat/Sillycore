using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.ReservoirSampling.Uniform;
using App.Metrics.Timer;

namespace WebApplication.HealthCheckers
{
    public static class MetricsRegistry
    {

        public static CounterOptions GlobalCounter => new CounterOptions
        {
            MeasurementUnit = Unit.Calls
        };

        public static TimerOptions GlobalTimer => new TimerOptions
        {
            Reservoir = () => new DefaultAlgorithmRReservoir(),
            MeasurementUnit = Unit.Requests,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Minutes
        };
    }
}