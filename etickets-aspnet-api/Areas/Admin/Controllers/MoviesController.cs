using etickets_aspnet_api.Areas.Admin.DTOs.Request;
using etickets_aspnet_api.Areas.Admin.DTOs.Response;
using etickets_aspnet_api.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            var moviesDTO = movies.Select(e => new MoviesResponse
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price,
                ImgUrl = e.ImgUrl,
                Quantity = e.Quantity,
                TrailerUrl = e.TrailerUrl,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                MovieStatus  = e.MovieStatus,
                CinemaName  = e.Cinema.Name,
                CategoryName = e.Category.Name
            });
            return Ok(moviesDTO);
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

            var moviesDTO = new MoviesResponse
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                Price = movie.Price,
                ImgUrl = movie.ImgUrl,
                Quantity = movie.Quantity,
                TrailerUrl = movie.TrailerUrl,
                StartDate = movie.StartDate,
                EndDate = movie.EndDate,
                MovieStatus  = movie.MovieStatus,
                CinemaName  = movie.Cinema.Name,
                CategoryName = movie.Category.Name
            };
            return Ok(moviesDTO);
        }
        #endregion

        #region Create
        // POST: api/admin/movies
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MovieRequest request, [FromForm] IFormFile? ImgUrl, [FromForm] List<int>? ActorIds)
        {
            
            var movie = new Movie
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImgUrl = request.ImgUrl,
                Quantity = request.Quantity,
                TrailerUrl = request.TrailerUrl,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CategoryId = request.CategoryId,
                CinemaId = request.CinemaId
            };

            // Upload image if provided
            if (ImgUrl is not null && ImgUrl.Length > 0)
                movie.ImgUrl = await SaveImageAsync(ImgUrl);

            // Link actors
            if (request.ActorsIds.Any())
            {
                var actors = await _repositoryActor.GetAsync(a => request.ActorsIds.Contains(a.Id));
                foreach (var actor in actors)
                    movie.Actors.Add(actor);
            }

            await _repository.AddAsync(movie);
            await _repository.CommitAsync();

            var category = await _repositoryCategory.GetOneAsync(e => e.Id == request.CategoryId);
            var cinema = await _repositoryCinema.GetOneAsync(e => e.Id == request.CinemaId);
            var movieDTO = new MoviesResponse
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                Price = movie.Price,
                ImgUrl = movie.ImgUrl,
                Quantity = movie.Quantity,
                TrailerUrl = movie.TrailerUrl,
                StartDate = movie.StartDate,
                EndDate = movie.EndDate,
                CategoryName = category.Name,
                CinemaName = cinema.Name,
                ActorsIds = request.ActorsIds

            };
            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, new
            {
                message = "Movie created successfully.",
                data = movieDTO
            });
        }
        #endregion

        #region Update
        // PUT: api/admin/movies/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] Movie movie, [FromForm] IFormFile? ImgUrl, [FromForm] List<int>? ActorIds)
        {
            
            var dbMovie = await _repository.GetOneAsync(e => e.Id == id, include: [e => e.Actors!, e=>e.Category, e=>e.Cinema]);
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
            //if (ActorIds != null && ActorIds.Any())
            //{
            //    dbMovie.Actors = new List<Actor>();
            //    foreach (var idActor in ActorIds)
            //    {
            //        var actor = await _repositoryActor.GetOneAsync(e => e.Id == idActor);
            //        if (actor != null)
            //            dbMovie.Actors.Add(actor);
            //    }
            //}
            var actors = await _repositoryActor.GetAsync(a => ActorIds.Contains(a.Id));
            dbMovie.Actors = actors.ToList();

            await _repository.Update(dbMovie);
            await _repository.CommitAsync();

            var ActorsIds = movie.Actors.Select(a => a.Id).ToList();
            var movieDTO = new MoviesResponse
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                Price = movie.Price,
                ImgUrl = movie.ImgUrl,
                Quantity = movie.Quantity,
                TrailerUrl = movie.TrailerUrl,
                StartDate = movie.StartDate,
                EndDate = movie.EndDate,
                CategoryName = movie.Category.Name,
                CinemaName = movie.Cinema.Name,
                ActorsIds = ActorsIds

            };

            return Ok(new { message = "Movie updated successfully.", data = movieDTO });
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
