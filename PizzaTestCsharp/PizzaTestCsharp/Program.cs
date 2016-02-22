using System;
using System.Collections.Generic;
using System.Text;

namespace RefactoringChallenge
{
    class ProgramRefactored
    {
        static void Main(string[] args)
        {
            var order = new Tuple<string, List<Product>>("John Doe",
                new List<Product>
                {
                    new Product
                    {
                        ProductName = "Pulled Pork",
                        Price = 6.99m,
                        Weight = 0.5m,
                        PricingMethod = Product.PricingMethods.PerPound, // uses constant
                    },
                    new Product
                    {
                        ProductName = "Coke",
                        Price = 3m,
                        Quantity = 2,
                        PricingMethod = Product.PricingMethods.PerItem, // uses constant
                    }
                }
            );

            // Order Processing logic was moved to a dedicated class.
            // This calling method should ideally be thin and simple.
            var orderProcessor = new OrderProcessor();
            orderProcessor.Process(order);

            Console.WriteLine(orderProcessor.OrderSummary);
            Console.WriteLine("Total Price: {0:C}", orderProcessor.TotalPrice);

            Console.ReadKey();
        }
    }

    /// <summary>
    /// Move business logic for processing orders into a specific class, allowing for 
    /// single-responsibility and better testability through unit and functional tests.
    /// 
    /// This class can also be implemented and re-used in a variety of different contexts
    /// such as a class library, web application, testing framework, etc.
    /// </summary>
    public class OrderProcessor
    {
        // Public properties to expose the summarized data
        public decimal TotalPrice { get; set; }
        public string OrderSummary { get; set; }
        public StringBuilder sbOrderSummary { get; private set; }

        public void Process(Tuple<string, List<Product>> order)
        {
            // Assign meaningful names, because otherwise Item1 & Item2 are like "magic numbers"
            var customerName = order.Item1;
            var orderProducts = order.Item2;

            TotalPrice = 0m;
            
            // Use a string builder for more efficient memory use than concatenating strings
            sbOrderSummary = new StringBuilder();
            sbOrderSummary.AppendFormat("ORDER SUMMARY FOR {0}:{1}", customerName, Environment.NewLine);

            foreach (var orderProduct in orderProducts)
            {
                ProductProcessorBase orderState = null;

                switch (orderProduct.PricingMethod)
                {
                    case Product.PricingMethods.PerPound:
                        orderState = new PerPoundProcessor();
                        break;
                    case Product.PricingMethods.PerItem:
                        orderState = new PerItemProcessor();
                        break;
                }

                orderState.Process(this, orderProduct);
            }

            OrderSummary = sbOrderSummary.ToString();
        }
    }

    public class Product
    {
        // PerPound and PerItem were moved into constants for 
        // better reliability and to reduce suprises at runtime
        public static class PricingMethods { 
            public const string PerPound = "PerPound";
            public const string PerItem = "PerItem";
        }

        public string ProductName;
        public decimal Price;
        public decimal? Weight;
        public int? Quantity;
        public string PricingMethod;
    }

    public abstract class ProductProcessorBase
    {
        protected void Log(string logMsg)
        {
            // TODO: Use a logger such as log4net, nlog, etc.
            // TODO: Expand upon this method to support types of logs such as Warn, Error, Info, etc
        }

        public decimal ProductPrice { get; set; }

        public virtual void Process(OrderProcessor orderProcessor, Product orderProduct)
        {
        }
    }

    public class PerPoundProcessor : ProductProcessorBase
    {
        public override void Process(OrderProcessor orderProcessor, Product orderProduct)
        {
            if (orderProduct.Weight != null && orderProduct.Weight >= 0)
            {
                ProductPrice = orderProduct.Weight.Value * orderProduct.Price;
                orderProcessor.TotalPrice += ProductPrice;
                orderProcessor.sbOrderSummary.AppendFormat("{0} {1:C} ({2}  pounds at {3:C} per pound){4}", orderProduct.ProductName, ProductPrice, orderProduct.Weight, orderProduct.Price, Environment.NewLine);
            }
            else
            {
                // If the PricingMethod has been correctly determined, but the corresponding property is unavailable or invalid
                // we can't proceed, so we need to log this and throw an exception, preventing the order from completing.
                const string errorMessage = "Order cannot be processed. PricingMethod set to 'PerPound', but Weight is missing.";
                Log(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }

    public class PerItemProcessor : ProductProcessorBase
    {
        public override void Process(OrderProcessor orderProcessor, Product orderProduct)
        {
            if (orderProduct.Quantity != null && orderProduct.Quantity >= 0)
            {
                ProductPrice = orderProduct.Quantity.Value * orderProduct.Price;
                orderProcessor.TotalPrice += ProductPrice;
                orderProcessor.sbOrderSummary.AppendFormat("{0} {1:C} ({2} items at {3:C} each){4}", orderProduct.ProductName, ProductPrice, orderProduct.Quantity, orderProduct.Price, Environment.NewLine);
            }
            else
            {
                const string errorMessage = "Order cannot be processed. PricingMethod set to 'PerItem', but Quantity is missing.";
                Log(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }

}

// For reference the original code is below:

namespace Refactoring
{
    class Program
    {
        static void Main(string[] args)
        {
            var order = new Tuple<string, List<Product>>("John Doe",
                new List<Product>
                {
                    new Product
                    {
                        ProductName = "Pulled Pork",
                        Price = 6.99m,
                        Weight = 0.5m,
                        PricingMethod = "PerPound",
                    },
                    new Product
                    {
                        ProductName = "Coke",
                        Price = 3m,
                        Quantity = 2,
                        PricingMethod = "PerItem"
                    }
                }
            );
 
            var price = 0m;
            var orderSummary = "ORDER SUMMARY FOR " + order.Item1 + ": \r\n";
            
            foreach (var orderProduct in order.Item2)
            {
                var productPrice = 0m;
                orderSummary += orderProduct.ProductName;
 
                if (orderProduct.PricingMethod == "PerPound")
                {
                    productPrice = (orderProduct.Weight.Value * orderProduct.Price);
                    price += productPrice;
                    orderSummary += (" $" + productPrice + " (" + orderProduct.Weight + " pounds at $" + orderProduct.Price + " per pound)");
                }
                else // Per item
                {
                    productPrice = (orderProduct.Quantity.Value * orderProduct.Price);
                    price += productPrice;
                    orderSummary += (" $" + productPrice + " (" + orderProduct.Quantity + " items at $" + orderProduct.Price + " each)");
                }
 
                orderSummary += "\r\n";
            }
            
            Console.WriteLine(orderSummary);
            Console.WriteLine("Total Price: $" + price);
 
            Console.ReadKey();
        }
    }
 
    public class Product
    {
        public string ProductName;
        public decimal Price;
        public decimal? Weight;
        public int? Quantity;
        public string PricingMethod;
    }
}