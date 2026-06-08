using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarPath.Web.Models;

namespace SolarPath.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
    { _userManager = um; _signInManager = sm; }

    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string firstName, string lastName, string email, string password)
    {
        var user = new ApplicationUser { FirstName = firstName, LastName = lastName, Email = email, UserName = email };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Tourist");
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Success"] = "Реєстрацію пройдено!";
            return RedirectToAction("Index", "Home");
        }
        foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        return View();
    }

    public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            TempData["Success"] = "Вітаємо!";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "Невірний email або пароль.");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
