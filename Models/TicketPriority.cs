using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Phoenix.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int CompanyId { get; set; }
    }
}
