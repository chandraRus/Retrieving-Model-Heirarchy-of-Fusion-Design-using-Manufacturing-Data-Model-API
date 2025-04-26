using Microsoft.AspNetCore.Mvc;
using ModelHierarchyApp.Services;

namespace ModelHierarchyApp.Controllers
{
    /// <summary>
    /// Handles the home page logic, including retrieving and displaying the model hierarchy.
    /// </summary>
    public class HomeController : Controller
    {
        // Private readonly field for the ModelHierarchyService used in the controller
        private readonly ModelHierarchyService _modelHierarchyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        public HomeController(ModelHierarchyService modelHierarchyService)
        {
            _modelHierarchyService = modelHierarchyService;
        }

        /// <summary>
        /// Action method that retrieves the model hierarchy based on the provided hub, project, and component IDs.
        /// </summary>
        /// <param name="hub">The ID of the hub (default: "yourHubId").</param>
        /// <param name="project">The ID of the project (default: "yourProjectId").</param>
        /// <param name="component">The ID of the component (default: "yourComponentId").</param>
        /// <returns>The view displaying the model hierarchy.</returns>
        public async Task<IActionResult> Index(string hub = "yourHubId", string project = "yourProjectId", string component = "yourComponentId")
        {
            // Retrieve the access token from the session
            var token = HttpContext.Session.GetString("AccessToken");

            // If the token is null or empty, redirect to OAuth login for authentication
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "OAuth");
            }

            // Asynchronously retrieve the model hierarchy using the provided IDs and access token
            var model = await _modelHierarchyService.GetModelHierarchyAsync(token, hub, project, component);

            // Return the view, passing the model data to it
            return View(model);
        }
    }
}
