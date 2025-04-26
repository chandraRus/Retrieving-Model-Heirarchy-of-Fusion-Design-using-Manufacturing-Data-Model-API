using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ModelHierarchyService
{
    private readonly HttpClient _httpClient;

    // Constructor to initialize HttpClient using IHttpClientFactory
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
    /// <returns>Model hierarchy as a JsonElement.</returns>
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

        // Send the query to get the project ID
        var projectIdResponse = await SendQueryAsync<GraphQLProjectResponse>(token, projectIdQuery, new { hubName, projectName });
        string projectId = projectIdResponse.Data.Hubs.Results[0].Projects.Results[0].Id;

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

        // Send the query to get the component version ID
        var componentIdResponse = await SendQueryAsync<ComponentIdResponse>(token, componentIdQuery, new { projectId, componentName });
        string componentVersionId = componentIdResponse.Data.Project.Items.Results[0].TipRootComponentVersion.Id;

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

            // Send the query to get the model hierarchy for the current page
            var hierarchyResponse = await SendQueryAsync<HierarchyResponse>(token, hierarchyQuery, new { componentVersionId, cursor });

            // Add the results to the list
            var occurrences = hierarchyResponse.Data.ComponentVersion.AllOccurrences;
            allResults.AddRange(occurrences.Results);

            // Update cursor for pagination
            cursor = occurrences.Pagination?.Cursor;
        } while (cursor != null); // Continue if there are more pages

        // Step 4: Return the hierarchy as a JsonElement
        var hierarchyJson = JsonSerializer.SerializeToElement(new
        {
            componentVersionId,
            results = allResults
        });

        return hierarchyJson;
    }
}

// Classes for deserializing GraphQL responses

public class GraphQLProjectResponse
{
    public ProjectData Data { get; set; }
}

public class ProjectData
{
    public HubCollection Hubs { get; set; }
}

public class HubCollection
{
    public List<HubResult> Results { get; set; }
}

public class HubResult
{
    public ProjectCollection Projects { get; set; }
}

public class ProjectCollection
{
    public List<Project> Results { get; set; }
}

public class Project
{
    public string Id { get; set; }
}

public class ComponentIdResponse
{
    public ComponentProjectData Data { get; set; }
}

public class ComponentProjectData
{
    public ComponentProject Project { get; set; }
}

public class ComponentProject
{
    public ComponentItems Items { get; set; }
}

public class ComponentItems
{
    public List<ComponentItemResult> Results { get; set; }
}

public class ComponentItemResult
{
    public TipRootComponentVersion TipRootComponentVersion { get; set; }
}

public class TipRootComponentVersion
{
    public string Id { get; set; }
}

public class HierarchyResponse
{
    public HierarchyData Data { get; set; }
}

public class HierarchyData
{
    public ComponentVersionNode ComponentVersion { get; set; }
}

public class ComponentVersionNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public AllOccurrences AllOccurrences { get; set; }
}

public class AllOccurrences
{
    public List<OccurrenceResult> Results { get; set; }
    public PaginationInfo Pagination { get; set; }
}

public class OccurrenceResult
{
    public ParentComponent ParentComponentVersion { get; set; }
    public ChildComponent ComponentVersion { get; set; }
}

public class ParentComponent
{
    public string Id { get; set; }
}

public class ChildComponent
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class PaginationInfo
{
    public string Cursor { get; set; }
}
