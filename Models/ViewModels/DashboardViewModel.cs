using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Phoenix.Models.ViewModels
{
    public class DashboardViewModel
    {
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
