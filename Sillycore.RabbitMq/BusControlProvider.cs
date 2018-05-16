using System.Collections.Generic;
using System.Linq;
using MassTransit;

namespace Sillycore.RabbitMq
{
    internal class BusControlProvider : IBusControlProvider
    {
        private readonly Dictionary<string, IBusControl> _busControls = new Dictionary<string, IBusControl>();

        public IBusControl GetBusControl(string rabbitMq)
        {
            return _busControls[rabbitMq];
        }

        public List<IBusControl> GetBusControls()
        {
            return _busControls.Select(p => p.Value).ToList();
        }

        internal void AddBusControl(string rabbitMq, IBusControl busControl)
        {
            _busControls.Add(rabbitMq, busControl);
        }
    }
}