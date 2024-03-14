using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using SKYResturant.Models;

namespace SKYResturant.Controllers
{
    public class CheckoutItemsController : Controller
    {
        private readonly SkyDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PaymentService _paymentService;
        public OrderHistory Order = new OrderHistory();
        public CheckoutItemsController(SkyDbContext context , UserManager<IdentityUser> userManager, PaymentService paymentService)
        {
            _context = context;
            _userManager = userManager;
            _paymentService = paymentService;
        }
        [Authorize]
        // GET: CheckoutItems
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.CheckoutCustomer.FirstOrDefaultAsync(c => c.Email == user.Email);

            if (customer != null)
            {
                var items = await _context.CheckoutItems
                    .FromSqlRaw("SELECT Menus.Id, Menus.Price, Menus.Name, BasketItem.Quantity " +
                                "FROM Menus INNER JOIN BasketItem " +
                                $"ON Menus.Id = BasketItem.StockID WHERE BasketItem.BasketID = {customer.BasketID}")
                    .ToListAsync();

                return View(items);
            }

            return View(new List<CheckoutItem>());
        }
        public async Task<IActionResult> ProcessPayment()
        {
            return View("ProcessPayment");
        }
        [HttpPost]
        public async Task<IActionResult> ProcessPaymentConfirmed()
        {
            // Process payment
            if (_paymentService.ProcessPayment(50.0m, "4242424242424242", 12, 2025, "123"))
            {
                // Payment successful, update order status, etc.
                //return RedirectToAction(nameof(PaymentSuccessful));
                return RedirectToAction(nameof(Buy));

            }
            else
            {
                // Payment failed, handle error
                return RedirectToAction(nameof(PaymentFailed));
            }
        }

        public IActionResult PaymentSuccessful()
        {
            return View();
        }

        public IActionResult PaymentFailed()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Buy()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Find the customer's basket
            var customer = await _context.CheckoutCustomer.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (customer == null)
            {
                return NotFound();
            }

            // Find the items in the basket
            var basketItems = await _context.BasketItem
                .Where(bi => bi.BasketID == customer.BasketID)
                .ToListAsync();

            // Create a new order
            var order = new OrderHistory
            {
                Email = user.Email,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in basketItems)
            {
                var menu = await _context.Menus.FindAsync(item.StockID);
                if (menu != null)
                {
                    // Reload the menu to get the latest version
                    await _context.Entry(menu).ReloadAsync();

                    order.OrderItems.Add(new OrderItem
                    {
                        StockID = item.StockID,
                        Quantity = item.Quantity
                    });
                }
                else
                {
                    _context.BasketItem.Remove(item);
                }
            }

            // Save changes, handling concurrency conflicts
            try
            {
                _context.OrderHistories.Add(order);
                await _context.SaveChangesAsync();

                _context.BasketItem.RemoveRange(basketItems);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflict
                // You can log the exception or provide feedback to the user
                // For example, you can reload the entities and retry the save operation
                // Alternatively, you can inform the user that their changes cannot be saved due to a concurrency conflict
                // See: https://docs.microsoft.com/en-us/ef/core/saving/concurrency
                // Example: return RedirectToAction(nameof(ConcurrencyError));
            }

            return RedirectToAction(nameof(PaymentSuccessful));
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var orderHistories = await _context.OrderHistories
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.FoodItem)
                .Where(o => o.Email == user.Email)
                .ToListAsync();

            return View(orderHistories);
        }


        // GET: CheckoutItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CheckoutItems == null)
            {
                return NotFound();
            }

            var checkoutItem = await _context.CheckoutItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkoutItem == null)
            {
                return NotFound();
            }

            return View(checkoutItem);
        }

        // GET: CheckoutItems/Create
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Price,Name,Quantity")] CheckoutItem checkoutItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(checkoutItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(checkoutItem);
        }

        // GET: CheckoutItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CheckoutItems == null)
            {
                return NotFound();
            }

            var checkoutItem = await _context.CheckoutItems.FindAsync(id);
            if (checkoutItem == null)
            {
                return NotFound();
            }
            return View(checkoutItem);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Price,Name,Quantity")] CheckoutItem checkoutItem)
        {
            if (id != checkoutItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(checkoutItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheckoutItemExists(checkoutItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(checkoutItem);
        }

        // GET: CheckoutItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CheckoutItems == null)
            {
                return NotFound();
            }

            var checkoutItem = await _context.CheckoutItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkoutItem == null)
            {
                return NotFound();
            }

            return View(checkoutItem);
        }

        // POST: CheckoutItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CheckoutItems == null)
            {
                return Problem("Entity set 'SkyDbContext.CheckoutItems'  is null.");
            }
            var checkoutItem = await _context.CheckoutItems.FindAsync(id);
            if (checkoutItem != null)
            {
                _context.CheckoutItems.Remove(checkoutItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CheckoutItemExists(int id)
        {
          return (_context.CheckoutItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
