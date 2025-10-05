using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace etickets_aspnet_api.Areas.Customer.Controllers
{
    [Route("api/customer/[controller]")]
    [ApiController]
    [Area("Customer")]
    [Authorize]
    public class CheckoutsController : ControllerBase
    {

        #region Fields
        private readonly UserManager<ApplicationUser> userManger;
        private readonly ILogger<CheckoutsController> _logger;
        private readonly IRepository<Cart> repositoryCart;
        private readonly IRepository<Order> repositoryOrder;
        private readonly IRepository<OrderItems> repositoryOrderItem;
        private readonly IEmailSender emailSender;
        private readonly CineBookContext _context;
        #endregion

        #region Constrocture

        public CheckoutsController(UserManager<ApplicationUser> userManger, ILogger<CheckoutController> logger, IRepository<Cart> repositoryCart, IRepository<Order> repositoryOrder, IRepository<OrderItems> repositoryOrderItem, IEmailSender emailSender, CineBookContext _context)
        {
            this.userManger = userManger;
            this._logger = logger;
            this.repositoryCart = repositoryCart;
            this.repositoryOrder = repositoryOrder;
            this.repositoryOrderItem = repositoryOrderItem;
            this.emailSender = emailSender;
            this._context = _context;
        }
        #endregion

        #region Success
        [HttpGet("Success/{orderId}")]
        public async Task<IActionResult> Success(OrderIdRequest orderId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await userManger.GetUserAsync(User);
                var carts = await repositoryCart.GetAsync(e => e.ApplicationUserId == user.Id, include: [e => e.Movie]);
                var orders = await repositoryOrder.GetOneAsync(e => e.Id == orderId.Id);



                var orderItems = carts.Select(e => new OrderItems()
                {
                    MovieId = e.MovieId,
                    OrderId = orders.Id,
                    Count = e.Count,
                    Price = (decimal)e.Movie.Price
                }).ToList();


                await repositoryOrderItem.CreateRangeAsync(orderItems);



                // 2. Decrement Quantity -> movie
                foreach (var item in carts)
                {
                    item.Movie.Quantity -= item.Count;
                }

                // 3. Delete Old Cart
                await repositoryCart.DeleteRangeAsync(carts);
                await repositoryCart.CommitAsync();
                // 4. Update Order Prop.
                var order = await repositoryOrder.GetOneAsync(e => e.Id == orderId.Id);

                var service = new SessionService();
                var session = service.Get(order.SessionId);

                order.SessionId = session.Id;

                order.OrderStatus = OrderStatus.Completed;
                //order.TransactionStatus = true;
                order.TransctionId = session.PaymentIntentId;

                await repositoryOrder.CommitAsync();

                // 5. Send Email to user
                await emailSender.SendEmailAsync(user.Email, "Thanks", "Order Completed");
                transaction.Commit();
                return Ok(new
                {
                    msg = "Payment Successfully"
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);

                transaction.Rollback();

                return BadRequest(new
                {
                    msg = ex.Message
                });
            }
        }
        #endregion

        #region Cancel
        [HttpGet("cancel/{orderId}")]
        public async Task<IActionResult> Cancel(OrderIdRequest orderId)
        {
            var order = await repositoryOrder.GetOneAsync(e => e.Id == orderId.Id);

            if (order is null)
                return NotFound();

            // update order status
            order.OrderStatus = OrderStatus.Canceled;

            await repositoryOrder.CommitAsync();

            return Ok(new
            {
                msg = "Payment Successfully"
            });

        }
        #endregion
    }
}
