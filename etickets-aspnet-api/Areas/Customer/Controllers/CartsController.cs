using etickets_aspnet_api.Areas.Customer.DTOs.Response;
using etickets_aspnet_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using etickets_aspnet_api.Areas.Customer.DTOs.Request;
using etickets_aspnet_api.Areas.Customer.DTOs.Response;

namespace etickets_aspnet_api.Areas.Customer.Controllers
{
    [Route("api/customer/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Customer")]
    public class CartsController : ControllerBase
    {
        #region Fields
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRepository<Cart> cartrepository;
        private readonly IRepository<Order> orderRepository;
        #endregion

        #region Constructore
        public CartsController(UserManager<ApplicationUser> userManager, IRepository<Cart> repositoryCart, IRepository<Order> repositoryOrder)
        {
            this.userManager = userManager;
            this.cartrepository = repositoryCart;
            this.orderRepository = repositoryOrder;
        }
        #endregion

        #region Home
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // get user
            var user = await userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            // get me all carts for this user
            var carts = await cartrepository.GetAsync(e => e.ApplicationUserId == user.Id, include: [e => e.Movie]);

            var totalAmount = carts.Sum(e => e.Movie.Price * e.Count);
            var cartDTO =  carts.Select(e=> new CartResponse
            {
                ApplicationUserId = e.ApplicationUserId,
                MovieId = e.MovieId,
                ApplicationUser = e.ApplicationUser,
                Count = e.Count,
            });
            return Ok(new
            {
                totalAmount,
                cartDTO,
            });
            
        }
        #endregion

        #region Add To Cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartRequest request)
        {
            // get user
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // check if this cart already exist
            var cart = await cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == request.MovieId);
            string msg = "";
            if (cart is not null)
            {
                cart.Count = + request.Count;
                msg = "Update to cart successfuly";
            }
            else
            {
                // create cart
                await cartrepository.AddAsync(new()
                {
                    ApplicationUserId = user.Id,
                    MovieId = request.MovieId,
                    Count = request.Count
                });
                msg = "Item add to cart successfuly";

            }

            await cartrepository.CommitAsync();

            return Ok(msg = msg);
        }
        #endregion

        #region IncrementCount
        [HttpPatch("IncrementCount")]
        public async Task<IActionResult> IncrementCount(MovieIdRequest MovieId)
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == MovieId.Id);

            if (cart is not null)
            {
                cart.Count += 1;
                await cartrepository.CommitAsync();
            }

            return Ok(new
            {
                msg = "Increment Count Successfully",
            });
        }
        #endregion

        #region DecrementCount
        [HttpPatch("DecrementCount")]
        public async Task<IActionResult> DecrementCount(MovieIdRequest MovieId)
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == MovieId.Id);

            if (cart is not null)
            {
                if (cart.Count > 1)
                {
                    cart.Count -= 1;
                    await cartrepository.CommitAsync();
                }
            }

            return Ok(new
            {
                msg = "Decrement Count Successfully",
            });
        }
        #endregion

        #region DeleteProductFromCart
        [HttpDelete]
        public async Task<IActionResult> DeleteProductFromCart(MovieIdRequest MovieId)
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var cart = await cartrepository.GetOneAsync(e => e.ApplicationUserId == user.Id && e.MovieId == MovieId.Id);

            if (cart is not null)
            {
                await cartrepository.DeleteAsync(cart);
                await cartrepository.CommitAsync();
            }
            return Ok(new
            {
                msg = "Delete Item Successfully",
            });
        }
        #endregion

        #region Cart
        [HttpGet("pay")]
        public async Task<IActionResult> Pay()
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
                return NotFound();

            var carts = await cartrepository.GetAsync(e => e.ApplicationUserId == user.Id, include: [e => e.Movie!]);

            if (user is not null && carts is not null)
            {
                // Create Order <-- Cart
                var order = new Order()
                {
                    ApplicationUserId = user.Id,
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = OrderStatus.Completed,
                    TotalPrice = carts.Sum(e => e.Movie.Price * e.Count)
                };
                await orderRepository.AddAsync(order);
                await orderRepository.CommitAsync();
                // Create options for strip
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/success?orderId={order.Id}",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/customer/checkout/cancel",
                };


                foreach (var item in carts)
                {
                    options.LineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Movie.Name,
                                //Description = item.Movie.Description,
                            },
                            UnitAmount = (long)item.Movie.Price * 100,
                        },
                        Quantity = item.Count,
                    });
                }

                var service = new SessionService();
                var session = service.Create(options);

                order.SessionId = session.Id;
                await orderRepository.CommitAsync();

                return Ok(new 
                { 
                msg = session.Url,
                });
            }
            return NotFound();
        }
        #endregion
    }
}
