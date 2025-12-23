using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartInventoryManagement.Models;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using SmartInventoryManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace SmartInventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
        }

        // Register action (GET)
        [HttpGet]
        public IActionResult Register()
        
        {
            ViewBag.SecurityQuestions = new List<string>
            {
                "Which elementary school did you attend?",
                "What is your favourite movie or book?",
                "What is the name of your best friend?"
            };

            return View();
        }

        // Register action (POST)
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (ModelState.IsValid)
    {
        try
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                ContactInformation = "N/A",
                Address = "N/A",
                Pronouns = "Would rather not say",
                SecurityQuestion = model.SecurityQuestion,
                SecurityAnswer = model.SecurityAnswer
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    code = token
                }, Request.Scheme);

                // Send confirmation email
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

                // Redirect to confirmation prompt
                return View("RegisterConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Registration error: " + ex.Message);
            ModelState.AddModelError(string.Empty, "Unexpected error: " + ex.Message);
        }
    }

    // 🔧 Ensure dropdown list is available again if ModelState is invalid
    ViewBag.SecurityQuestions = new List<string>
    {
        "Which elementary school did you attend?",
        "What is your favourite movie or book?",
        "What is the name of your best friend?"
    };

    return View(model);
}

        // Login (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // Logout (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ForgotPassword (GET)
        [HttpGet]
        public IActionResult ForgotPW()
        {
            ViewBag.SecurityQuestions = new List<string>
            {
                "Which elementary school did you attend?",
                "What is your favourite movie or book?",
                "What is the name of your best friend?"
            };

            return View();
        }

        // ForgotPassword (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPWView model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || user.SecurityQuestion != model.SecurityQuestion || user.SecurityAnswer != model.SecurityAnswer)
                {
                    ModelState.AddModelError(string.Empty, "Invalid security question or answer.");
                    return View(model);
                }

                TempData["ResetEmail"] = user.Email;
                return RedirectToAction("ResetPassword");
            }

            return View(model);
        }

        // ResetPassword (GET)
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["ResetEmail"] == null)
                return RedirectToAction("ForgotPassword");

            return View(new ResetPasswordViewModel { Email = TempData["ResetEmail"].ToString() });
        }

        // ResetPassword (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Profile page (GET)
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.GuestEmail == user.Email && o.CreatedBy == "User")
                .ToList();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Pronouns = user.Pronouns,
                Address = user.Address,
                Orders = orders
            };

            return View(model);
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Pronouns = user.Pronouns,
                Address = user.Address,
                Email = user.Email
            };

            return View(model);
        }
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
        {
            // Remove validation errors for properties that are not bound
            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.Orders));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            user.FullName = model.FullName;
            user.DateOfBirth = model.DateOfBirth?.ToUniversalTime();
            user.Pronouns = model.Pronouns;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return View("Error");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return View("Error");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return result.Succeeded ? View("ConfirmEmail") : View("Error");
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePasswordWithSecurity()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.SecurityQuestion = user?.SecurityQuestion;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordWithSecurity(ChangePasswordWithSecurityViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            if (user.SecurityAnswer != model.SecurityAnswer)
            {
                ModelState.AddModelError(string.Empty, "Incorrect security answer.");
                ViewBag.SecurityQuestion = user.SecurityQuestion;
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.SecurityQuestion = user.SecurityQuestion;
            return View(model);
        }


    }
}
