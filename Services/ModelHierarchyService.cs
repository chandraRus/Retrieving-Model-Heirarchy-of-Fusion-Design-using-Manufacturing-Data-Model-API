using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using ModelHierarchyApp.Models;

/// <summary>
/// Service for retrieving and building the model hierarchy for Fusion designs using Autodesk GraphQL APIs.
/// </summary>
public class ModelHierarchyService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelHierarchyService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    public ModelHierarchyService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Sends a GraphQL query to the Autodesk API and returns the response as a generic type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="token">OAuth token for authentication.</param>
    /// <param name="query">GraphQL query string.</param>
    /// <param name="variables">Variables for the query.</param>
    /// <returns>Deserialized response of type T.</returns>
    private async Task<T> SendQueryAsync<T>(string token, string query, object variables)
    {
        // Create the HTTP request
        var request = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/mfg/graphql");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create the payload with the query and variables
        var payload = new
        {
            query,
            variables
        };

        // Serialize the payload to JSON and set it as the request content
        var json = JsonSerializer.Serialize(payload);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        // Send the request and ensure success
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Read and deserialize the response body
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Retrieves the model hierarchy for a specific project and component.
    /// </summary>
    /// <param name="token">OAuth token for authentication.</param>
    /// <param name="hubName">Name of the hub.</param>
    /// <param name="projectName">Name of the project.</param>
    /// <param name="componentName">Name of the component.</param>
    /// <returns>Model hierarchy as a <see cref="JsonElement"/>.</returns>
    public async Task<JsonElement> GetModelHierarchyAsync(string token, string hubName, string projectName, string componentName)
    {
        // Step 1: Get Project ID
        var projectIdQuery = @"
    query GetProjectId($hubName: String!, $projectName: String!) {
        hubs(filter:{name:$hubName}) {
            results {
                projects(filter:{name:$projectName}) {
                    results {
                        id
                    }
                }
            }
        }
    }";

        var projectIdResponse = await SendQueryAsync<GraphQLProjectResponse>(token, projectIdQuery, new { hubName, projectName });
        string projectId = projectIdResponse?.Data?.Hubs?.Results?.FirstOrDefault()?.Projects?.Results?.FirstOrDefault()?.Id;

        if (string.IsNullOrEmpty(projectId))
        {
            throw new Exception($"Project '{projectName}' not found under hub '{hubName}'.");
        }

        // Step 2: Get Component Version ID
        var componentIdQuery = @"
    query GetComponentVersionId($projectId: ID!, $componentName: String!) {
        project(projectId: $projectId) {
            items(filter:{name:$componentName}) {
                results {
                    ... on DesignItem {
                        tipRootComponentVersion {
                            id                                       	            
                        }
                    }
                }
            }
        }
    }";

        var componentIdResponse = await SendQueryAsync<ComponentIdResponse>(token, componentIdQuery, new { projectId, componentName });

        var componentResult = componentIdResponse?.Data?.Project?.Items?.Results?.FirstOrDefault();
        var componentVersionId = componentResult?.TipRootComponentVersion?.Id;

        // If componentVersionId is null, return only the root name
        if (string.IsNullOrEmpty(componentVersionId))
        {
            var fallbackJson = JsonSerializer.SerializeToElement(new
            {
                componentVersionId = (string)null,
                hierarchy = $"Root ({componentName})\n"
            });

            return fallbackJson;
        }

        // Step 3: Paginate Model Hierarchy
        var allResults = new List<OccurrenceResult>();
        string cursor = null;

        do
        {
            var hierarchyQuery = @"
        query GetModelHierarchy($componentVersionId: ID!, $cursor: String) {
            componentVersion(componentVersionId: $componentVersionId) {
                id
                name
                allOccurrences(pagination: {cursor: $cursor}) {
                    results {
                        parentComponentVersion { id }
                        componentVersion { id name }
                    }
                    pagination { cursor }
                }
            }
        }";

            var hierarchyResponse = await SendQueryAsync<HierarchyResponse>(token, hierarchyQuery, new { componentVersionId, cursor });
            var occurrences = hierarchyResponse?.Data?.ComponentVersion?.AllOccurrences;

            if (occurrences?.Results != null)
            {
                allResults.AddRange(occurrences.Results);
                cursor = occurrences.Pagination?.Cursor;
            }
            else
            {
                cursor = null;
            }

        } while (cursor != null);

        // Step 4: Build and return hierarchy
        var rootNode = BuildHierarchyTree(allResults, componentVersionId, componentName);
        var sb = new StringBuilder();
        sb.AppendLine($"Root ({componentName})");
        TraverseHierarchy(rootNode, 1, sb);

        var hierarchyString = sb.ToString();
        var hierarchyJson = JsonSerializer.SerializeToElement(new
        {
            componentVersionId,
            hierarchy = hierarchyString
        });

        return hierarchyJson;
    }

    /// <summary>
    /// Builds a tree structure from a flat list of occurrence results.
    /// </summary>
    /// <param name="flatList">Flat list of <see cref="OccurrenceResult"/> representing parent-child relationships.</param>
    /// <param name="rootId">The ID of the root component.</param>
    /// <param name="rootName">The name of the root component.</param>
    /// <returns>The root <see cref="ComponentNode"/> of the hierarchy tree.</returns>
    private ComponentNode BuildHierarchyTree(List<OccurrenceResult> flatList, string rootId, string rootName)
    {
        var lookup = new Dictionary<string, ComponentNode>();

        // Create nodes
        foreach (var occ in flatList)
        {
            var childId = occ.ComponentVersion.Id;
            if (!lookup.ContainsKey(childId))
            {
                lookup[childId] = new ComponentNode
                {
                    Id = childId,
                    Name = occ.ComponentVersion.Name
                };
            }
        }

        ComponentNode root = new ComponentNode
        {
            Id = rootId,
            Name = rootName
        };

        lookup[rootId] = root;

        // Build parent-child relationships
        foreach (var occ in flatList)
        {
            var child = lookup[occ.ComponentVersion.Id];
            var parentId = occ.ParentComponentVersion?.Id;

            if (parentId == null || parentId == rootId)
            {
                root.Children.Add(child);
            }
            else if (lookup.TryGetValue(parentId, out var parent))
            {
                parent.Children.Add(child);
            }
        }

        return root;
    }

    /// <summary>
    /// Traverses the hierarchy tree and appends each node's name to a <see cref="StringBuilder"/> with indentation.
    /// </summary>
    /// <param name="node">The current <see cref="ComponentNode"/>.</param>
    /// <param name="level">The current depth level in the tree.</param>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    private void TraverseHierarchy(ComponentNode node, int level, StringBuilder sb)
    {
        if (node == null) return;

        var indent = new string(' ', level * 5);
        foreach (var child in node.Children)
        {
            sb.AppendLine($"{indent}{child.Name}");
            TraverseHierarchy(child, level + 1, sb);
        }
    }
}

