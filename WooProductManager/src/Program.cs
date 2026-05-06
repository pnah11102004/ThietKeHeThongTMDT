using System;
using System.Text.Json;

namespace WooProductManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Woo Product Manager started...");

            var api = new WooApiClient(WooConfig.BaseUrl, WooConfig.ConsumerKey, WooConfig.ConsumerSecret);

            Console.WriteLine("1. Danh sách sản phẩm");
            Console.WriteLine("2. Thêm sản phẩm");
            Console.WriteLine("3. Sửa giá");
            Console.WriteLine("4. Xoá sản phẩm");
            Console.Write("Chọn: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine(api.GetProducts());
                    break;

                case "2":
                    var newProduct = new Product
                    {
                        name = "C# Product",
                        regular_price = "150000"
                    };
                    var json = JsonSerializer.Serialize(newProduct, new JsonSerializerOptions { PropertyNamingPolicy = null });
                    Console.WriteLine(api.CreateProduct(json));
                    break;

                case "3":
                    Console.Write("Product ID: ");
                    if (!int.TryParse(Console.ReadLine(), out int id))
                    {
                        Console.WriteLine("Invalid product ID");
                        break;
                    }
                    Console.Write("New Price: ");
                    string price = Console.ReadLine();
                    Console.WriteLine(api.UpdatePrice(id, price));
                    break;

                case "4":
                    Console.Write("Product ID: ");
                    if (!int.TryParse(Console.ReadLine(), out int delId))
                    {
                        Console.WriteLine("Invalid product ID");
                        break;
                    }
                    Console.WriteLine(api.DeleteProduct(delId));
                    break;

                default:
                    Console.WriteLine("No valid choice selected.");
                    break;
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}