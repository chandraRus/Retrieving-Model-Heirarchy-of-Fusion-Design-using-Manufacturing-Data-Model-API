using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ModelHierarchyApp.Models;
using ModelHierarchyApp.Services;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModelHierarchyApp.Controllers
{
    /// <summary>
    /// Controller responsible for retrieving and displaying the model hierarchy for Fusion designs.
    /// </summary>
    public class ModelHierarchyController : Controller
    {
        private readonly OAuthService _oauthService;
        private readonly ModelHierarchyService _modelHierarchyService;
        private readonly GraphQLService _graphQLService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelHierarchyController"/> class.
        /// </summary>
        /// <param name="oauthService">Service for handling OAuth authentication.</param>
        /// <param name="modelHierarchyService">Service for retrieving model hierarchy data.</param>
        /// <param name="graphQLService">Service for executing GraphQL queries.</param>
        public ModelHierarchyController(
            OAuthService oauthService,
            ModelHierarchyService modelHierarchyService,
            GraphQLService graphQLService)
        {
            _oauthService = oauthService;
            _modelHierarchyService = modelHierarchyService;
            _graphQLService = graphQLService;
        }

        /// <summary>
        /// Returns the tree data for the jsTree component, representing hubs, projects, folders, and items.
        /// </summary>
        /// <param name="id">The node id to expand (root is "#").</param>
        /// <returns>A JSON array of <see cref="TreeNode"/> objects for jsTree.</returns>
        public async Task<IActionResult> TreeData(string id)
        {
            var token = _oauthService.GetAccessTokenFromSession();
            var nodes = new List<TreeNode>();

            if (id == "#" || string.IsNullOrEmpty(id)) // Hubs
            {
                var hubs = await _graphQLService.GetHubsAsync(token);
                foreach (var hub in hubs)
                {
                    nodes.Add(new TreeNode
                    {
                        id = $"hub_{hub.id}",
                        parent = "#",
                        text = hub.name ?? "Unnamed Hub",
                        type = "hub",
                        children = true
                    });
                }
            }
            else if (id.StartsWith("hub_")) // Projects
            {
                var hubId = id.Substring(4);
                var projects = await _graphQLService.GetProjectsAsync(token, hubId);
                foreach (var project in projects)
                {
                    nodes.Add(new TreeNode
                    {
                        id = $"project_{hubId}_{project.id}", // Embed hubId in project ID
                        parent = id,
                        text = project.name,
                        type = "project",
                        children = true
                    });
                }
            }
            else if (id.StartsWith("project_")) // Items and Folders
            {
                var projectParts = id.Substring(8).Split('_', 2); // project_{hubId}_{projectId}
                var hubId = projectParts[0];
                var projectId = projectParts[1];

                // Items
                var itemsJson = await _graphQLService.GetFolderItemsByProjectAsync(projectId, token);
                var items = itemsJson.GetProperty("data").GetProperty("itemsByProject").GetProperty("results");
                foreach (var item in items.EnumerateArray())
                {
                    nodes.Add(new TreeNode
                    {
                        id = $"item_{item.GetProperty("id").GetString()}",
                        parent = id,
                        text = $"{item.GetProperty("__typename").GetString()}: {item.GetProperty("name").GetString()}",
                        type = "item",
                        children = false
                    });
                }

                // Folders
                var folders = await _graphQLService.GetFoldersByProjectAsync(token, projectId);
                foreach (var folder in folders)
                {
                    nodes.Add(new TreeNode
                    {
                        id = $"folder_{hubId}_{folder.Id}", // Embed hubId in folder ID
                        parent = id,
                        text = folder.Name,
                        type = "folder",
                        children = true
                    });
                }
            }
            else if (id.StartsWith("folder_")) // Nested folders/items
            {
                var folderParts = id.Substring(7).Split('_', 2); // folder_{hubId}_{folderId}
                var hubId = folderParts[0];
                var folderId = folderParts[1];

                var resultJson = await _graphQLService.GetFolderItemsAsync(token, hubId, folderId);
                var items = resultJson.GetProperty("data").GetProperty("itemsByFolder").GetProperty("results");

                foreach (var item in items.EnumerateArray())
                {
                    var typename = item.GetProperty("__typename").GetString();
                    var isFolder = typename == "Folder";
                    nodes.Add(new TreeNode
                    {
                        id = isFolder
                            ? $"folder_{hubId}_{item.GetProperty("id").GetString()}" // Keep passing hubId
                            : $"item_{item.GetProperty("id").GetString()}",
                        parent = id,
                        text = $"{typename}: {item.GetProperty("name").GetString()}",
                        type = isFolder ? "folder" : "item",
                        children = isFolder
                    });
                }
            }

            return Json(nodes);
        }

        /// <summary>
        /// Returns the main view for the model hierarchy browser.
        /// </summary>
        /// <returns>The Index view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns the details and hierarchy of a selected component as a partial view.
        /// </summary>
        /// <param name="itemId">The ID of the selected item.</param>
        /// <param name="componentName">The name of the selected component.</param>
        /// <param name="hubName">The name of the hub containing the component.</param>
        /// <param name="projectName">The name of the project containing the component.</param>
        /// <returns>A partial view with the component hierarchy.</returns>
        public async Task<IActionResult> ItemDetails(string itemId, string componentName, string hubName, string projectName)
        {
            var token = _oauthService.GetAccessTokenFromSession();
            var hierarchyJson = await _modelHierarchyService.GetModelHierarchyAsync(token, hubName, projectName, componentName);

            var componentVersionId = hierarchyJson.GetProperty("componentVersionId").GetString();
            var hierarchyString = hierarchyJson.GetProperty("hierarchy").GetString();
            var jsTreeNodes = ParseHierarchyStringToJsTree(hierarchyString); // Your method

            var model = new ComponentViewModel
            {
                RootComponentName = componentName,
                ComponentVersionId = componentVersionId,
                HierarchyText = JsonConvert.SerializeObject(jsTreeNodes)
            };

            return PartialView("_ComponentHierarchyPartial", model);
        }

        /// <summary>
        /// Parses a hierarchy string into a list of <see cref="JsTreeNode"/> objects for jsTree.
        /// </summary>
        /// <param name="hierarchy">The hierarchy string, with indentation representing levels.</param>
        /// <returns>A list of root <see cref="JsTreeNode"/> objects.</returns>
        private List<JsTreeNode> ParseHierarchyStringToJsTree(string hierarchy)
        {
            var lines = hierarchy.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(line => new {
                                     Level = line.TakeWhile(char.IsWhiteSpace).Count() / 4, // assuming 4-space indent
                                     Text = line.Trim()
                                 }).ToList();

            var stack = new Stack<(int Level, JsTreeNode Node)>();
            var roots = new List<JsTreeNode>();

            foreach (var line in lines)
            {
                var node = new JsTreeNode { text = line.Text };

                while (stack.Count > 0 && stack.Peek().Level >= line.Level)
                    stack.Pop();

                if (stack.Count == 0)
                {
                    roots.Add(node);
                }
                else
                {
                    var parent = stack.Peek().Node;
                    parent.children ??= new List<JsTreeNode>();
                    parent.children.Add(node);
                }

                stack.Push((line.Level, node));
            }

            return roots;
        }

        /// <summary>
        /// Converts a <see cref="ComponentNode"/> to a <see cref="JsTreeNode"/> for jsTree.
        /// </summary>
        /// <param name="component">The component node to convert.</param>
        /// <returns>A <see cref="JsTreeNode"/> representing the component.</returns>
        private JsTreeNode ConvertToJsTreeNode(ComponentNode component)
        {
            return new JsTreeNode
            {
                text = component.Name,
                children = component.Children?.Select(ConvertToJsTreeNode).ToList()
            };
        }   
    }

    /// <summary>
    /// Represents a Fusion Team hub.
    /// </summary>
    public class Hub
    {
        /// <summary>
        /// Gets or sets the hub ID.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Gets or sets the hub name.
        /// </summary>
        public string name { get; set; }
    }
}
