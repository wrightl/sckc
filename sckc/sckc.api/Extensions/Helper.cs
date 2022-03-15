using System.IO;
using System.Web;

namespace sckc.api.Extensions
{
    public static class Helper
    {
		public static string MapPath(string path)
		{
			if (path.StartsWith("/"))
			{
				path = path.Substring(1);
			}
			string text = path;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			path = text.Replace("/", directorySeparatorChar.ToString());
			string physicalApplicationPath = HttpContext.Current.Request.PhysicalApplicationPath;
			return Path.Combine(physicalApplicationPath, path);
		}
	}
}