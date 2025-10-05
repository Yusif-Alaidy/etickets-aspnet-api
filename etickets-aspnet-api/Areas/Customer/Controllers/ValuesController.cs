using etickets_aspnet_api.Areas.Customer.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace etickets_aspnet_api.Areas.Customer.Controllers
{
    [Area(SD.CustomerRole)]
    [Route("api/customer/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        #region Fields
        private readonly IRepository<Movie> movieRepository;
        private readonly IRepository<Cinema> cinemaRepository;
        private readonly IRepository<Category> categoryRepository;
        private readonly IRepository<Actor> actorRepository;
        #endregion

        #region Constructor
        public ValuesController(IRepository<Movie> repositoryMovie, IRepository<Cinema> repositoryCinema, IRepository<Category> repositoryCategory, IRepository<Actor> repositoryActor)
        {
            this.movieRepository = repositoryMovie;
            this.categoryRepository = repositoryCategory;
            this.actorRepository = repositoryActor;
            this.cinemaRepository = repositoryCinema;

        }
        #endregion

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index(FilterRequest request, int page = 1)
        {

            // Base data
            var categories = await categoryRepository.GetAsync();
            var cinemas = await cinemaRepository.GetAsync();
            var movies = await movieRepository.GetAsync(include: [e => e.Category! , e => e.Cinema!]);
            var commingSoon = await movieRepository.GetAsync(e=>e.StartDate > DateTime.UtcNow ,include: [e => e.Category!, e => e.Cinema!]);

            if (movies is null) return NotFound();

            #region Filtering
            if (request.search is not null)
            {

                movies = await movieRepository.GetAsync(e => e.Name.Contains(request.search));
            }

            if (request.minPrice is not null)
            {
                movies = await movieRepository.GetAsync(e => e.Price >= request.minPrice);
            }

            if (request.maxPrice is not null)
            {
                movies = await movieRepository.GetAsync(e => e.Price <= request.maxPrice);
            }

            if (request.categoryId is not null)
            {
                movies = await movieRepository.GetAsync(e => e.Category.Id == request.categoryId);
            }

            if (request.cinemaId is not null)
            {
                movies = await movieRepository.GetAsync(e => e.Cinema!.Id == request.cinemaId);
            }
            #endregion

            #region Pagination
            var totalNumberOfPages = Math.Ceiling(movies.Count() / 10.0);
            var currentPage = page;

            movies = movies.Skip(( page - 1 ) * 10).Take(10).ToList();

            var movieDTO = movies.Select(e=> new MoviesResponse 
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
            });
            var categoriesDTO = categories.Select(e => new CategoriesResponse
            {
                Name=e.Name,
            }); 
            var cinemasDTO = cinemas.Select(e => new CinemasResponse
            {
                Name=e.Name,
            }); 
            #endregion
  

            return Ok(new
            {
              movieDTO,
              request.search,
              request.minPrice,
              request.maxPrice,
              request.categoryId,
              request.cinemaId,
              categoriesDTO,
               cinemasDTO,
              currentPage,
              totalNumberOfPages,
            });
        }
        #endregion

        #region Actor
        [HttpGet("actore/{id}")]
        public async Task<IActionResult> Actor(int id)
        {
            
            var actor = await actorRepository.GetOneAsync(e => e.Id == id);
            if (actor == null) return NotFound();

            var Movies = await movieRepository.GetAsync(e => e.Actors!.Any(e => e.Id == actor.Id));

            var actorDTO = new ActorResponse
            {
                Id = actor.Id,
                FirstName = actor.FirstName,
                LastName = actor.LastName,
                ProfilePicture = actor.ProfilePicture,
                Bio = actor.Bio,
                News = actor.News,
            };

            var movieDTO = Movies.Select(e => new MoviesResponse
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

            });
            return Ok(new {actorDTO, movieDTO});
        }
        #endregion

        #region Movie

        [HttpGet("movie/{id}")]
        public async Task<IActionResult> Movie(int id)
        {
            
            var movie = await movieRepository.GetOneAsync(e => e.Id == id, include: [e=>e.Category!,e=>e.Cinema!,e=>e.Actors!]);

            if (movie is null) return NotFound();


            var similerMovie = await movieRepository.GetAsync(e => e.Category == movie.Category && e.Id != movie.Id);

            var movieDTO = new MoviesResponse
            {
                Id = movie.id,
                Name = movie.Name,
                Description = movie.Description,
                Price = movie.Price,
                ImgUrl = movie.ImgUrl,
                Quantity = movie.Quantity,
                TrailerUrl = movie.TrailerUrl,
                EndDate = movie.EndDate,
                StartDate = movie.StartDate,
                MovieStatus = movie.MovieStatus,
                CategoryName = movie.Category.Name,
                CinemaName = movie.Cinema.Name,

            };
            var similerMovieDTO = similerMovie.Select(e => new MoviesResponse
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

            });


            return Ok(new
            {
                movieDTO,
                similerMovieDTO,
            });
        }
        #endregion
    }
}
