namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents the response from a GraphQL query for hubs.
    /// </summary>
    public class GraphQLResponse
    {
        /// <summary>
        /// Gets or sets the data section of the response.
        /// </summary>
        public HubData? Data { get; set; }
    }

    /// <summary>
    /// Contains the hub data returned by the GraphQL query.
    /// </summary>
    public class HubData
    {
        /// <summary>
        /// Gets or sets the container for hubs.
        /// </summary>
        public HubsContainer? Hubs { get; set; }
    }

    /// <summary>
    /// Represents a container for a list of hub results.
    /// </summary>
    public class HubsContainer
    {
        /// <summary>
        /// Gets or sets the list of hub results.
        /// </summary>
        public List<HubResult>? Results { get; set; }
    }

    /// <summary>
    /// Represents a hub result.
    /// </summary>
    public class HubResult
    {
        /// <summary>
        /// Gets or sets the name of the hub.
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// Gets or sets the ID of the hub.
        /// </summary>
        public string? id { get; set; }
    }
}
