using System.Security.Claims;
using AccountPortal.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountPortal.Controllers;

public class HomeController : Controller
{
    private readonly UserRepository _users;

    public HomeController(UserRepository users)
    {
        _users = users;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var idText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idText, out var id)) return await ForceLogout();

        var user = await _users.GetByIdAsync(id);
        if (user == null) return await ForceLogout();

        return View(user);
    }

    public IActionResult Error() => Content("Something went wrong. Please try again.");

    private async Task<IActionResult> ForceLogout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}
