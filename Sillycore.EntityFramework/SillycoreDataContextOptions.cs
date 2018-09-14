namespace Sillycore.EntityFramework
{
    public class SillycoreDataContextOptions
    {
        /// <summary>
        /// Adds Audit and SoftDelete event listeners
        /// </summary>
        public bool UseDefaultEventListeners { get; set; } = true;
    }
}