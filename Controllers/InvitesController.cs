﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phoenix.Data;
using Phoenix.Models;

namespace Phoenix.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvitesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Email,CompanyToken,InviteDate,InvitorId,InviteeId,IsValid")] Invite invite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "FullName", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "FullName", invite.InvitorId);
            return View(invite);
        }

        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "FullName", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "FullName", invite.InvitorId);
            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Email,CompanyToken,InviteDate,InvitorId,InviteeId,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "FullName", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "FUllName", invite.InvitorId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invite = await _context.Invites.FindAsync(id);
            _context.Invites.Remove(invite);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
            return _context.Invites.Any(e => e.Id == id);
        }
    }
}
