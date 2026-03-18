using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartComplaint.Web.Models;
using SmartComplaint.Web.Services;

namespace SmartComplaint.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApiService _api;
    public UsersController(ApiService api) => _api = api;

    // GET /Users
    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _api.GetAsync<PagedResult<UserModel>>(
            $"api/users?page={page}&pageSize=10")
            ?? new PagedResult<UserModel>();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = result.TotalPages;
        return View(result.Items);
    }

    // GET /Users/Agents
    public async Task<IActionResult> Agents()
    {
        var agents = await _api.GetAsync<List<UserModel>>("api/users/agents")
                     ?? new List<UserModel>();
        return View(agents);
    }

    // POST /Users/Create
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserModel model)
    {
        var result = await _api.PostAsync<UserModel>("api/users", new
        {
            name = model.Name,
            email = model.Email,
            password = model.Password,
            role = model.Role
        });

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = $"User '{model.Name}' created successfully!";

        return RedirectToAction("Index");
    }

    // POST /Users/Update
    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, bool isActive)
    {
        var result = await _api.PutAsync<UserModel>($"api/users/{id}", new
        {
            name,
            isActive
        });

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "User updated successfully!";

        return RedirectToAction("Index");
    }

    // POST /Users/UpdateRole
    [HttpPost]
    public async Task<IActionResult> UpdateRole(int id, string role)
    {
        var result = await _api.PatchAsync<object>($"api/users/{id}/role", new { role });

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = $"Role updated to {role}!";

        return RedirectToAction("Index");
    }

    // GET /Users/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _api.DeleteAsync<object>($"api/users/{id}");

        if (!result.IsSuccess)
            TempData["Error"] = result.ErrorMessage;
        else
            TempData["Success"] = "User deactivated successfully!";

        return RedirectToAction("Index");
    }
}