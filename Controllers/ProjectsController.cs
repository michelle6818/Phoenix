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
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTRoleService _roleService;
        private readonly UserManager<BTUser> _userManager;

        public ProjectsController(
            ApplicationDbContext context,
            IBTProjectService projectService,
            IBTRoleService roleService,
            UserManager<BTUser> userManager)
        {
            _context = context;
            _projectService = projectService;
            _roleService = roleService;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> ManageUsersOnProject()
        {
            ViewData["ProjectId"] = new SelectList(_context.Set<Project>(), "Id", "Name");
            ViewData["ProjectManagerId"] = new SelectList(await _roleService.UsersInRoleAsync(Roles.ProjectManager.ToString()), "Id", "FullName");
            ViewData["DeveloperIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            ViewData["SubmitterIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Submitter.ToString()), "Id", "FullName");

            return View();
        }


        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsersOnProject(int projectId, string projectManagerId, List<string> developerIds, List<string> submitterIds)
        {
            var currentlyOnProject = await _projectService.UsersNotOnProjectAsync(projectId);
            foreach (var user in currentlyOnProject)
            {
                await _projectService.RemoveUserFromProjectAsync(user.Id, projectId);
            }
            await _projectService.AddUserToProjectAsync(projectManagerId, projectId);
            foreach (var userId in developerIds)
            {
                await _projectService.AddUserToProjectAsync(userId, projectId);
            }
            foreach (var userId in submitterIds)
            {
                await _projectService.AddUserToProjectAsync(userId, projectId);
            }
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        //GET: Project view per user
        public async Task<IActionResult> MyProjects()
        {
            //var model = new List<Project>();
            var userId = _userManager.GetUserId(User);
            var model = await _projectService.ListUserProjectsAsync(userId);
            var tickets = _context.Tickets
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketPriority)
                .ToList();
            var projects = _context.Projects
                .Include(p => p.Company)
                .Include(p => p.Members)
                .ToList();
            //model = _context.Projects.Include(p => p.Members).Include(p => p.User);
            return View(model.ToList());
        }

        ////Practice adding view list
        public async Task<IActionResult> ProjectMembers(int? id)
        {
           
            var model = await _projectService.UsersOnProjectAsync(id.Value);
            
            return View(model);
        }



        // GET: Projects
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects.Include(p => p.Company);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(m => m.Company)
                .Include(m => m.Tickets)
                .Include(m => m.Tickets).ThenInclude(t => t.TicketPriority)
                .Include(m => m.Tickets).ThenInclude(t => t.TicketStatus)
                .Include(m => m.Tickets).ThenInclude(t => t.TicketType)
                .Include(m => m.Members)
                //.Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(await _roleService.UsersInRoleAsync(Roles.ProjectManager.ToString()), "Id", "FullName");
            ViewData["ProjectManagerId"] = new SelectList(await _roleService.UsersInRoleAsync(Roles.ProjectManager.ToString()), "Id", "FullName");
            ViewData["DeveloperIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Developer.ToString()), "Id", "FullName");
            ViewData["SubmitterIds"] = new MultiSelectList(await _roleService.UsersInRoleAsync(Roles.Submitter.ToString()), "Id", "FullName");

            return View(project);
        }


        // GET: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileName,ImageFileData,CompanyId")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                if (!User.IsInRole("DemoUser"))
                {
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileName,ImageFileData,CompanyId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    if (!User.IsInRole("DemoUser"))
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            if (!User.IsInRole("DemoUser"))
            {
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
