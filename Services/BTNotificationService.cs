using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Phoenix.Data;
using Phoenix.Models;


namespace Phoenix.Services
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync(ClaimsPrincipal currentUser)
        {
            throw new NotImplementedException();
        }

        //I need access to the database to get the notification
        //I need the UserManager to convert ClaimsPrincipal to BTUser
        public BTNotificationService(
        ApplicationDbContext context,
         UserManager<BTUser> UserManager)
        {
            _context = context;
            _userManager = UserManager;
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(ClaimsPrincipal currentUser)
        {
            //Cover the ClaimsPrincipal into a BTUser - database doesn't know ClaimsPrinc
            BTUser user = await _userManager.GetUserAsync(currentUser);

            var userNotifications = _context.Notifications.Where(n => n.RecipientId == user.Id).Include(n => n.Sender);
            var unreadNotifications = await userNotifications.Where(n => !n.Viewed).ToListAsync();
            //var unreadNotification = _context.Notifications.Where(n => n.RecipientId == user.Id && !n.Viewed).ToList();
            //var unreadNotification = _context.Notifications.Where(n => n.RecipientId == user.Id && !n.Viewed).Include(n => n.Sender)ToListAsync();
            return unreadNotifications;
        }
    }
}
