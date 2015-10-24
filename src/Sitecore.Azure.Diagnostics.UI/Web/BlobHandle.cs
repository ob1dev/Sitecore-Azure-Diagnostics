using System.Collections.Specialized;
using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Web;

namespace Sitecore.Azure.Diagnostics.UI.Web
{
  /// <summary>
  /// Represents the blob handle to safely transfer blobs paths between requests.
  /// </summary>
  public class BlobHandle
  {
    /// <summary>
    /// Gets the BLOB handle.
    /// </summary>
    /// <param name="blobName">Name of the BLOB.</param>
    /// <returns></returns>
    public static string GetBlobHandle(string blobName)
    {
      Assert.ArgumentNotNull(blobName, "blobName");

      var handle = new UrlHandle();
      handle["blob"] = blobName;
      handle.Add(new UrlString());

      return (handle.Handle ?? string.Empty);
    }
    
    /// <summary>
    /// Gets the name of the BLOB from the specified handle.
    /// </summary>
    /// <param name="blobHandle">The blob handle.</param>
    /// <returns></returns>
    public static string GetBlobName(string blobHandle)
    {
      Assert.ArgumentNotNull(blobHandle, "blobHandle");

      var parameters = new NameValueCollection
      {
        {"blob", blobHandle}
      };

      return UrlHandle.Get(new UrlString(parameters), "blob")["blob"];
    }
  }
}