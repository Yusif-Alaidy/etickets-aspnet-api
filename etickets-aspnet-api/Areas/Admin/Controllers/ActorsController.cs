using etickets_aspnet_api.Areas.Admin.DTOs.Request;
using etickets_aspnet_api.Areas.Admin.DTOs.Response;
using etickets_aspnet_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
namespace etickets_aspnet_api.Areas.Admin.Controllers
{
    [ApiController]
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [Authorize(Roles = $"{SD.AdminRole}, {SD.SuperAdminRole}")]
    public class ActorsController : ControllerBase
    {
        #region Fields
        private readonly IRepository<Actor> _repository;
        #endregion

        #region Constructor
        public ActorsController(IRepository<Actor> repo)
        {
            _repository = repo;
        }
        #endregion

        #region Get All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var actors = await _repository.GetAsync();
            if (actors is null)
                return NotFound(new { message = "No actors found" });

            var actorsDTO = actors.Select(e => new ActorsResponse
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                ProfilePicture = e.ProfilePicture,
                Bio = e.Bio,
                News = e.News,
                Movies = e.Movies.Select(e => new MoviesResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Price = e.Price,
                    ImgUrl = e.ImgUrl,
                    Quantity = e.Quantity,
                    TrailerUrl = e.TrailerUrl,
                    EndDate = e.EndDate,
                    StartDate = e.StartDate,
                    MovieStatus = e.MovieStatus,
                    CategoryName = e.Category.Name,
                    CinemaName = e.Cinema.Name,
                }).ToList(),

            });
            return Ok(actorsDTO);
        }
        #endregion

        #region Get By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(ActorIdRequest request)
        {
            var actor = await _repository.GetOneAsync(c => c.Id == request.Id);
            if (actor is null)
                return NotFound(new { message = $"Actor with id {request} not found" });

            var actorsDTO = new ActorsResponse
            {
                Id = actor.Id,
                FirstName = actor.FirstName,
                LastName = actor.LastName,
                ProfilePicture = actor.ProfilePicture,
                Bio = actor.Bio,
                News = actor.News,
                Movies = actor.Movies.Select(e => new MoviesResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Price = e.Price,
                    ImgUrl = e.ImgUrl,
                    Quantity = e.Quantity,
                    TrailerUrl = e.TrailerUrl,
                    EndDate = e.EndDate,
                    StartDate = e.StartDate,
                    MovieStatus = e.MovieStatus,
                    CategoryName = e.Category.Name,
                    CinemaName = e.Cinema.Name,
                }).ToList(),

            };

            return Ok(actorsDTO);
        }
        #endregion

        #region Create
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ActorRequest actorDTO, IFormFile profilePicture)
        {

            if (profilePicture == null || profilePicture.Length == 0)
                return BadRequest(new { message = "Profile picture is required" });

            // Save file
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePicture.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await profilePicture.CopyToAsync(stream);
            }

            actorDTO.ProfilePicture = fileName;
            var actor = new Actor
            {
                FirstName = actorDTO.FirstName,
                LastName = actorDTO.LastName,
                ProfilePicture = actorDTO.ProfilePicture,
                Bio = actorDTO.Bio,
                News = actorDTO.News,

            };

            await _repository.AddAsync(actor);
            await _repository.CommitAsync();

            return Ok(new { message = "Actor created successfully", actor });
        }
        #endregion

        //#region Update
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(ActorIdRequest request, [FromBody] Actor actor)
        //{
        //    if (request.Id != actor.Id)
        //        return BadRequest(new { message = "ID mismatch" });

        //    var existingActor = await _repository.GetOneAsync(c => c.Id == request.Id);
        //    if (existingActor is null)
        //        return NotFound(new { message = $"Actor with id {request} not found" });

        //    await _repository.Update(actor);
        //    await _repository.CommitAsync();

        //    var actorDTO = new Actor
        //    {
        //        FirstName = existingActor.FirstName,
        //        LastName = existingActor.LastName,
        //        ProfilePicture = existingActor.ProfilePicture,
        //        Bio = existingActor.Bio,
        //        News = existingActor.News,

        //    };

        //    return Ok(new { message = "Actor updated successfully", actorDTO });
        //}
        //#endregion

        #region Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(ActorIdRequest request)
        {
            var actor = await _repository.GetOneAsync(e => e.Id == request.Id);
            if (actor is null)
                return NotFound(new { message = $"Actor with id {request} not found" });

            await _repository.DeleteAsync(actor);
            await _repository.CommitAsync();

            return Ok(new { message = "Actor deleted successfully" });
        }
        #endregion
    }
}

