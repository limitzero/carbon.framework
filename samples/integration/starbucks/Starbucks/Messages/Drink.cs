using Carbon.Core.Stereotypes.For.Components.Message;
using System;

namespace Starbucks.Messages
{
    [Message]
    public class Drink
    {
        public string Name { get; set; }
        public DrinkSize Size { get; set; }
        public bool IsIced { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public Drink()
        {    
        }

        public Drink(string name, DrinkSize size, bool isIced)
            : this(name, size, isIced, 1)
        {
        }

        public Drink(string name, DrinkSize size, bool isIced, int quantity)
        {
            Name = name;
            Size = size;
            IsIced = isIced;
            Quantity = quantity;
            GetPrice();
        }

        private void GetPrice()
        {
            var random = new Random();
            var markUp = (Int32)Size;
            Price = random.Next(8) + Name.Length + markUp;
        }
    }
}