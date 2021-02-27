using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Phoenix.Models;

namespace Phoenix.Services
{
    public class BTTicketService : IBTTicketService
    {
        public Task GetPriority()
        {
            throw new NotImplementedException();
        }

        public Task GetStatus()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserOnTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ticket>> ListTicketProjectsAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        Task IBTTicketService.GetType()
        {
            throw new NotImplementedException();
        }
    }
}
