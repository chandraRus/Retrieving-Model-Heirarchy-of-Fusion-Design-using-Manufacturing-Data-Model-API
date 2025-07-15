namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// ViewModel representing the component hierarchy and related data for display in the UI.
    /// </summary>
    public class ComponentViewModel
    {
        /// <summary>
        /// Gets or sets the name of the root component.
        /// </summary>
        public string RootComponentName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the root component version.
        /// </summary>
        public string ComponentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy text (typically serialized for display or processing).
        /// </summary>
        public string HierarchyText { get; set; }

        /// <summary>
        /// Gets or sets the list of folder items associated with the component.
        /// </summary>
        public List<FolderItem> FolderItems { get; set; } = new();
    }

    /// <summary>
    /// Represents a folder item in the Autodesk data model.
    /// </summary>
    public class FolderItem
    {
        /// <summary>
        /// Gets or sets the type name of the item (e.g., "folders").
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the folder item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the hub ID associated with the folder item.
        /// </summary>
        public string HubId { get; set; }

        /// <summary>
        /// Gets or sets the project ID associated with the folder item.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the folder ID.
        /// </summary>
        public string FolderId { get; set; }
    }
}
