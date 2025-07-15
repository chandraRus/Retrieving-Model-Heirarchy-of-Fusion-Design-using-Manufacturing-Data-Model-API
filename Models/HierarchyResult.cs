namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents the result of a hierarchy query, including the component version ID and the root node of the hierarchy.
    /// </summary>
    public class HierarchyResult
    {
        /// <summary>
        /// Gets or sets the ID of the component version.
        /// </summary>
        public string ComponentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the root node of the hierarchy.
        /// </summary>
        public HierarchyNode Root { get; set; }
    }
}
