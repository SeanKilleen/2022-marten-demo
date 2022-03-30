using System;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.Hosting;

namespace MartenDemo
{
    public class DemoService : IHostedService
    {
        IDocumentStore _marten;
        public DemoService(IDocumentStore marten)
        {
            _marten = marten;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_marten is null) { throw new ArgumentNullException(nameof(_marten)); }

            var itemName = "Funny hat";
            var added1 = new ItemAdded(itemName, 1);
            var added2 = new ItemAdded(itemName, 4);
            var removed1 = new ItemRemoved(itemName, 3);
            var added3 = new ItemAdded(itemName, 1);

            Console.WriteLine("Events instantiated");
            using (var session = _marten.OpenSession())
            {

                Console.WriteLine("Session opened");
                var streamId1 = session.Events.StartStream<InventoryItemAggregate>(added1, added2, removed1, added3).Id;

                Console.WriteLine($"Stream created at ID '{streamId1}' -- saving");

                await session.SaveChangesAsync();

                Console.WriteLine("Rehydrating");
                var rehydratedItem = session.Events.AggregateStream<InventoryItemAggregate>(streamId1);

                Console.WriteLine(rehydratedItem.ToString());
            }

            return;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
