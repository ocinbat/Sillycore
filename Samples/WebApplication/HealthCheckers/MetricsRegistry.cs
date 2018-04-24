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
        public static CounterOptions SamplesGetCounter => new CounterOptions
        {
            Name = "QuerySamples Get Counter",
            MeasurementUnit = Unit.Calls
        };
        public static TimerOptions SamplesGetTimer => new TimerOptions
        {
            Name = "QuerySamples Get Timer",
            Reservoir = () => new DefaultAlgorithmRReservoir()
        };

        public static CounterOptions SamplesPostCounter => new CounterOptions
        {
            Name = "QuerySamples Post Counter",
            MeasurementUnit = Unit.Calls
        };
        public static TimerOptions SamplesPostTimer => new TimerOptions
        {
            Name = "QuerySamples Post Timer"
        };

    }
}