/// <summary>
/// Represents a node in the component hierarchy tree.
/// </summary>
public class ComponentNode
{
    /// <summary>
    /// Gets or sets the component ID.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Gets or sets the component name.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the child components.
    /// </summary>
    public List<ComponentNode> Children { get; set; } = new();
}

// Classes for deserializing GraphQL responses

/// <summary>
/// Represents the response for a project GraphQL query.
/// </summary>
public class GraphQLProjectResponse
{
    /// <summary>
    /// Gets or sets the project data.
    /// </summary>
    public ProjectData Data { get; set; }
}

/// <summary>
/// Contains hub collection data.
/// </summary>
public class ProjectData
{
    /// <summary>
    /// Gets or sets the collection of hubs.
    /// </summary>
    public HubCollection Hubs { get; set; }
}

/// <summary>
/// Represents a collection of hubs.
/// </summary>
public class HubCollection
{
    /// <summary>
    /// Gets or sets the list of hub results.
    /// </summary>
    public List<HubResult> Results { get; set; }
}

/// <summary>
/// Represents a hub result containing projects.
/// </summary>
public class HubResult
{
    /// <summary>
    /// Gets or sets the collection of projects.
    /// </summary>
    public ProjectCollection Projects { get; set; }
    /// <summary>
    /// Gets or sets the hub name.
    /// </summary>
    public string? name { get; set; }
    /// <summary>
    /// Gets or sets the hub id.
    /// </summary>
    public string? id { get; set; }
}

