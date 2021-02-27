using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Phoenix.Models;

namespace Phoenix.Services
{
    interface IBTTicketService
    {
        public Task<bool> IsUserOnTicketAsync(Ticket ticket);

        //All tickets for one project
        public Task<IEnumerable<Ticket>> ListTicketProjectsAsync(int projectId);

        //Ticket Priority
        public Task GetPriority();

        //Ticket Status
        public Task GetStatus();

        //Ticket Type
        public Task GetType();


       
    }
}
