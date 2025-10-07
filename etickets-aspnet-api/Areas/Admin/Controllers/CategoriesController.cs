using etickets_aspnet_api.Areas.Admin.DTOs.Response;
using etickets_aspnet_api.Areas.Admin.DTOs.Request;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace etickets_aspnet_api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.AdminRole}, {SD.SuperAdminRole}")]
    public class CategoriesController : ControllerBase
    {
        #region Fields & Constructore
        private readonly IRepository<Category> _repository;
        public CategoriesController(IRepository<Category> repository)
        {
            _repository = repository;
        }
        #endregion

        #region Get All Categories
        // GET: api/admin/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _repository.GetAsync();
            if (categories == null || !categories.Any())
                return NotFound(new { message = "No categories found." });

            var categoryDTO = categories.Adapt<CategoryResponse>();
            return Ok(categoryDTO);
        }
        #endregion

        #region Get One Category
        // GET: api/admin/categories/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(CategoryIdRequest request)
        {
            var category = await _repository.GetOneAsync(e => e.Id == request.Id);
            if (category == null)
                return NotFound(new { message = $"Category with id {request} not found." });

            var categoryDTO = category.Adapt<CategoryResponse>();
            return Ok(categoryDTO);
        }
        #endregion

        #region Create
        // POST: api/admin/categories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequest category)
        {
            var cat = new Category()
            {
                Name = category.Name,
            };
            await _repository.AddAsync(cat);
            await _repository.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
        }
        #endregion

        #region Update
        // PUT: api/admin/categories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            if (id != category.Id)
                return BadRequest(new { message = "Category ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCategory = await _repository.GetOneAsync(e => e.Id == id);
            if (existingCategory == null)
                return NotFound(new { message = $"Category with id {id} not found." });

            await _repository.Update(category);
            await _repository.CommitAsync();

            return Ok(new { message = "Category updated successfully.", category });
        }
        #endregion

        #region Delete
        // DELETE: api/category/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _repository.GetOneAsync(e => e.Id == id);
            if (category == null)
                return NotFound(new { message = $"Category with id {id} not found." });

            await _repository.DeleteAsync(category);
            await _repository.CommitAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
        #endregion
    }
}
