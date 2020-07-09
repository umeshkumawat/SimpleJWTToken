using Microsoft.EntityFrameworkCore;
using SimpleJWTToken.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJWTToken.Data
{
    public class GroceryListContext : DbContext
    {
        public GroceryListContext(DbContextOptions<GroceryListContext> option)
            : base(option)
        {

        }

        public DbSet<GroceryItem> GroceryList { get; set; }
    }
}
