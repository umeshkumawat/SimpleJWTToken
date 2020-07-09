using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleJWTToken.Data;
using SimpleJWTToken.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJWTToken.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class GroceryListController : Controller
    {
        private readonly GroceryListContext _dbContext;

        public GroceryListController(GroceryListContext dbContext)
        {
            _dbContext = dbContext;

            if(_dbContext.GroceryList.Count() == 0)
            {
                _dbContext.GroceryList.Add(new GroceryItem { Description = "Noodles" });
                _dbContext.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<GroceryItem> GetAll()
        {
            return _dbContext.GroceryList.ToList();
        }

        [HttpGet("{id}", Name = "GetGroceryItem")]
        public IActionResult GetById(int id)
        {
            var item = _dbContext.GroceryList.FirstOrDefault(g => g.Id == id);
            if (item == null)
                return NotFound();
            else
                return new ObjectResult(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] GroceryItem item)
        {
            if (item == null)
                return BadRequest();

            _dbContext.GroceryList.Add(item);
            _dbContext.SaveChanges();

            return CreatedAtRoute("GetGroceryItem", new { id = item.Id }, item);
        }
    }
}
