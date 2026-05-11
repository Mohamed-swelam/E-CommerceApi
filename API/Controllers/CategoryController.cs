using Core.DTOs.Category;
using Core.Interfaces.IRepositories;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoryController(ICategoryService service)
        {
            _service = service;
        }
        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var response = await _service.GetAllCategoriesAsync();
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        // Get: api/Category/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var response = await _service.GetCategoryByIdAsync(id);
            if (!response.IsSuccess)
                return NotFound(response);
            return Ok(response);
        }
        // Post: api/Category
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDto dto)
        {
            var response = await _service.AddCategoryAsync(dto);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        // Put: api/Category
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            var response = await _service.UpdateCategoryAsync(dto);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
        //Delete: api/Category/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var response = await _service.RemoveCategoryAsync(id);
            if (!response.IsSuccess)
                return BadRequest(response);
            return Ok(response);
        }
    }
}