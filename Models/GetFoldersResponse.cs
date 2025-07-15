using System.Text.Json.Serialization;

namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents the response from a GraphQL query for folders by project.
    /// </summary>
    public class GetFoldersResponse
    {
        /// <summary>
        /// Gets or sets the data section of the response.
        /// </summary>
        public FoldersData Data { get; set; }
    }

    /// <summary>
    /// Contains the folders data returned by the GraphQL query.
    /// </summary>
    public class FoldersData
    {
        /// <summary>
        /// Gets or sets the folders by project information.
        /// </summary>
        [JsonPropertyName("foldersByProject")]
        public FoldersByProject FoldersByProject { get; set; }
    }

    /// <summary>
    /// Represents the folders by project, including pagination and results.
    /// </summary>
    public class FoldersByProject
    {
        /// <summary>
        /// Gets or sets the pagination information.
        /// </summary>
        public Pagination Pagination { get; set; }

        /// <summary>
        /// Gets or sets the list of folder results.
        /// </summary>
        public List<FolderResult> Results { get; set; }
    }

    /// <summary>
    /// Represents pagination information for folder queries.
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// Gets or sets the pagination cursor.
        /// </summary>
        public string Cursor { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Represents a folder result returned by the query.
    /// </summary>
    public class FolderResult
    {
        /// <summary>
        /// Gets or sets the folder ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the folder name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object count in the folder.
        /// </summary>
        public int ObjectCount { get; set; }

        /// <summary>
        /// Gets or sets the type name (e.g., "folders").
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the hub ID associated with the folder.
        /// </summary>
        public string HubId { get; set; }

        /// <summary>
        /// Gets or sets the project ID associated with the folder.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the folder ID (may duplicate Id).
        /// </summary>
        public string FolderId { get; set; }
    }
}
