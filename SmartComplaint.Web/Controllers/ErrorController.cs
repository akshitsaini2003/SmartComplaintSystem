using Microsoft.AspNetCore.Mvc;

namespace SmartComplaint.Web.Controllers;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult Index(int statusCode)
    {
        return statusCode switch
        {
            404 => View("Error404"),
            403 => View("Error403"),
            _ => View("Error404")
        };
    }
}