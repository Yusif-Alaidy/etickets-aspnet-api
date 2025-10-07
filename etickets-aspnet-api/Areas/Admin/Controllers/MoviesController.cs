using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace etickets_aspnet_api.Areas.Admin.Controllers
{
    [Area(SD.AdminArea)]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.AdminRole}, {SD.SuperAdminRole}")]
    public class MoviesController : ControllerBase
    {
        #region Fields & Constructor
        private readonly IRepository<Movie> _repository;
        private readonly IRepository<Category> _repositoryCategory;
        private readonly IRepository<Cinema> _repositoryCinema;
        private readonly IRepository<Actor> _repositoryActor;

        public MoviesController(
            IRepository<Movie> repository,
            IRepository<Category> repositoryCategory,
            IRepository<Cinema> repositoryCinema,
            IRepository<Actor> repositoryActor)
        {
            _repository = repository;
            _repositoryCategory = repositoryCategory;
            _repositoryCinema = repositoryCinema;
            _repositoryActor = repositoryActor;
        }
        #endregion

        #region Get All
        // GET: api/admin/movies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _repository.GetAsync(include: [e => e.Category!, e => e.Cinema!]);
            if (movies is null || !movies.Any())
                return NotFound(new { message = "No movies found." });

            return Ok(movies);
        }
        #endregion

        #region Get One
        // GET: api/admin/movies/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _repository.GetOneAsync(e => e.Id == id, include: [e => e.Category!, e => e.Cinema!, e => e.Actors!]);
            if (movie is null)
                return NotFound(new { message = $"Movie with ID {id} not found." });

            return Ok(movie);
        }
        #endregion

        #region Create
        // POST: api/admin/movies
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] Movie movie, [FromForm] IFormFile? ImgUrl, [FromForm] List<int>? ActorIds)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Upload image if provided
            if (ImgUrl is not null && ImgUrl.Length > 0)
                movie.ImgUrl = await SaveImageAsync(ImgUrl);

            // Link actors
            if (ActorIds != null && ActorIds.Any())
            {
                movie.Actors = new List<Actor>();
                foreach (var id in ActorIds)
                {
                    var actor = await _repositoryActor.GetOneAsync(e => e.Id == id);
                    if (actor != null)
                        movie.Actors.Add(actor);
                }
            }

            await _repository.AddAsync(movie);
            await _repository.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, new
            {
                message = "Movie created successfully.",
                data = movie
            });
        }
        #endregion

        #region Update
        // PUT: api/admin/movies/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] Movie movie, [FromForm] IFormFile? ImgUrl, [FromForm] List<int>? ActorIds)
        {
            if (id != movie.Id)
                return BadRequest(new { message = "Movie ID mismatch." });

            var dbMovie = await _repository.GetOneAsync(e => e.Id == id, include: [e => e.Actors!]);
            if (dbMovie is null)
                return NotFound(new { message = $"Movie with ID {id} not found." });

            // Update basic info
            dbMovie.Name = movie.Name;
            dbMovie.Description = movie.Description;
            dbMovie.Price = movie.Price;
            dbMovie.TrailerUrl = movie.TrailerUrl;
            dbMovie.StartDate = movie.StartDate;
            dbMovie.EndDate = movie.EndDate;
            dbMovie.MovieStatus = movie.MovieStatus;
            dbMovie.CategoryId = movie.CategoryId;
            dbMovie.CinemaId = movie.CinemaId;

            // Update image if new one uploaded
            if (ImgUrl is not null)
            {
                var newFile = await SaveImageAsync(ImgUrl);

                // Delete old image if exists
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", dbMovie.ImgUrl ?? "");
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                dbMovie.ImgUrl = newFile;
            }

            // Update actors (optional)
            if (ActorIds != null && ActorIds.Any())
            {
                dbMovie.Actors = new List<Actor>();
                foreach (var idActor in ActorIds)
                {
                    var actor = await _repositoryActor.GetOneAsync(e => e.Id == idActor);
                    if (actor != null)
                        dbMovie.Actors.Add(actor);
                }
            }

            await _repository.Update(dbMovie);
            await _repository.CommitAsync();

            return Ok(new { message = "Movie updated successfully.", data = dbMovie });
        }
        #endregion

        #region Delete
        // DELETE: api/admin/movies/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _repository.GetOneAsync(e => e.Id == id);
            if (movie == null)
                return NotFound(new { message = $"Movie with ID {id} not found." });

            // Delete image if exists
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", movie.ImgUrl ?? "");
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            await _repository.DeleteAsync(movie);
            await _repository.CommitAsync();

            return Ok(new { message = "Movie deleted successfully." });
        }
        #endregion

        #region Helpers
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
        #endregion
    }
}
