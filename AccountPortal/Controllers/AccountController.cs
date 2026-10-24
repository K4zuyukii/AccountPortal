using System.Security.Claims;
using AccountPortal.Data;
using AccountPortal.Helpers;
using AccountPortal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AccountPortal.Controllers;

public class AccountController : Controller
{
    private readonly UserRepository _users;

    public AccountController(UserRepository users)
    {
        _users = users;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email);

        if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Incorrect email or password.");
            return View(model);
        }

        await SignInAsync(user);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Signup()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new SignupViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();

        if (await _users.GetByEmailAsync(email) != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with that email already exists.");
            return View(model);
        }

        var user = new User
        {
            Name = model.Name.Trim(),
            Phone = model.Phone.Trim(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(model.Password)
        };
        user.Id = await _users.CreateAsync(user);

        await SignInAsync(user);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }
}
