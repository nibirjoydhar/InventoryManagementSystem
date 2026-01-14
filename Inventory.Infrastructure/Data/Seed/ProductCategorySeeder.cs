using Inventory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.Infrastructure.Data.Seed
{
    public static class ProductCategorySeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Check if already seeded
            if (context.Categories.Any() || context.Products.Any())
                return;

            // ========================
            // Seed Categories
            // ========================
            var categories = new List<Category>
            {
                new Category { Name = "Electronics" },
                new Category { Name = "Books" },
                new Category { Name = "Clothing" },
                new Category { Name = "Sports" },
                new Category { Name = "Home & Kitchen" },
                new Category { Name = "Toys & Games" },
                new Category { Name = "Beauty & Health" },
                new Category { Name = "Automotive" },
                new Category { Name = "Office Supplies" },
                new Category { Name = "Garden & Outdoor" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            // ========================
            // Seed Products
            // ========================
            var random = new Random();
            var products = new List<Product>();

            for (int i = 1; i <= 100; i++)
            {
                var category = categories[random.Next(categories.Count)];
                products.Add(new Product
                {
                    Name = $"Product {i}",
                    Description = $"Description for Product {i}",
                    Price = Math.Round((decimal)(random.NextDouble() * 1000), 2),
                    StockQuantity = random.Next(1, 100),
                    Status = Domain.Enums.ProductStatus.Available,
                    CategoryId = category.Id
                });
            }

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
