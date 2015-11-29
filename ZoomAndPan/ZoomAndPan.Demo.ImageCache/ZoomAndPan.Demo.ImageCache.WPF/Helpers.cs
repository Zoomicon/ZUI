using System;
using System.Reflection;
using System.Text;

namespace ZoomAndPanSample
{
    internal static class Helpers
    {
        private static string _AssemblyShortName;

        public static Uri MakePackUri(string relativeFile)
        {
            StringBuilder uriString = new StringBuilder(); ;
            uriString.Append("pack://application:,,,");
            uriString.Append("/" + AssemblyShortName + ";component/" + relativeFile);
            
            return new Uri(uriString.ToString(), UriKind.RelativeOrAbsolute);
        }

        private static string AssemblyShortName
        {
            get
            {
                if (_AssemblyShortName == null)
                {
                    Assembly a = typeof(Helpers).Assembly;

                    // Pull out the short name.
                    _AssemblyShortName = a.ToString().Split(',')[0];
                }

                return _AssemblyShortName;
            }
        }
    }
}