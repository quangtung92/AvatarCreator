using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Telerik.Windows.Controls;

namespace AvatarCreator
{
    public class ImageExampleHelper
    {
        public static Uri GetResourceUri(string resource)
        {
            AssemblyName assemblyName = new AssemblyName(typeof(ImageExampleHelper).Assembly.FullName);
            string resourcePath = "/" + assemblyName.Name + ";component/" + resource;
            Uri resourceUri = new Uri(resourcePath, UriKind.Relative);

            return resourceUri;
        }
    }
}