/// <summary>
/// Represents a collection of projects.
/// </summary>
public class ProjectCollection
{
    /// <summary>
    /// Gets or sets the list of project results.
    /// </summary>
    public List<Project> Results { get; set; }
}

/// <summary>
/// Represents a project.
/// </summary>
public class Project
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public string Id { get; set; }
}

/// <summary>
/// Represents the response for a component ID GraphQL query.
/// </summary>
public class ComponentIdResponse
{
    /// <summary>
    /// Gets or sets the component project data.
    /// </summary>
    public ComponentProjectData Data { get; set; }
}

/// <summary>
/// Contains component project data.
/// </summary>
public class ComponentProjectData
{
    /// <summary>
    /// Gets or sets the component project.
    /// </summary>
    public ComponentProject Project { get; set; }
}

/// <summary>
/// Represents a component project.
/// </summary>
public class ComponentProject
{
    /// <summary>
    /// Gets or sets the items in the project.
    /// </summary>
    public ComponentItems Items { get; set; }
}

/// <summary>
/// Represents a collection of component items.
/// </summary>
public class ComponentItems
{
    /// <summary>
    /// Gets or sets the list of component item results.
    /// </summary>
    public List<ComponentItemResult> Results { get; set; }
}

/// <summary>
/// Represents a component item result.
/// </summary>
public class ComponentItemResult
{
    /// <summary>
    /// Gets or sets the tip root component version.
    /// </summary>
    public TipRootComponentVersion TipRootComponentVersion { get; set; }
}

/// <summary>
/// Represents the tip root component version.
/// </summary>
public class TipRootComponentVersion
{
    /// <summary>
    /// Gets or sets the component version ID.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Gets or sets the component version name.
    /// </summary>
    public string Name { get; set; }    
} 
/// <summary>
/// Represents the response for a hierarchy GraphQL query.
/// </summary>
public class HierarchyResponse
{
    /// <summary>
    /// Gets or sets the hierarchy data.
    /// </summary>
    public HierarchyData Data { get; set; }
}

/// <summary>
/// Contains hierarchy data.
/// </summary>
public class HierarchyData
{
    /// <summary>
    /// Gets or sets the component version node.
    /// </summary>
    public ComponentVersionNode ComponentVersion { get; set; }
}

/// <summary>
/// Represents a component version node.
/// </summary>
public class ComponentVersionNode
{
    /// <summary>
    /// Gets or sets the component version ID.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Gets or sets the component version name.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets all occurrences of the component version.
    /// </summary>
    public AllOccurrences AllOccurrences { get; set; }
}

/// <summary>
/// Represents all occurrences of a component version.
/// </summary>
public class AllOccurrences
{
    /// <summary>
    /// Gets or sets the list of occurrence results.
    /// </summary>
    public List<OccurrenceResult> Results { get; set; }
    /// <summary>
    /// Gets or sets the pagination info.
    /// </summary>
    public PaginationInfo Pagination { get; set; }
}

/// <summary>
/// Represents an occurrence result in the hierarchy.
/// </summary>
public class OccurrenceResult
{
    /// <summary>
    /// Gets or sets the parent component version.
    /// </summary>
    public ParentComponent ParentComponentVersion { get; set; }
    /// <summary>
    /// Gets or sets the child component version.
    /// </summary>
    public ChildComponent ComponentVersion { get; set; }
}

/// <summary>
/// Represents a parent component in the hierarchy.
/// </summary>
public class ParentComponent
{
    /// <summary>
    /// Gets or sets the parent component ID.
    /// </summary>
    public string Id { get; set; }
}

/// <summary>
/// Represents a child component in the hierarchy.
/// </summary>
public class ChildComponent
{
    /// <summary>
    /// Gets or sets the child component ID.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Gets or sets the child component name.
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Represents pagination information for GraphQL queries.
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// Gets or sets the pagination cursor.
    /// </summary>
    public string Cursor { get; set; }
}
