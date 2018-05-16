using System.Collections.Generic;
using MassTransit;

namespace Sillycore.RabbitMq
{
    public interface IBusControlProvider
    {
        IBusControl GetBusControl(string rabbitMq);

        List<IBusControl> GetBusControls();
    }
}