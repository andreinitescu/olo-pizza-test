using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PizzaTestCsharp
{
    class Topping
    {
        public string[] toppings { get; set; }
    }

    class ToppingCount
    {
        public Topping Topping { get; set; }
        public int Count { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var json = File.ReadAllText("App_Data/pizzas.json");
            var toppings = JsonConvert.DeserializeObject<Topping[]>(json);

            var toppingCounts = new List<ToppingCount>();

            foreach(var topping in toppings)
            {
                var found = false;

                if (topping.toppings.Length > 1) Array.Sort(topping.toppings);

                foreach(var toppingCount in toppingCounts)
                {
                    if (Enumerable.SequenceEqual(topping.toppings, toppingCount.Topping.toppings))
                    {
                        found = true;
                        toppingCount.Count++;
                        continue;
                    }
                }

                if (!found)
                {
                    toppingCounts.Add(new ToppingCount { 
                        Topping = topping,
                        Count = 1
                    });
                }
            }

            foreach(var toppingCount in toppingCounts.OrderBy(x => x.Count).Reverse().Take(20))
            {
                var sb = new StringBuilder();
                foreach (var topping in toppingCount.Topping.toppings)
                    sb.AppendFormat("{0}, ", topping);

                Console.WriteLine("{0}\t{1}", toppingCount.Count, sb.ToString().Trim().TrimEnd(','));
            }

            Console.ReadLine();
        }
    }
}
