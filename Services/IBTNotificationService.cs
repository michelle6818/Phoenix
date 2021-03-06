using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Phoenix.Models;

namespace Phoenix.Services
{
    public interface IBTNotificationService
    {
        //When we talk about a "user" there are two versions of a "user"
        //A. A record in the Users table - represents any person who is registered
                //can use UserId, email.UserName, etc
        //B. The other is capital "U" User - this is the currently logged in user
            //ClaimsPrinciple User   @UserManager.GetUserAsync()---this converts "User" into "user"
               //Full userrecord BTUser <user>   Views and Controllers have access

        public Task<IEnumerable<Notification>>GetUnreadNotificationsAsync(ClaimsPrincipal currentUser);

        public Task<IEnumerable<Notification>> GetAllNotificationsAsync(ClaimsPrincipal currentUser);
    }
}
