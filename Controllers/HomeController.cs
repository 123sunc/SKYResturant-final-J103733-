using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SKYResturant.Models;
using System.Diagnostics;

namespace SKYResturant.Controllers
{
    
    public class HomeController : Controller
    {
        private   ILogger<HomeController> _logger;
        private readonly SkyDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public HomeController(ILogger<HomeController> logger, SkyDbContext context, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;

            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin"); // Redirect to the admin/index page
            }
            else
            {
                return _context.Menus != null ?
                              View(await _context.Menus.ToListAsync()) :
                              Problem("Entity set 'SkyDbContext.Menus'  is null.");
            }  //return View();
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int ItemID)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                var customer = await _context.CheckoutCustomer.FirstOrDefaultAsync(c => c.Email == user.Email);
                if (customer == null)
                {
                    throw new Exception($"CheckoutCustomer not found for user '{user.Email}'.");
                }

                var item = await _context.BasketItem.FirstOrDefaultAsync(i => i.StockID == ItemID && i.BasketID == customer.BasketID);
                if (item == null)
                {
                    BasketItem newItem = new BasketItem
                    {
                        BasketID = customer.BasketID,
                        StockID = ItemID,
                        Quantity = 1,
                    };
                    _context.BasketItem.Add(newItem);
                }
                else
                {
                    item.Quantity++;
                    _context.Entry(item).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred while processing the request.");

                // Redirect to an error page or display a generic error message
                return RedirectToAction("Error", "Home");
            }
        }

        public IActionResult Aboutus()
        {
            return View();
        } 
        //public IActionResult Menu()
        //{
        //    return View("~/Views/Home/Menu.cshtml");
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}