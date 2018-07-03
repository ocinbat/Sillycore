using App.Metrics.Counter;
using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics;
using App.Metrics.ReservoirSampling.Uniform;
using App.Metrics.Timer;

namespace ConsoleApp.HealthCheckers
{
    class MetricsRegistry
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
