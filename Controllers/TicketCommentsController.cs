using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phoenix.Data;
using Phoenix.Models;

namespace Phoenix.Controllers
{
    [Authorize]
    public class TicketCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IEmailSender _emailSender;

        public TicketCommentsController(ApplicationDbContext context,
            UserManager<BTUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: TicketComments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TicketComments.Include(t => t.Ticket).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TicketComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketComment = await _context.TicketComments
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // GET: TicketComments/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Comment,Created,TicketId")] TicketComment ticketComment, int ticketId, string content)
        {
            if (ModelState.IsValid)
            {
                var ticket = _context.Tickets.Include(t => t.DeveloperUser).FirstOrDefault(t => t.Id == ticketId);
                ticketComment.Created = DateTime.Now;
                ticketComment.UserId = _userManager.GetUserId(User);
                var user = await _userManager.GetUserAsync(User);
                _context.Add(ticketComment);
                if (!User.IsInRole("DemoUser"))
                {
                    await _context.SaveChangesAsync();
                }

                if (ticketComment.UserId != ticket.DeveloperUserId)
                {
                    Notification notification = new Notification
                    {
                        Created = DateTimeOffset.Now,
                        RecipientId = ticket.DeveloperUserId,
                        SenderId = ticketComment.UserId,
                    };
                    await _context.Notifications.AddAsync(notification);
                    string devEmail = ticket.DeveloperUser.Email;
                    string subject = "Ticket Comment Added";
                    string message = $"'{ticket.Title}' has a new comment by {user.FullName} stating: '{ticketComment.Comment}'";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);
                }
                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", ticketComment.UserId);
            //return View(ticketComment);
            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });


        }


        //TICKETS SHOULD NOT BE ABLE TO BE EDITED OR DELETED



        // GET: TicketComments/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticketComment = await _context.TicketComments.FindAsync(id);
        //    if (ticketComment == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", ticketComment.UserId);
        //    return View(ticketComment);
        //}

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        //{
        //    if (id != ticketComment.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(ticketComment);
        //            if (!User.IsInRole("DemoUser"))
        //            {
        //                await _context.SaveChangesAsync();
        //            }
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TicketCommentExists(ticketComment.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", ticketComment.UserId);
        //    return View(ticketComment);
        //}

        // GET: TicketComments/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticketComment = await _context.TicketComments
        //        .Include(t => t.Ticket)
        //        .Include(t => t.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (ticketComment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(ticketComment);
        //}

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
       
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var ticketComment = await _context.TicketComments.FindAsync(id);
        //    _context.TicketComments.Remove(ticketComment);
        //    if (!User.IsInRole("DemoUser"))
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction(nameof(Index));
        //}

        private bool TicketCommentExists(int id)
        {
            return _context.TicketComments.Any(e => e.Id == id);
        }
    }
}
