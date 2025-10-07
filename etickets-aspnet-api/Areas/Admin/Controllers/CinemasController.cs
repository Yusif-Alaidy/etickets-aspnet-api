using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace etickets_aspnet_api.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.AdminRole}, {SD.SuperAdminRole}")]
    public class CinemasController : ControllerBase
    {
        #region Fields & Constructor
        private readonly IRepository<Cinema> _repository;

        public CinemasController(IRepository<Cinema> repository)
        {
            _repository = repository;
        }
        #endregion

        #region Get All
        // GET: api/admin/cinemas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cinemas = await _repository.GetAsync();
            if (cinemas is null || !cinemas.Any())
                return NotFound(new { message = "No cinemas found." });

            return Ok(cinemas);
        }
        #endregion

        #region Get One
        // GET: api/admin/cinemas/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cinema = await _repository.GetOneAsync(e => e.Id == id);
            if (cinema is null)
                return NotFound(new { message = $"Cinema with ID {id} not found." });

            return Ok(cinema);
        }
        #endregion

        #region Create
        // POST: api/admin/cinemas
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Cinema cinema, IFormFile cinemaLogo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (cinemaLogo is null)
                return BadRequest(new { message = "Cinema logo is required." });

            // Save logo to wwwroot/images
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cinemaLogo.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await cinemaLogo.CopyToAsync(stream);
            }

            cinema.CinemaLogo = fileName;

            await _repository.AddAsync(cinema);
            await _repository.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = cinema.Id }, new
            {
                message = "Cinema created successfully.",
                data = cinema
            });
        }
        #endregion

        #region Update
        // PUT: api/admin/cinemas/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] Cinema cinema, IFormFile? cinemaLogo)
        {
            if (id != cinema.Id)
                return BadRequest(new { message = "Cinema ID mismatch." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cinemaInDb = await _repository.GetOneAsync(e => e.Id == id);
            if (cinemaInDb is null)
                return NotFound(new { message = $"Cinema with ID {id} not found." });

            // Handle new logo upload
            if (cinemaLogo is not null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cinemaLogo.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await cinemaLogo.CopyToAsync(stream);
                }

                // Delete old file if exists
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", cinemaInDb.CinemaLogo ?? "");
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                cinema.CinemaLogo = fileName;
            }
            else
            {
                cinema.CinemaLogo = cinemaInDb.CinemaLogo;
            }

            await _repository.Update(cinema);
            await _repository.CommitAsync();

            return Ok(new { message = "Cinema updated successfully.", data = cinema });
        }
        #endregion

        #region Delete
        // DELETE: api/admin/cinemas/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _repository.GetOneAsync(e => e.Id == id);
            if (cinema is null)
                return NotFound(new { message = $"Cinema with ID {id} not found." });

            // Delete logo file
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", cinema.CinemaLogo ?? "");
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            await _repository.DeleteAsync(cinema);
            await _repository.CommitAsync();

            return Ok(new { message = "Cinema deleted successfully." });
        }
        #endregion
    }
}
