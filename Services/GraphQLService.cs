using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using ModelHierarchyApp.Models;

namespace ModelHierarchyApp.Services
{
    /// <summary>
    /// Service for interacting with Autodesk GraphQL APIs to retrieve hubs, projects, folders, and items.
    /// </summary>
    public class GraphQLService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used for API requests.</param>
        public GraphQLService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves all hubs accessible with the given access token.
        /// </summary>
        /// <param name="accessToken">OAuth access token.</param>
        /// <returns>List of <see cref="HubResult"/> objects.</returns>
        public async Task<List<ModelHierarchyApp.Models.HubResult>> GetHubsAsync(string accessToken)
        {
            var query = new
            {
                query = @"query {
                                 hubs {
                                        results {
                                                    id
                                                    name
                                                            }
                                                                }
                                                                    }"
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(query),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync("https://developer.api.autodesk.com/graphql", requestContent);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<GraphQLResponse>(
                responseStream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            return result?.Data?.Hubs?.Results ?? new();
        }

        /// <summary>
        /// Retrieves all projects for a given hub.
        /// </summary>
        /// <param name="accessToken">OAuth access token.</param>
        /// <param name="hubId">The ID of the hub.</param>
        /// <returns>List of <see cref="ProjectResult"/> objects.</returns>
        public async Task<List<ModelHierarchyApp.Models.ProjectResult>> GetProjectsAsync(string accessToken, string hubId)
        {
            const string graphqlQuery = @"
        query GetProjects($hubId: ID!, $filter: ProjectFilterInput) {
          projects(hubId: $hubId, filter: $filter) {
            results {
              id
              name
              __typename
              alternativeIdentifiers {
                dataManagementAPIProjectId
              }
            }
          }
        }";

            var request = new GraphQLRequest
            {
                query = graphqlQuery,
                variables = new
                {
                    hubId = hubId,
                    filter = new { }
                }
            };

            var requestJson = JsonSerializer.Serialize(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync("https://developer.api.autodesk.com/mfg/graphql", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"GraphQL query failed: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            }

            var responseStream = await response.Content.ReadAsStreamAsync();
            var graphQLResponse = await JsonSerializer.DeserializeAsync<GraphProjectsQLResponse>(responseStream);

            return graphQLResponse?.data?.projects?.results ?? new List<ModelHierarchyApp.Models.ProjectResult>();
        }

        /// <summary>
        /// Retrieves all folders for a given project.
        /// </summary>
        /// <param name="accessToken">OAuth access token.</param>
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>List of <see cref="FolderResult"/> objects.</returns>
        public async Task<List<FolderResult>> GetFoldersByProjectAsync(string accessToken, string projectId)
        {
            var queryObject = new
            {
                query = @"
            query GetFolders($projectId: ID!) {
                foldersByProject(projectId: $projectId) {
                    pagination {
                        cursor
                        pageSize
                    }
                    results {
                        id
                        name
                        objectCount
                    }
                }
            }",
                variables = new
                {
                    projectId = projectId
                }
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(queryObject),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync("https://developer.api.autodesk.com/graphql", requestContent);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<GetFoldersResponse>(
                responseStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Data?.FoldersByProject?.Results ?? new List<FolderResult>();
        }

        /// <summary>
        /// Retrieves all folders across all hubs and projects accessible with the given token.
        /// </summary>
        /// <param name="token">OAuth access token.</param>
        /// <returns>List of <see cref="FolderItem"/> objects.</returns>
        public async Task<List<FolderItem>> GetAllFolders(string token)
        {
            var allItems = new List<FolderItem>();
            var hubs = await GetHubsAsync(token);
            foreach (var hub in hubs)
            {
                var projects = await GetProjectsAsync(token, hub.id);
                foreach (var project in projects)
                {
                    var folders = await GetFoldersByProjectAsync(token, project.id);
                    foreach (var folder in folders)
                    {
                        allItems.Add(new FolderItem
                        {
                            TypeName = "folders",
                            Name = folder.Name,
                            HubId = hub.id,
                            ProjectId = project.id,
                            FolderId = folder.Id
                        });
                    }
                }
            }
            return allItems;
        }

        /// <summary>
        /// Retrieves all items for a given project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="accessToken">OAuth access token.</param>
        /// <returns>A <see cref="JsonElement"/> containing the items.</returns>
        public async Task<JsonElement> GetFolderItemsByProjectAsync(string projectId, string accessToken)
        {
            var queryObj = new
            {
                query = @"
            query GetFolderItemsByProject($projectId: ID!) {
                itemsByProject(projectId: $projectId) {
                    results {
                        __typename
                        id
                        name
                    }
                }
            }",
                variables = new { projectId = projectId }
            };

            var json = JsonSerializer.Serialize(queryObj);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/graphql")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"GraphQL Error {response.StatusCode}:\n{content}");

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.Clone();
        }

        /// <summary>
        /// Retrieves all items for a given folder.
        /// </summary>
        /// <param name="accessToken">OAuth access token.</param>
        /// <param name="hubId">The ID of the hub.</param>
        /// <param name="folderId">The ID of the folder.</param>
        /// <returns>A <see cref="JsonElement"/> containing the items.</returns>
        public async Task<JsonElement> GetFolderItemsAsync(string accessToken, string hubId, string folderId)
        {
            var query = @"
                            query GetFolderItemsByFolder($hubId: ID!, $folderId: ID!) {
                              itemsByFolder(hubId: $hubId, folderId: $folderId) {
                                results {
                                  __typename
                                  id
                                  name
                                }
                              }
                            }";

            var requestPayload = new
            {
                query,
                variables = new
                {
                    hubId,
                    folderId
                }
            };

            var contentJson = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync("https://developer.api.autodesk.com/graphql", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"GraphQL Error {response.StatusCode}:\n{responseContent}");

            using var doc = JsonDocument.Parse(responseContent);
            return doc.RootElement.Clone(); // Caller can process "data.itemsByFolder.results"
        }

        /// <summary>
        /// Retrieves all items across all hubs, projects, and folders accessible with the given token.
        /// </summary>
        /// <param name="token">OAuth access token.</param>
        /// <returns>List of <see cref="FolderItem"/> objects.</returns>
        public async Task<List<FolderItem>> GetAllFolderItemsAsync(string token)
        {
            var allItems = new List<FolderItem>();

            // 1. Get hubs
            var hubs = await GetHubsAsync(token);

            foreach (var hub in hubs)
            {
                var hubId = hub.id;

                // 2. Get projects for each hub
                var projects = await GetProjectsAsync(token, hubId);
                foreach (var project in projects)
                {
                    var projectId = project.id;

                    // 3. Get folders in each project
                    var folders = await GetFoldersByProjectAsync(token, projectId);
                    foreach (var folder in folders)
                    {
                        var folderId = folder.Id;

                        // 4. Get folder items for each folder
                        var resultJson = await GetFolderItemsAsync(token, hubId, folderId);

                        var itemsJson = resultJson.GetProperty("data")
                                                  .GetProperty("itemsByFolder")
                                                  .GetProperty("results");

                        foreach (var item in itemsJson.EnumerateArray())
                        {
                            allItems.Add(new FolderItem
                            {
                                TypeName = item.GetProperty("__typename").GetString(),
                                Name = item.GetProperty("name").GetString(),
                                HubId = hubId,
                                ProjectId = projectId,
                                FolderId = folderId
                            });
                        }
                    }
                }
            }

            return allItems;
        }
    }

    /// <summary>
    /// Represents a GraphQL request with query and variables.
    /// </summary>
    public class GraphQLRequest
    {
        /// <summary>
        /// Gets or sets the GraphQL query string.
        /// </summary>
        public string query { get; set; }
        /// <summary>
        /// Gets or sets the variables for the GraphQL query.
        /// </summary>
        public object variables { get; set; }
    }
}
