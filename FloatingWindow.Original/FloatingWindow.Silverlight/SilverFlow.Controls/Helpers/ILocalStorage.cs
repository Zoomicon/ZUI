namespace SilverFlow.Controls.Helpers
{
    /// <summary>
    /// This interface defines isolated storage methods and properties.
    /// </summary>
    public interface ILocalStorage
    {
        /// <summary>
        /// Gets or saves the value associated with the specified key.
        /// </summary>
        /// <value>The value associated with the specified key.</value>
        object this[string key] { get; set; }

        /// <summary>
        /// Determines if the local storage contains the specified key.
        /// </summary>
        /// <param name="key">The key for the entry to be located.</param>
        /// <returns>True if the local storage contains the specified key; otherwise, false.</returns>
        bool Contains(string key);
    }
}
