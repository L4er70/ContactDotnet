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
using System.Globalization;
using System.IO;
using CsvHelper;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace ContactBook.Controllers
{
    [Authorize]  // Require authentication for all actions
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ContactsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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
        public async Task<IActionResult> Create(Contact contact)
        {
            try
            {
                // Log received data
                _logger.LogInformation("Received contact data - FirstName: {FirstName}, LastName: {LastName}", 
                    contact.FirstName, contact.LastName);
                _logger.LogInformation("Collections - Emails: {EmailCount}, Phones: {PhoneCount}, Addresses: {AddressCount}", 
                    contact.Emails?.Count ?? 0, contact.Phones?.Count ?? 0, contact.Addresses?.Count ?? 0);

                if (!ModelState.IsValid)
                {
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("ModelState error: {Error}", modelError.ErrorMessage);
                    }
                    return View(contact);
                }

                if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
                {
                    ModelState.AddModelError(string.Empty, "First Name and Last Name are required.");
                    return View(contact);
                }

                // Initialize collections and filter out empty entries
                contact.Emails = contact.Emails?.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList() ?? new List<Email>();
                contact.Phones = contact.Phones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<Phone>();
                contact.Addresses = contact.Addresses?.Where(a => 
                    !string.IsNullOrWhiteSpace(a.Street) || 
                    !string.IsNullOrWhiteSpace(a.City) || 
                    !string.IsNullOrWhiteSpace(a.State) || 
                    !string.IsNullOrWhiteSpace(a.Zip)
                ).ToList() ?? new List<Address>();

                _logger.LogInformation("After filtering - Emails: {EmailCount}, Phones: {PhoneCount}, Addresses: {AddressCount}", 
                    contact.Emails.Count, contact.Phones.Count, contact.Addresses.Count);

                // Link nested objects to the parent contact and ensure no null strings
                foreach (var email in contact.Emails)
                {
                    email.Contact = contact;
                    _logger.LogInformation("Processing email: {EmailAddress}", email.EmailAddress);
                }

                foreach (var phone in contact.Phones)
                {
                    phone.Contact = contact;
                    phone.PhoneNumber = phone.PhoneNumber ?? string.Empty;
                    _logger.LogInformation("Processing phone: {PhoneNumber}", phone.PhoneNumber);
                }

                foreach (var address in contact.Addresses)
                {
                    address.Contact = contact;
                    address.Street = address.Street ?? string.Empty;
                    address.City = address.City ?? string.Empty;
                    address.State = address.State ?? string.Empty;
                    address.Zip = address.Zip ?? string.Empty;
                    _logger.LogInformation("Processing address: {Street}, {City}, {State} {Zip}", 
                        address.Street, address.City, address.State, address.Zip);
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Contact created successfully with ID: {ContactId}", contact.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while saving the contact. Please try again.");
                return View(contact);
            }
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

            _logger.LogInformation("Received edit data for contact {Id}: {FirstName} {LastName}", 
                id, contact.FirstName, contact.LastName);
            _logger.LogInformation("Collections - Emails: {EmailCount}, Phones: {PhoneCount}, Addresses: {AddressCount}", 
                contact.Emails?.Count ?? 0, contact.Phones?.Count ?? 0, contact.Addresses?.Count ?? 0);

            if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
            {
                ModelState.AddModelError(string.Empty, "First Name and Last Name are required.");
                return View(contact);
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("ModelState error: {Error}", modelError.ErrorMessage);
                }
                return View(contact);
            }

            try
            {
                var existingContact = await _context.Contacts
                    .Include(c => c.Emails)
                    .Include(c => c.Phones)
                    .Include(c => c.Addresses)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (existingContact == null)
                {
                    return NotFound();
                }

                existingContact.FirstName = contact.FirstName;
                existingContact.LastName = contact.LastName;

                // Initialize and filter collections
                contact.Emails = contact.Emails?.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList() ?? new List<Email>();
                contact.Phones = contact.Phones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<Phone>();
                contact.Addresses = contact.Addresses?.Where(a => 
                    !string.IsNullOrWhiteSpace(a.Street) || 
                    !string.IsNullOrWhiteSpace(a.City) || 
                    !string.IsNullOrWhiteSpace(a.State) || 
                    !string.IsNullOrWhiteSpace(a.Zip)
                ).ToList() ?? new List<Address>();

                // Clear existing collections
                _context.Emails.RemoveRange(existingContact.Emails);
                _context.Phones.RemoveRange(existingContact.Phones);
                _context.Addresses.RemoveRange(existingContact.Addresses);

                // Update collections with new items
                foreach (var email in contact.Emails)
                {
                    email.Contact = existingContact;
                    email.ContactId = existingContact.Id;
                    _context.Emails.Add(email);
                }

                foreach (var phone in contact.Phones)
                {
                    phone.Contact = existingContact;
                    phone.ContactId = existingContact.Id;
                    _context.Phones.Add(phone);
                }

                foreach (var address in contact.Addresses)
                {
                    address.Contact = existingContact;
                    address.ContactId = existingContact.Id;
                    address.Street = address.Street ?? string.Empty;
                    address.City = address.City ?? string.Empty;
                    address.State = address.State ?? string.Empty;
                    address.Zip = address.Zip ?? string.Empty;
                    _context.Addresses.Add(address);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Contact {Id} updated successfully", id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating contact {Id}", id);
                if (!ContactExists(contact.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {Id}: {Message}", id, ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred while saving the contact. Please try again.");
                return View(contact);
            }
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

        // GET: Contacts/ExportCsv
        [Authorize(Roles = "Reader,Editor,Admin")]
        public async Task<IActionResult> ExportCsv()
        {
            var contacts = await _context.Contacts
                .Include(c => c.Emails)
                .Include(c => c.Phones)
                .Include(c => c.Addresses)
                .ToListAsync();

            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("FirstName");
                csv.WriteField("LastName");
                csv.WriteField("Emails");
                csv.WriteField("Phones");
                csv.WriteField("Addresses");
                csv.NextRecord();
                foreach (var c in contacts)
                {
                    csv.WriteField(c.FirstName);
                    csv.WriteField(c.LastName);
                    csv.WriteField(string.Join("; ", c.Emails.Select(e => e.EmailAddress)));
                    csv.WriteField(string.Join("; ", c.Phones.Select(p => p.PhoneNumber)));
                    csv.WriteField(string.Join("; ", c.Addresses.Select(a => $"{a.Street}, {a.City}, {a.State}, {a.Zip}")));
                    csv.NextRecord();
                }
            }
            memoryStream.Position = 0;
            return File(memoryStream.ToArray(), "text/csv", $"contacts_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Contacts/ExportExcel
        [Authorize(Roles = "Reader,Editor,Admin")]
        public async Task<IActionResult> ExportExcel()
        {
            var contacts = await _context.Contacts
                .Include(c => c.Emails)
                .Include(c => c.Phones)
                .Include(c => c.Addresses)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Contacts");
            worksheet.Cell(1, 1).Value = "FirstName";
            worksheet.Cell(1, 2).Value = "LastName";
            worksheet.Cell(1, 3).Value = "Emails";
            worksheet.Cell(1, 4).Value = "Phones";
            worksheet.Cell(1, 5).Value = "Addresses";
            int row = 2;
            foreach (var c in contacts)
            {
                worksheet.Cell(row, 1).Value = c.FirstName;
                worksheet.Cell(row, 2).Value = c.LastName;
                worksheet.Cell(row, 3).Value = string.Join("; ", c.Emails.Select(e => e.EmailAddress));
                worksheet.Cell(row, 4).Value = string.Join("; ", c.Phones.Select(p => p.PhoneNumber));
                worksheet.Cell(row, 5).Value = string.Join("; ", c.Addresses.Select(a => $"{a.Street}, {a.City}, {a.State}, {a.Zip}"));
                row++;
            }
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"contacts_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
