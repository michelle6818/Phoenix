using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Phoenix.Data;
using Phoenix.Data.Enums;
using Phoenix.Models;

namespace Phoenix.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        //This enables this to access the logged in user
        private readonly IBTRoleService _roleService;

        public BTProjectService(
            ApplicationDbContext context,
            UserManager<BTUser> userManager,
            IBTRoleService roleService)
        {
            _context = context;
            _userManager = userManager;
            _roleService = roleService;
        }

        public async Task AddUserToProjectAsync(string userId, int projectId)
        {
            //Need to check for Project Manager - only one allowed

            try
            {
                if (!await IsUserOnProjectAsync(userId, projectId))
                {
                    //GEt the user's record
                    BTUser user = await _userManager.FindByIdAsync(userId);
                    //Project manger is a specialcase, check for that role first
                    if (await _roleService.IsUserInRoleAsync(user, Roles.ProjectManager.ToString()))
                    {
                        //Find the old project manager and remove them from the project
                        var oldManager = await ProjectManagerOnProjectAsync(projectId);
                        if (oldManager != null)
                        {
                            await RemoveUserFromProjectAsync(oldManager.Id, projectId);
                        }
                        await RemoveUserFromProjectAsync(userId, projectId);
                    }
                    ////Get the project's full record
                    Project project = await _context.Projects.FindAsync(projectId);

                        try
                        {
                            //Add the user recordto the virtual ICollection of the project - Entity Framework does the heavy lifting here
                            project.Members.Add(user);
                            //Same way to write it:
                            //user.Projects.Add(project);
                            await _context.SaveChangesAsync();
                        }
                        //If there is a problem throw it to the outer catch
                        catch (Exception)
                        {
                            throw;
                        }

                    }

                
            }
            //If there is a problem print it out in the debugger
            catch (Exception ex)
            {
                Debug.WriteLine($"*****ERROR***** - Error Adding user to project --> {ex.Message}");
            }
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            var project = _context.Projects.Include(p => p.Members).FirstOrDefault(p => p.Id == projectId);
            var flag = project.Members.Any(u => u.Id == userId);
            return flag;
        }

        public async Task<IEnumerable<Project>> ListUserProjectsAsync(string userId)
        {
            //Check the request to see if the user is an Admin....if so return all projects
            var user = await _userManager.FindByIdAsync(userId);
            if (await _roleService.IsUserInRoleAsync(user, Roles.Admin.ToString())) 
            {
                return await _context.Projects.ToListAsync();
            }
           //What if the user is an Admin
           //Admins have access to everything so do I need to run the code below?
           //Before we run this code let's check the "if the user is Admin" first
           //Create a container for the records
            var output = new List<Project>();
            //Go though all the projects
            foreach(var project in await _context.Projects
                .Include(t => t.Tickets).ThenInclude(t => t.TicketPriority)
                .Include(t => t.Tickets).ThenInclude(t => t.TicketStatus)
                .Include(t => t.Tickets).ThenInclude(t => t.TicketType)
                .ToListAsync())

            {
                //If the user is on the project add it to the list
                if(await IsUserOnProjectAsync(userId, project.Id))
                {
                    output.Add(project);
                }
            }
            //Send out the list
            return output;
        }

        public async Task<BTUser> ProjectManagerOnProjectAsync(int projectId)
        {
            //Get all the project managers
            var projectManagers = await _userManager.GetUsersInRoleAsync(Roles.ProjectManager.ToString());
            //Get the users 
            var onProject = await UsersOnProjectAsync(projectId);
            var projectManager = projectManagers.Intersect(onProject).FirstOrDefault();
            return projectManager;
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                if (await IsUserOnProjectAsync(userId, projectId))
                {

                    BTUser user = await _userManager.FindByIdAsync(userId);
                    Project project = await _context.Projects.FindAsync(projectId);

                    try
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"****ERROR**** - Error Removing user to project --> {ex.Message}");
            }
        }

        public async Task<IEnumerable<BTUser>> SubmittersOnProjectAsync(int projectId)
        {
            var submitters = await _userManager.GetUsersInRoleAsync(Roles.Submitter.ToString());
            var onProject = await UsersOnProjectAsync(projectId);
            var subsOnProject = submitters.Intersect(onProject);
            return subsOnProject.ToList();
        }

        public async Task<IEnumerable<BTUser>> DevelopersOnProjectAsync(int projectId)
        {

            var developers = await _userManager.GetUsersInRoleAsync(Roles.Developer.ToString());
            var onProject = await UsersOnProjectAsync(projectId);
            var devsOnProject = developers.Intersect(onProject);
            return devsOnProject.ToList();

        }

        public async Task<IEnumerable<BTUser>> UsersNotOnProjectAsync(int projectId)
        {
            var output = new List<BTUser>();
            foreach (var user in await _context.Users.ToListAsync())
            {
                if (!await IsUserOnProjectAsync(user.Id, projectId))
                {
                    output.Add(user);
                }
            }

            return output;
        }

        public async Task<IEnumerable<BTUser>> UsersOnProjectAsync(int projectId)
        {
            //LINQ
            var output = new List<BTUser>();
            foreach(var user in await _context.Users.ToListAsync()) 
            { 
                if(await IsUserOnProjectAsync(user.Id, projectId))
                {
                    output.Add(user);
                }
            }

            return output;
        }
    }
}
