using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartComplaint.Web.Models;
using SmartComplaint.Web.Services;
using System.Security.Claims;

namespace SmartComplaint.Web.Controllers;

public class AuthController : Controller
{
    private readonly ApiService _api;

    public AuthController(ApiService api) => _api = api;

    // ─── Login ───────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectByRole();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _api.PostAsync<LoginResponseModel>("api/auth/login", new
        {
            email = model.Email,
            password = model.Password
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage ?? "Invalid credentials.";
            return View(model);
        }

        var data = result.Data!;

        // Store tokens in cookies
        Response.Cookies.Append("AccessToken", data.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15)
        });
        Response.Cookies.Append("RefreshToken", data.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        // Sign in with claims
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()), // ← ADD
        new Claim(ClaimTypes.Name,  data.Name),
        new Claim(ClaimTypes.Email, data.Email),
        new Claim(ClaimTypes.Role,  data.Role),
    };

        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // ✅ Use data.Role directly — not User.IsInRole()
        return data.Role switch
        {
            "Admin" => Redirect("/Dashboard/Admin"),
            "Agent" => Redirect("/Dashboard/Agent"),
            _ => Redirect("/Dashboard/User")
        };
    }

    // ─── Register ────────────────────────────────────────────
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var result = await _api.PostAsync<object>("api/auth/register", new
        {
            name = model.Name,
            email = model.Email,
            password = model.Password
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(model);
        }

        TempData["Success"] = "OTP sent to your email!";
        TempData["Email"] = model.Email;
        return RedirectToAction("VerifyOtp");
    }

    // ─── Verify OTP ──────────────────────────────────────────
    [HttpGet]
    public IActionResult VerifyOtp()
    {
        var model = new VerifyOtpViewModel
        {
            Email = TempData["Email"]?.ToString() ?? ""
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        var result = await _api.PostAsync<object>("api/auth/verify-otp", new
        {
            email = model.Email,
            otp = model.OTP
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(model);
        }

        TempData["Success"] = "Email verified! Please login.";
        return RedirectToAction("Login");
    }

    // ─── Forgot Password ─────────────────────────────────────
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        var result = await _api.PostAsync<object>("api/auth/forgot-password", new
        {
            email = model.Email
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(model);
        }

        TempData["Success"] = "Password reset link sent to your email!";
        return View();
    }

    // ─── Reset Password ──────────────────────────────────────
    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        var model = new ResetPasswordViewModel { Token = token };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (model.NewPassword != model.ConfirmPassword)
        {
            TempData["Error"] = "Passwords do not match.";
            return View(model);
        }

        var result = await _api.PostAsync<object>("api/auth/reset-password", new
        {
            token = model.Token,
            newPassword = model.NewPassword
        });

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(model);
        }

        TempData["Success"] = "Password reset successfully! Please login.";
        return RedirectToAction("Login");
    }

    // ─── Logout ──────────────────────────────────────────────
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _api.PostAsync<object>("api/auth/logout", new
            {
                refreshToken = refreshToken
            });
        }

        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login");
    }

    // ─── Access Denied ───────────────────────────────────────
    public IActionResult AccessDenied() => View();

    // ─── Helper ──────────────────────────────────────────────
    private IActionResult RedirectByRole()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return role switch
        {
            "Admin" => Redirect("/Dashboard/Admin"),
            "Agent" => Redirect("/Dashboard/Agent"),
            _ => Redirect("/Dashboard/User")
        };
    }
}