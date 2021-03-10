using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Phoenix.Models.ViewModels;

namespace Phoenix.Services
{
    public interface IBTInviteService
    {
        public Task<string> InviteWizardAsync(InviteViewModel inviteViewModel);
    }
}
