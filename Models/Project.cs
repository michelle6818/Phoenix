using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Phoenix.Extensions;

namespace Phoenix.Models
{
    public class Project
    {
        //Default Constructor
        public Project()
        {
            Tickets = new HashSet<Ticket>();
            Members = new HashSet<BTUser>();
        }

        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        public string Description { get; set; }
        
        [Display(Name = "Project Image")]
        [NotMapped]
        [DataType(DataType.Upload)]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png" })]

        public IFormFile ImageFormFile { get; set; }

        public string ImageFileName { get; set; }

        public byte[] ImageFileData { get; set; }

        public int? CompanyId { get; set; }

        //Navigation
        public virtual ICollection<BTUser> Members { get; set; }

        //public IList<ProjectUser> ProjectUsers { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }

        public virtual Company Company { get; set; }
    }
}
