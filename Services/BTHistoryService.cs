using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Phoenix.Data;
using Phoenix.Models;

namespace Phoenix.Services
{
    public class BTHistoryService : IBTHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<BTUser> _userManager;

        //Constructors
        public BTHistoryService(
            ApplicationDbContext context,
            IEmailSender emailSender,
            UserManager<BTUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            if (oldTicket.Title != newTicket.Title)
            {
                //Ticket History for Title
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Title",
                    OldValue = oldTicket.Title,
                    NewValue = newTicket.Title,
                    Created = DateTimeOffset.Now,
                    UserId = userId
                };
                await _context.TicketHistories.AddAsync(history);

                //Only notify the developer of changes if someone ELSE makes them
                if (userId != oldTicket.DeveloperUserId)
                {
                    Notification notification = new Notification
                    {
                        TicketId = newTicket.Id,
                        Description = "Your Ticket Title has been changed.",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = newTicket.DeveloperUserId,
                    };

                    await _context.Notifications.AddAsync(notification);
                    string devEmail = newTicket.DeveloperUser.Email;
                    string subject = "Ticket Title Change";
                    string message = $"The title for {oldTicket.Title} was changed to '{newTicket.Title}'.";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);
                }
            }

            //---------------------------------------

            if (oldTicket.Description != newTicket.Description)
            {

                //Ticket History for Description
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Description",
                    OldValue = oldTicket.Description,
                    NewValue = newTicket.Description,
                    Created = DateTimeOffset.Now,
                    UserId = userId
                };
                await _context.TicketHistories.AddAsync(history);

                if (userId != oldTicket.DeveloperUserId)
                {
                    Notification notification = new Notification
                    {
                        TicketId = newTicket.Id,
                        Description = "Your Ticket Description has been changed.",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = newTicket.DeveloperUserId,
                    };
                    await _context.Notifications.AddAsync(notification);
                    string devEmail = newTicket.DeveloperUser.Email;
                    string subject = "Ticket Description Change";
                    string message = $"The description for {newTicket.Title} was changed to '{history.NewValue}'.";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);
                }
            }
            //------------------------------------------

            if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
            {
                //Ticket History for Type -- special case:  we need the NAME of TYPE, not just Id
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Type",
                    OldValue = oldTicket.TicketType.Name,
                    NewValue = newTicket.TicketType.Name,
                    Created = DateTimeOffset.Now,
                    UserId = userId
                };
                await _context.TicketHistories.AddAsync(history);

                if (userId != oldTicket.DeveloperUserId)
                {
                    Notification notification = new Notification
                    {
                        TicketId = newTicket.Id,
                        Description = "Your Ticket Type has been changed.",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = newTicket.DeveloperUserId,
                    };
                    await _context.Notifications.AddAsync(notification);
                    string devEmail = newTicket.DeveloperUser.Email;
                    string subject = "Ticket Type Change";
                    string message = $"The type for {newTicket.Title} was changed to {history.NewValue}.";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);
                }
            }
            //-----------------------------------------------------------------
            if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
            {
                //Ticket History for Priority (like the Type)
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Priority",
                    OldValue = oldTicket.TicketPriority.Name,
                    NewValue = newTicket.TicketPriority.Name,
                    Created = DateTimeOffset.Now,
                    UserId = userId
                };
                await _context.TicketHistories.AddAsync(history);

                if (userId != oldTicket.DeveloperUserId)
                {
                    Notification notification = new Notification
                    {
                        TicketId = newTicket.Id,
                        Description = "Your Ticket Priority has been changed.",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = newTicket.DeveloperUserId,
                    };
                    await _context.Notifications.AddAsync(notification);
                    string devEmail = newTicket.DeveloperUser.Email;
                    string subject = "Ticket Priority Change";
                    string message = $"The priority for {newTicket.Title} was changed to {history.NewValue}.";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);
                }
            }
            //----------------------------------------------------------------------
            if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
            {
                //Ticket History for Status (like the Type)
                TicketHistory history = new TicketHistory
                {
                    TicketId = newTicket.Id,
                    Property = "Status",
                    OldValue = oldTicket.TicketStatus.Name,
                    NewValue = newTicket.TicketStatus.Name,
                    Created = DateTimeOffset.Now,
                    UserId = userId
                };
                await _context.TicketHistories.AddAsync(history);

                if (userId != oldTicket.DeveloperUserId)
                {
                    Notification notification = new Notification
                    {
                        TicketId = newTicket.Id,
                        Description = "Your Ticket Status has been changed.",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = newTicket.DeveloperUserId,
                    };
                    await _context.Notifications.AddAsync(notification);
                    string devEmail = newTicket.DeveloperUser.Email;
                    string subject = "Ticket Status Change";
                    string message = $"The status for {newTicket.Title} was changed to {history.NewValue}.";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);
                }
            }
            //----------------------------------------------------------------------
            if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
            {

                //Ticket History for Developer (like the Type)
                TicketHistory history = new TicketHistory();

                if (oldTicket.DeveloperUserId == null)
                {
                    history = new TicketHistory
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = "Unassigned",
                        NewValue = newTicket.DeveloperUser.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                else if (newTicket.DeveloperUserId == null)
                {
                    history = new TicketHistory
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = "Unassigned",
                        NewValue = newTicket.DeveloperUser.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId
                    };
                    await _context.TicketHistories.AddAsync(history);
                }
                else
                {
                    history = new TicketHistory
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = oldTicket.DeveloperUser.FullName,
                        NewValue = newTicket.DeveloperUser.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                if (userId != oldTicket.DeveloperUserId)
                {
                    Notification notification = new Notification
                    {
                        TicketId = newTicket.Id,
                        Description = "You have a new ticket.",
                        Created = DateTimeOffset.Now,
                        SenderId = userId,
                        RecipientId = newTicket.DeveloperUserId,
                    };
                    await _context.Notifications.AddAsync(notification);

                    //Send an Email
                    string devEmail = newTicket.DeveloperUser.Email;
                    string subject = "New Ticket Assignment";
                    string message = $"You have a new ticket for project: {newTicket.Project.Name}";

                    await _emailSender.SendEmailAsync(devEmail, subject, message);


                }
            }

                await _context.SaveChangesAsync();
            
        }
    }
}
