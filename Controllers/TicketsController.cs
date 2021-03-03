using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phoenix.Data;
using Phoenix.Data.Enums;
using Phoenix.Models;
using Phoenix.Services;

namespace Phoenix.Controllers
{
    [Authorize]

    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTHistoryService _historyService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRoleService _roleService;

        public TicketsController(ApplicationDbContext context, 
            UserManager<BTUser> userManager, 
            IBTHistoryService historyService,
            IBTProjectService projectService,
            IBTRoleService roleService)
        {
            _context = context;
            _userManager = userManager;
            _historyService = historyService;
            _projectService = projectService;
            _roleService = roleService;
        }

        // GET: Tickets
       
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType);
            return View(await applicationDbContext.ToListAsync());
        }

        //GET: Ticket view per user
        public async Task<IActionResult> MyTickets()
        {
            var model = new List<Ticket>();

            //Test if ADMIN
            if (User.IsInRole("Admin")) 
            {
                model = await _context.Tickets
                  .Include(t => t.DeveloperUser)
                  .Include(t => t.OwnerUser)
                  .Include(t => t.Project)
                  .Include(t => t.TicketPriority)
                  .Include(t => t.TicketStatus)
                  .Include(t => t.TicketType).ToListAsync();
            }
            //Test if ProjectManager
            else if (User.IsInRole("ProjectManager"))
            {
                var userId = _userManager.GetUserId(User);
                var projects = await _projectService.ListUserProjectsAsync(userId);

                model = projects.SelectMany(p => p.Tickets).ToList();
            }

            //Test if Developer
            else if (User.IsInRole("Developer"))
            {
                var userId = _userManager.GetUserId(User);
                model = _context.Tickets
                   .Include(t => t.DeveloperUser)
                  .Include(t => t.OwnerUser)
                  .Include(t => t.Project)
                  .Include(t => t.TicketPriority)
                  .Include(t => t.TicketStatus)
                  .Include(t => t.TicketType).Where(t => t.DeveloperUserId == userId).ToList();
            }

            //Test if Submitter/Owner (anyone can submit/own a ticket)
            else
            {
                var userId = _userManager.GetUserId(User);
                model = _context.Tickets
                  .Include(t => t.DeveloperUser)
                  .Include(t => t.OwnerUser)
                  .Include(t => t.Project)
                  .Include(t => t.TicketPriority)
                  .Include(t => t.TicketStatus)
                  .Include(t => t.TicketType).Where(t => t.OwnerUserId == userId).ToList();
            }     


            return View(model);
        }
        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .Include(t => t.Attachments).ThenInclude(a => a.User)
                .Include(t => t.Histories).ThenInclude(h => h.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Admin,ProjectManager,Developer,Submitter")]
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var userProjects = await _projectService.ListUserProjectsAsync(userId);

            ViewData["DeveloperIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            //ViewData["ProjectId"] = new SelectList(userProjects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,DeveloperUserId,DeveloperIds")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.Now;
                ticket.OwnerUserId = _userManager.GetUserId(User);
                _context.Add(ticket);
                if (!User.IsInRole("DemoUser"))
                {
                await _context.SaveChangesAsync();

                }
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var userProjects = await _projectService.ListUserProjectsAsync(userId);

            ViewData["DeveloperIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["ProjectId"] = new SelectList(userProjects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Admin,ProjectManager,Developer,Submitter")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            //Get Old Ticket
            Ticket oldTicket = await _context.Tickets
                .Include(t => t.TicketType)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.Project)
                .Include(t => t.DeveloperUser)
                .AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Updated = DateTimeOffset.Now;

                    _context.Update(ticket);
                    if (!User.IsInRole("DemoUser"))
                    {
                        await _context.SaveChangesAsync();
                    }

                    //Add History

                    //Get User Id
                    string userId = _userManager.GetUserId(User);

                    //Get New Ticket
                    Ticket newTicket = await _context.Tickets
                     .Include(t => t.TicketType)
                     .Include(t => t.TicketPriority)
                     .Include(t => t.TicketStatus)
                     .Include(t => t.Project)
                     .Include(t => t.DeveloperUser)
                     .AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

                    //Call History Service
                    await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);
                    //await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }


            ViewData["DeveloperIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            //ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatus, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            _context.Tickets.Remove(ticket);
            if (!User.IsInRole("DemoUser"))
            {
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}
