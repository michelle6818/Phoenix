using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Phoenix.Models.ViewModels
{
    public class InviteViewModel
    //To send an invite
    //   Need a new user
    //   Need a company for that user - can be a new company or an existing
    //   Need a project for that user - can be a new project for an existing
    //   Need a role for that user - what role they occupy is a design decision, but should not be demo

    //Information collected ,what next?
    //    The order of creation matters- Company, then project/user (company is the parent)
    //        Create a new Company
    //        Create a new BTUser
    //        Assign that user to a role
    //        Create a new Project
    //        Assign that user to a project
    {
        public BTUser User { get; set; }
        //Need Email, FirstName, LastName
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public Company Company { get; set; }
        //Need name and Descripton
        [Required]
        [StringLength(50)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Company Description")]
        public string CompanyDescription { get; set; }


        public Project Project { get; set; }
        //Need Name and Description
        //Need name and Descripton
        [Required]
        [StringLength(50)]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Project Description")]
        public string ProjectDescription { get; set; }

    }
}
