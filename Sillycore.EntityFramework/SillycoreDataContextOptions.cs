namespace Sillycore.EntityFramework
{
    public class SillycoreDataContextOptions
    {
        public bool SetUpdatedOnSameAsCreatedOnForNewObjects { get; set; }

        /// <summary>
        /// Adds Audit and SoftDelete event listeners
        /// </summary>
        public bool UseDefaultEventListeners { get; set; } = true;
    }
}