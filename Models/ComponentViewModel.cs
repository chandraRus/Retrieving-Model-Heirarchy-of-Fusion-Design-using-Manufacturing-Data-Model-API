namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// ViewModel for displaying component hierarchy information.
    /// </summary>
    public class ComponentViewModel
    {
        /// <summary>
        /// Gets or sets the name of the root component.
        /// </summary>
        public string RootComponentName { get; set; }

        /// <summary>
        /// Gets or sets the list of component names in the hierarchy.
        /// </summary>
        public List<string> ComponentNames { get; set; }
    }
}
