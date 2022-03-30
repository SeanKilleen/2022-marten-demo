using System;

namespace MartenDemo
{
    public class InventoryItemAggregate
    {
        public int RemovalEvents { get; set; } = 0;
        public int AddedEvents { get; set; } = 0;
        public int CurrentQuantity { get; set; } = 0;

        public Guid Id { get; set; }

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
}
