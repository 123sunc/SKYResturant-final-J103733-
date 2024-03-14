using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SKYResturant.Models;

namespace SKYResturant.Controllers
{
    public class OrderHistoriesController : Controller
    {
        private readonly SkyDbContext _context;

        public OrderHistoriesController(SkyDbContext context)
        {
            _context = context;
        }

        // GET: OrderHistories
        public async Task<IActionResult> Index()
        {
              return _context.OrderHistories != null ? 
                          View(await _context.OrderHistories.ToListAsync()) :
                          Problem("Entity set 'SkyDbContext.OrderHistories'  is null.");
        }

        // GET: OrderHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.OrderHistories == null)
            {
                return NotFound();
            }

            var orderHistory = await _context.OrderHistories
                .FirstOrDefaultAsync(m => m.OrderNo == id);
            if (orderHistory == null)
            {
                return NotFound();
            }

            return View(orderHistory);
        }

        // GET: OrderHistories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OrderHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderNo,Email")] OrderHistory orderHistory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orderHistory);
        }

        // GET: OrderHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.OrderHistories == null)
            {
                return NotFound();
            }

            var orderHistory = await _context.OrderHistories.FindAsync(id);
            if (orderHistory == null)
            {
                return NotFound();
            }
            return View(orderHistory);
        }

        // POST: OrderHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderNo,Email")] OrderHistory orderHistory)
        {
            if (id != orderHistory.OrderNo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderHistoryExists(orderHistory.OrderNo))
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
            return View(orderHistory);
        }

        // GET: OrderHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.OrderHistories == null)
            {
                return NotFound();
            }

            var orderHistory = await _context.OrderHistories
                .FirstOrDefaultAsync(m => m.OrderNo == id);
            if (orderHistory == null)
            {
                return NotFound();
            }

            return View(orderHistory);
        }

        // POST: OrderHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.OrderHistories == null)
            {
                return Problem("Entity set 'SkyDbContext.OrderHistories'  is null.");
            }
            var orderHistory = await _context.OrderHistories.FindAsync(id);
            if (orderHistory != null)
            {
                _context.OrderHistories.Remove(orderHistory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderHistoryExists(int id)
        {
          return (_context.OrderHistories?.Any(e => e.OrderNo == id)).GetValueOrDefault();
        }
    }
}
