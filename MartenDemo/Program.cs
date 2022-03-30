using System;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weasel.Core;

namespace MartenDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMarten(options =>
                {
                    options.Connection("host=localhost;port=5432;database=postgres;user id=postgres;password=mysecretpassword");
                    options.AutoCreateSchemaObjects = AutoCreate.All; // For demo purposes only
                });
                services.AddHostedService<DemoService>();
            }).RunConsoleAsync();
        }

    }

    public record ItemAdded(string ItemName, int Quantity);
    public record ItemRemoved(string ItemName, int Quantity);

    public class InventoryItemAggregate
    {
        public int RemovalEvents { get; set; } = 0;
        public int AddedEvents { get; set; } = 0;
        public int CurrentQuantity { get; set; } = 0;

        public Guid Id { get; set; }

        // These methods take in events and update the QuestParty
        public void Apply(ItemAdded added)
        {
            this.AddedEvents++;
            this.CurrentQuantity += added.Quantity;
        }

        public void Apply(ItemRemoved removed)
        {
            this.RemovalEvents++;
            this.CurrentQuantity -= removed.Quantity;
        }

        public override string ToString()
        {
            return $"Inventory item Id '{Id}' has {AddedEvents} added events, {RemovalEvents} removal events, and a quantity of {CurrentQuantity}";
        }
    }

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
                var rehydratedItem = await session.Events.LoadAsync<InventoryItemAggregate>(streamId1);

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
