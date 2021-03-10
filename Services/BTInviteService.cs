using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Phoenix.Data;
using Phoenix.Data.Enums;
using Phoenix.Models;
using Phoenix.Models.ViewModels;

namespace Phoenix.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTRoleService _roleService;

        public BTInviteService(
            ApplicationDbContext context,
            UserManager<BTUser> userManager,
            IBTProjectService projectService,
            IBTRoleService roleService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _roleService = roleService;

        }
        public async Task<string> InviteWizardAsync(InviteViewModel inviteViewModel)
        {
            // Create a new Company -- Database
            var companyId = await CreateCompanyAsync(inviteViewModel.CompanyName, inviteViewModel.CompanyDescription);
            // I will call a private mehtod here
            // Create a new BTUser -- UserManage
            var user = new BTUser
            {
                Email = inviteViewModel.Email,
                UserName = inviteViewModel.Email,
                FirstName = inviteViewModel.FirstName,
                LastName = inviteViewModel.LastName,
                EmailConfirmed = true,
                CompanyId = companyId
            };
            var result = await _userManager.CreateAsync(user, "Abc&123");
            // Assign that user to a role -- RoleService
            await _roleService.AddUserToRoleAsync(user, Roles.Admin.ToString());
            // Create a new Project -- Database
            var projectId = await CreateProjectAsync(inviteViewModel.ProjectName, inviteViewModel.ProjectDescription, companyId);
            // Assign that user to a project -- ProjectService
            await _projectService.AddUserToProjectAsync(user.Id, projectId);

            return user.Id;
        }

        private async Task<int> CreateCompanyAsync(string companyName, string companyDescription)
        {
            Company company = new Company
            {
                Name = companyName,
                Description = companyDescription
            };
            if (!_context.Companies.Any(c => c.Name == company.Name))
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }
            else
            {
                company = _context.Companies.FirstOrDefault(c => c.Name == company.Name);
            }
            return company.Id;
        }

        private async Task<int> CreateProjectAsync(string projectName, string projectDescription, int companyId)
        {
            var project = new Project
            {
                Name = projectName,
                Description = projectDescription,
                CompanyId = companyId
            };
            if (!_context.Projects.Any(c => c.Name == project.Name))
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
            }
            else
            {
                project = _context.Projects.FirstOrDefault(c => c.Name == project.Name);
            }

            return project.Id;
        }
    }
}
