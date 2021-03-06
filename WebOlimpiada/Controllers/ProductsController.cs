﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebOlimpiada.Data;
using WebOlimpiada.Helpers;
using WebOlimpiada.ViewModels;

namespace WebOlimpiada.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("{page}")]
        public IActionResult GetProducts(int page=1)
        {
            var query = _context.Products.AsQueryable();
            //Thread.Sleep(2000);
            int pageSize = 4;
            int pageNo = page - 1;

            var products = query
                .OrderBy(p => p.Name)
                .Skip(pageNo * pageSize)
                .Take(pageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category.Name
                })
                .ToList();

            int allCount = query.Count();
            ProductPageModel model = new ProductPageModel
            {
                Data = products,
                CurrentPage = page,
                TotalPage = (int)Math.Ceiling((double)allCount / pageSize)
            };

            return Ok(model);
        }

        [HttpGet("edit/{id}")]
        public IActionResult GetEditProductById(int id)
        {
            var model = _context.Products
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category.Name
                }).SingleOrDefault(prod=>prod.Id==id);

            if (model == null)
                return NotFound();          
   
            return Ok(model);
        }

        [HttpPost]
        public IActionResult AddProduct([FromBody]ProductAddViewModel model)
        {
            Thread.Sleep(3000);
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }
            try
            {
                var cat = _context.Categories.SingleOrDefault(c => c.Name == model.Category);
                if(cat==null)
                {
                    cat = new Category
                    {
                        Name = model.Category
                    };
                    _context.Categories.Add(cat);
                    _context.SaveChanges();
                }
                var product = new Product
                {
                    CategoryId=cat.Id,
                    Name=model.Name
                };
                _context.Products.Add(product);
                _context.SaveChanges();
                var result = new ProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = cat.Name
                };
                return Ok(result);
            }
            catch
            {
                return BadRequest(new { invalid = "Помилка збереження даних!" });
            }
        }

        [HttpPut]
        public IActionResult EditProduct([FromBody]ProductEditViewModel model)
        {
            Thread.Sleep(3000);
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }
           

            if (model == null)
                return NotFound();
            try
            {
                var cat = _context.Categories.SingleOrDefault(c => c.Name == model.Category);
                if (cat == null)
                {
                    cat = new Category
                    {
                        Name = model.Category
                    };
                    _context.Categories.Add(cat);
                    _context.SaveChanges();
                }
                var product = _context.Products.SingleOrDefault(c => c.Id == model.Id);
                if(product!=null)
                {
                    product.Name = model.Name;
                    product.CategoryId = cat.Id;
                    _context.SaveChanges();
                    var result = new ProductViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Category = cat.Name
                    };
                    return Ok(result);
                }
                else
                    return NotFound();
            }
            catch
            {
                return BadRequest(new { invalid = "Помилка збереження даних!" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            Thread.Sleep(3000);
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }


            if (id == 0)
                return NotFound();
            try
            {
                
                var product = _context.Products.SingleOrDefault(c => c.Id == id);
                if (product != null)
                {
                    _context.Remove(product);
                    _context.SaveChanges();
                    var result = new ProductViewModel
                    {
                        Id = product.Id
                    };
                    return Ok(result);
                }
                else
                    return NotFound();
            }
            catch
            {
                return BadRequest(new { invalid = "Помилка збереження даних!" });
            }
        }
    }
}