using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ModelHierarchyApp.Models;
using ModelHierarchyApp.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModelHierarchyApp.Controllers
{
    /// <summary>
    /// Handles the logic for retrieving and displaying model hierarchy.
    /// </summary>
    public class ModelHierarchyController : Controller
    {
        // Private readonly fields for services used in the controller
        private readonly OAuthService _oauthService;
        private readonly ModelHierarchyService _modelHierarchyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelHierarchyController"/> class.
        /// </summary>
        public ModelHierarchyController(
            OAuthService oauthService,
            ModelHierarchyService modelHierarchyService)
        {
            _oauthService = oauthService;
            _modelHierarchyService = modelHierarchyService;
        }

        /// <summary>
        /// Action method that retrieves the model hierarchy and passes it to the view.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Retrieve the access token from the session using OAuthService
            var token = _oauthService.GetAccessTokenFromSession();

            // If the token is null or empty, redirect to the OAuth login
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "OAuth");
            }

            // Hardcoded values for hub, project, and component
            var hubName = "<YOUR_HUB_NAME>";
            var projectName = "<YOUR_PROJECT_NAME>";
            var componentName = "<YOUR_COMPONENT_NAME>";

            // Retrieve the model hierarchy JSON using ModelHierarchyService
            var hierarchyJson = await _modelHierarchyService.GetModelHierarchyAsync(
                token, hubName, projectName, componentName);

            // Convert the retrieved JSON object to string for further processing
            var hierarchyJsonString = hierarchyJson.ToString();

            // Parse the JSON string into a JsonDocument for easy manipulation
            var jsonDoc = JsonDocument.Parse(hierarchyJsonString);

            // Extract the root component ID from the JSON document
            var rootComponentId = jsonDoc.RootElement.GetProperty("componentVersionId").GetString();

            // List to hold component names extracted from the model hierarchy
            var componentNames = new List<string>();

            // Enumerate through the "results" array and extract component names
            foreach (var result in jsonDoc.RootElement.GetProperty("results").EnumerateArray())
            {
                // Access the "ComponentVersion" property for each result
                var componentVersion = result.GetProperty("ComponentVersion");

                // Extract the name of the component
                var name = componentVersion.GetProperty("Name").GetString();

                // Add the component name to the list
                componentNames.Add(name);
            }

            // Create a view model to pass to the view
            var viewModel = new ComponentViewModel
            {
                RootComponentName = componentName, // Root component name (e.g., "Utility Knife")
                ComponentNames = componentNames   // List of component names
            };

            // Return the Index view and pass the view model to it
            return View("Index", viewModel);
        }
    }
}
