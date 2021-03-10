using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly SignInManager<BTUser> _signInManager;

        public TicketsController(ApplicationDbContext context, 
            UserManager<BTUser> userManager, 
            IBTHistoryService historyService,
            IBTProjectService projectService,
            IBTRoleService roleService,
            SignInManager<BTUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _historyService = historyService;
            _projectService = projectService;
            _roleService = roleService;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> AcceptInvite(string userId, string code)
        {
            var realGuid = Guid.Parse(code);
            var invite = _context.Invites.FirstOrDefault(i => i.CompanyToken == realGuid && i.InviteeId == userId);
            if(invite is null)
            {
                return NotFound();
            }
            if(invite.IsValid)
            {
                invite.IsValid = false;
                var user = await _context.Users.FindAsync(userId);
               await _signInManager.SignInAsync(user, isPersistent: false);
                var roles = await _userManager.GetRolesAsync(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create");
            }
            return NotFound();
        }
        // GET: Tickets
        [Authorize]
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


        public async Task<IActionResult> GoToTicket(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync((int)id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.Viewed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = notification.TicketId });
        }

        //Show attachments
        public IActionResult ShowFile(int id)
        {
            TicketAttachment ticketAttachment = _context.TicketAttachments.Find(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
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
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(userProjects, "Id", "Name");
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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created, Updated, ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId,DeveloperIds")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var member = await _projectService.UsersOnProjectAsync(ticket.ProjectId);
                ticket.Created = DateTimeOffset.Now;
                ticket.OwnerUserId = _userManager.GetUserId(User);
                _context.Add(ticket);
                if (User.IsInRole("Admin") || (User.IsInRole("Submitter") && member.Contains(user)))
                {
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("MyTickets");

            }

            var userId = _userManager.GetUserId(User);
            var userProjects = await _projectService.ListUserProjectsAsync(userId);

            ViewData["DeveloperIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["ProjectId"] = new SelectList(userProjects.ToString(), "Id", "Name", ticket.ProjectId);
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
            
           ////Restrict to only people on the project
           // var user = await _userManager.GetUserAsync(User);
           // var notMember = await _projectService.UsersNotOnProjectAsync(ticket.ProjectId);
           // if (notMember.Contains(user)) 
           // {
           //     return RedirectToAction("MyTickets");
           // };

           // //Block Owners from making changes to tickets that are not theirs
           // if (await _roleService.IsUserInRoleAsync(user, "Submitter") && ticket.OwnerUserId != user.Id)
           // {
           //     return RedirectToAction("MyTickets");
           // };

           // //Developers cannot change tickets to which they are not assigned
           // if (await _roleService.IsUserInRoleAsync(user, "Developer") && ticket.DeveloperUserId != user.Id)
           // {
           //     return RedirectToAction("MyTickets");
           // };

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

               
               var user = await _userManager.GetUserAsync(User);
                var notMember = await _projectService.UsersNotOnProjectAsync(ticket.ProjectId);
            if (ModelState.IsValid)
            {
                try
                {

                    ticket.Updated = DateTimeOffset.Now;

                    _context.Update(ticket);
                    //Admin can make changes
                    //Submitter can make changes to tickets they own
                    if (ticket.OwnerUserId == user.Id)
                    {
                        await _context.SaveChangesAsync();
                    }
                    else if (User.IsInRole("Admin"))
                    {
                        await _context.SaveChangesAsync();
                    }
                    //Developers can make changes to their tickets
                    else if (User.IsInRole("Developer") && (ticket.DeveloperUserId == user.Id))
                    {
                        await _context.SaveChangesAsync();
                    }
                    //PM can make changes to tickets they manager
                    else if (await _roleService.IsUserInRoleAsync(user, "ProjectManager") && (!notMember.Contains(user)))
                    {
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return RedirectToAction("MyTickets");
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

                    return RedirectToAction("MyTickets");
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
        [Authorize(Roles = "Admin")]
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
