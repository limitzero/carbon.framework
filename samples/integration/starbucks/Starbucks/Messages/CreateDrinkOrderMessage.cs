using System.Collections.Generic;
using Carbon.Core.Stereotypes.For.Components.Message;

namespace Starbucks.Messages
{
    [Message]
    public class CreateDrinkOrderMessage
    {
        private List<Drink> _drinks;
        public List<Drink> Drinks
        {
            get { return _drinks; }
            set { _drinks = value; }
        }

        public CreateDrinkOrderMessage()
        {
            _drinks = new List<Drink>();
        }

        public void AddDrink(string name, DrinkSize size, bool isIced, int total)
        {
            _drinks.Add(new Drink(name, size, isIced, total));
        }

        public void AddDrink(string name, DrinkSize size, bool isIced)
        {
            _drinks.Add(new Drink(name, size, isIced));
        }
    }
}