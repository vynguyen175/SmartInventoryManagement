using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryManagement.Data;
using SmartInventoryManagement.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SmartInventoryManagement.Controllers
{
    [AllowAnonymous]
    public class GuestOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GuestOrdersController> _logger;

        public GuestOrdersController(ApplicationDbContext context, ILogger<GuestOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var orders = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ToList();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving guest orders.");
                return RedirectToAction("Error500", "Error");
            }
        }

        public IActionResult Create()
        {
            try
            {
                ViewBag.Products = _context.Products.ToList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Create Order page.");
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string guestName, string guestEmail, List<int> productIds, List<int> quantities)
        {
            try
            {
                if (!User.Identity.IsAuthenticated && (string.IsNullOrEmpty(guestName) || string.IsNullOrEmpty(guestEmail)))
                {
                    ModelState.AddModelError("", "Guest Name and Email are required.");
                    ViewBag.Products = _context.Products.ToList();
                    return View();
                }

                if (productIds == null || !productIds.Any() || quantities == null || productIds.Count != quantities.Count)
                {
                    ModelState.AddModelError("", "Please select at least one product with a valid quantity.");
                    ViewBag.Products = _context.Products.ToList();
                    return View();
                }

                decimal totalPrice = 0;
                var orderItems = new List<OrderItem>();

                for (int i = 0; i < productIds.Count; i++)
                {
                    var product = _context.Products.Find(productIds[i]);
                    if (product == null || quantities[i] <= 0)
                    {
                        ModelState.AddModelError("", $"Invalid product or quantity for product ID {productIds[i]}.");
                        ViewBag.Products = _context.Products.ToList();
                        return View();
                    }

                    if (quantities[i] > product.QuantityInStock)
                    {
                        ModelState.AddModelError("", $"Not enough stock for {product.Name}. Only {product.QuantityInStock} left.");
                        ViewBag.Products = _context.Products.ToList();
                        return View();
                    }

                    product.QuantityInStock -= quantities[i];

                    decimal subtotal = product.Price * quantities[i];
                    totalPrice += subtotal;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = productIds[i],
                        Quantity = quantities[i],
                        Price = product.Price
                    });
                }

                var user = User.Identity.IsAuthenticated
                    ? await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name)
                    : null;

                var order = new Order
                {
                    GuestName = User.Identity.IsAuthenticated ? user.FullName : guestName,
                    GuestEmail = User.Identity.IsAuthenticated ? user.Email : guestEmail,
                    TotalPrice = totalPrice,
                    CreatedDate = DateTime.UtcNow,
                    OrderItems = orderItems,
                    CreatedBy = User.Identity.IsAuthenticated ? "User" : "Guest"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction("ConfirmOrder", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order.");
                ModelState.AddModelError("", "⚠ An unexpected error occurred.");
                ViewBag.Products = _context.Products.ToList();
                return View();
            }
        }

        public IActionResult ConfirmOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return RedirectToAction("Error", "Error", new { code = 404 });

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);

            if (order == null)
                return RedirectToAction("Error", "Error", new { code = 404 });

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Details(int id)
        {
            try
            {
                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefault(o => o.Id == id);

                if (order == null)
                {
                    return RedirectToAction("Error", "Error", new { code = 404 });
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving details for order ID {id}");
                return RedirectToAction("Error500", "Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return RedirectToAction("Error", "Error", new { code = 404 });

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Order updatedOrder)
        {
            if (!ModelState.IsValid)
                return View(updatedOrder);

            var existingOrder = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == updatedOrder.Id);

            if (existingOrder == null)
                return RedirectToAction("Error", "Error", new { code = 404 });

            existingOrder.GuestName = updatedOrder.GuestName;
            existingOrder.GuestEmail = updatedOrder.GuestEmail;

            foreach (var updatedItem in updatedOrder.OrderItems)
            {
                var item = existingOrder.OrderItems.FirstOrDefault(oi => oi.Id == updatedItem.Id);
                if (item != null)
                {
                    item.Quantity = updatedItem.Quantity;
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return RedirectToAction("Error", "Error", new { code = 404 });

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var order = _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.Id == id);

                if (order == null)
                {
                    return RedirectToAction("Error", "Error", new { code = 404 });
                }

                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting order ID {id}");
                return RedirectToAction("Error500", "Error");
            }
        }
    }
}
