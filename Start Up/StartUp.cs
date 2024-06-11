using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext context = new ProductShopContext();
            //1.import users
            string userjson = File.ReadAllText("../../../Datasets/users.json");
            // Console.WriteLine(ImportUsers(context, userjson));

            //2. import products
            string productjson = File.ReadAllText("../../../Datasets/products.json");
            // Console.WriteLine(ImportProducts(context, productjson));

            //3.import category
            string categoryjson = File.ReadAllText("../../../Datasets/categories.json");
            //Console.WriteLine(ImportCategories(context, categoryjson));

            //4.import mapping table
            string categoriesproductjson = File.ReadAllText("../../../Datasets/categories-products.json");
            //Console.WriteLine(ImportCategoryProducts(context, categoriesproductjson));

           Console.WriteLine(GetUsersWithProducts(context));

            Console.WriteLine();
        }
        //Import users from json file
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<User[]>(inputJson);

            context.Users.AddRange(users);

            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }
        //Import products from json file

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson);


            context.Products.AddRange(products);

            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }
        //Import categories from json file

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<Category[]>(inputJson);

            var validcategories = categories?
                .Where(o => o.Name != null)
                .ToArray();

            if (validcategories != null)
            {
                context.Categories.AddRange(validcategories);

                context.SaveChanges();

                return $"Successfully imported {validcategories.Length}";
            }
            return $"Successfully imported 0";
        }
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);
            context.CategoriesProducts.AddRange(categories);

            context.SaveChanges();
            return $"Successfully imported {categories.Length}";
        }

        //мапинг таблицата e с един запис по-малко
        //5.export products in range - Serialize
        public static string GetProductsInRange(ProductShopContext context)
        {
            var productsinrange = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    seller = $"{p.Seller.FirstName} {p.Seller.LastName}"
                }).OrderBy(p => p.price)
                .ToArray();

            var json = JsonConvert.SerializeObject(productsinrange, Formatting.Indented);

            return json;
        }
        //6. Get sold products
        public static string GetSoldProducts(ProductShopContext context)
        {
            var solditems = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    soldProducts = u.ProductsSold
                    .Select(p => new
                    {
                        name = p.Name,
                        price = p.Price,
                        buyerFirstName = p.Buyer.FirstName,
                        buyerLastName = p.Buyer.LastName,

                    }).ToArray()
                }).ToArray();

            string json = JsonConvert.SerializeObject(solditems, Formatting.Indented);

            return json;
        }//7.get categories by products counts
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoriesProducts.Count(),
                    averagePrice = c.CategoriesProducts.Average(cp => cp.Product.Price).ToString("f2"),
                    totalRevenue = c.CategoriesProducts.Sum(p => p.Product.Price).ToString("f2")
                }).OrderByDescending(c => c.productsCount)
                .ToArray();

            string json = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return json;
        }
        //8 get users with products
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var userswithproducts = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = u.ProductsSold
                    .Where(b => b.BuyerId != null)
                    .Select(p => new
                    {
                        name = p.Name,
                        price = p.Price
                    }).ToArray()
                }).OrderByDescending(u => u.soldProducts.Count())
                .ToArray();

            var otuput = new
            {
                userCount = userswithproducts.Count(),
                users = userswithproducts.Select(u => new
                {
                    u.firstName,
                    u.lastName,
                    u.age,
                    soldProducts = new
                    {
                        count = u.soldProducts.Count(),
                        products = u.soldProducts
                    }
                })
            };
            // IGNORE ALL NULL VALUES
            string json = JsonConvert.SerializeObject(otuput, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            });
            return json;
        }
    }

}