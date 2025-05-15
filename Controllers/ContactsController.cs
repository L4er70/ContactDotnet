using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ContactBook.Controllers
{
    [Authorize]  // Require authentication for all actions
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Contacts
        [Authorize(Roles = "Reader,Editor,Admin")]
        public async Task<IActionResult> Index()
        {
            var contacts = await _context.Contacts
                .Include(c => c.Emails)
                .Include(c => c.Phones)
                .Include(c => c.Addresses)
                .ToListAsync();

            return View(contacts);
        }

        // GET: Contacts/Details/5
        [Authorize(Roles = "Reader,Editor,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Phones,Emails,Addresses")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                // Ensure mandatory fields (FirstName and LastName) are valid
                if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
                {
                    ModelState.AddModelError(string.Empty, "First Name and Last Name are required.");
                    return View(contact);
                }

                // Initialize collections if they are null
                contact.Emails = contact.Emails?.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList() ?? new List<Email>();
                contact.Phones = contact.Phones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<Phone>();
                contact.Addresses = contact.Addresses?.Where(a => 
                    !string.IsNullOrWhiteSpace(a.Street) || 
                    !string.IsNullOrWhiteSpace(a.City) || 
                    !string.IsNullOrWhiteSpace(a.State) || 
                    !string.IsNullOrWhiteSpace(a.Zip)
                ).ToList() ?? new List<Address>();

                // Link nested objects to the parent contact
                foreach (var email in contact.Emails)
                {
                    email.Contact = contact;
                }

                foreach (var phone in contact.Phones)
                {
                    phone.Contact = contact;
                }

                foreach (var address in contact.Addresses)
                {
                    address.Contact = contact;
                    // Ensure null strings are empty strings to avoid database issues
                    address.Street = address.Street ?? string.Empty;
                    address.City = address.City ?? string.Empty;
                    address.State = address.State ?? string.Empty;
                    address.Zip = address.Zip ?? string.Empty;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        // GET: Contacts/Edit/5
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.Phones)
                .Include(c => c.Addresses)
                .Include(c => c.Emails)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }
            return View(contact);
        }

        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int id, Contact contact)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure mandatory fields are set
                    if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
                    {
                        ModelState.AddModelError(string.Empty, "First Name and Last Name are required.");
                        return View(contact);
                    }

                    // Initialize collections if they are null
                    contact.Emails = contact.Emails?.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList() ?? new List<Email>();
                    contact.Phones = contact.Phones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<Phone>();
                    contact.Addresses = contact.Addresses?.Where(a => 
                        !string.IsNullOrWhiteSpace(a.Street) || 
                        !string.IsNullOrWhiteSpace(a.City) || 
                        !string.IsNullOrWhiteSpace(a.State) || 
                        !string.IsNullOrWhiteSpace(a.Zip)
                    ).ToList() ?? new List<Address>();

                    // Link nested objects to the parent Contact
                    foreach (var email in contact.Emails)
                    {
                        email.Contact = contact;
                    }
                    foreach (var phone in contact.Phones)
                    {
                        phone.Contact = contact;
                    }
                    foreach (var address in contact.Addresses)
                    {
                        address.Contact = contact;
                        // Ensure null strings are empty strings to avoid database issues
                        address.Street = address.Street ?? string.Empty;
                        address.City = address.City ?? string.Empty;
                        address.State = address.State ?? string.Empty;
                        address.Zip = address.Zip ?? string.Empty;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
