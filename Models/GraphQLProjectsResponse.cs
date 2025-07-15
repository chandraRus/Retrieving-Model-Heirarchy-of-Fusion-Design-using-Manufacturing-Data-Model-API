namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents alternative identifiers for a project.
    /// </summary>
    public class AlternativeIdentifiers
    {
        /// <summary>
        /// Gets or sets the Data Management API project ID.
        /// </summary>
        public string dataManagementAPIProjectId { get; set; }
    }

    /// <summary>
    /// Represents a project result returned by the GraphQL query.
    /// </summary>
    public class ProjectResult
    {
        /// <summary>
        /// Gets or sets the project ID.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the project name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the type name of the project.
        /// </summary>
        public string __typename { get; set; }

        /// <summary>
        /// Gets or sets the alternative identifiers for the project.
        /// </summary>
        public AlternativeIdentifiers alternativeIdentifiers { get; set; }
    }

    /// <summary>
    /// Contains a list of project results.
    /// </summary>
    public class ProjectsData
    {
        /// <summary>
        /// Gets or sets the list of project results.
        /// </summary>
        public List<ProjectResult> results { get; set; }
    }

    /// <summary>
    /// Contains the projects data returned by the GraphQL query.
    /// </summary>
    public class ProjectsResponseData
    {
        /// <summary>
        /// Gets or sets the projects data.
        /// </summary>
        public ProjectsData projects { get; set; }
    }

    /// <summary>
    /// Represents the root response for a GraphQL projects query.
    /// </summary>
    public class GraphProjectsQLResponse
    {
        /// <summary>
        /// Gets or sets the data section of the response.
        /// </summary>
        public ProjectsResponseData data { get; set; }
    }
}