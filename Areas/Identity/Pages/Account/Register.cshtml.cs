using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ContactBook.Models;

namespace ContactBook.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "First Name is required")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Last Name is required")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            // Do NOT reset Input here
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    if (entry?.Errors != null)
                    {
                        var errors = entry.Errors;
                        foreach (var error in errors)
                        {
                            _logger.LogWarning($"ModelState error for {key}: {error.ErrorMessage}");
                            Console.WriteLine($"ModelState error for {key}: {error.ErrorMessage}");
                        }
                    }
                }
                return Page();
            }

            try
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    EmailConfirmed = true
                };

                _logger.LogInformation("Attempting to create user {Email}", Input.Email);
                Console.WriteLine($"Attempting to create user {Input.Email}");
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} created successfully", Input.Email);
                    Console.WriteLine($"User {Input.Email} created successfully");
                    // Add user to Reader role
                    var roleResult = await _userManager.AddToRoleAsync(user, "Reader");
                    if (!roleResult.Succeeded)
                    {
                        var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to add user to Reader role: {Errors}", roleErrors);
                        Console.WriteLine($"Failed to add user to Reader role: {roleErrors}");
                        ModelState.AddModelError(string.Empty, "Failed to assign user role.");
                        return Page();
                    }
                    _logger.LogInformation("User {Email} added to Reader role", Input.Email);
                    Console.WriteLine($"User {Input.Email} added to Reader role");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogError("User creation failed: {Error}", error.Description);
                    Console.WriteLine($"User creation failed: {error.Description}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during registration");
                Console.WriteLine($"Exception during registration: {ex.Message}\n{ex.StackTrace}");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            }

            return Page();
        }
    }
}