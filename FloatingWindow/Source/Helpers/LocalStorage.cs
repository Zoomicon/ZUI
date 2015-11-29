using System.IO.IsolatedStorage;

namespace SilverFlow.Controls.Helpers
{
    /// <summary>
    /// Local isolated storage.
    /// </summary>
    public class LocalStorage : ILocalStorage
    {
        /// <summary>
        /// Gets or saves the value associated with the specified key.
        /// </summary>
        /// <value>The value associated with the specified key.</value>
        public object this[string key]
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings[key];
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings[key] = value;
            }
        }

        /// <summary>
        /// Determines if the local storage contains the specified key.
        /// </summary>
        /// <param name="key">The key for the entry to be located.</param>
        /// <returns>
        /// True if the local storage contains the specified key; otherwise, false.
        /// </returns>
        public bool Contains(string key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(key);
        }
    }
}
