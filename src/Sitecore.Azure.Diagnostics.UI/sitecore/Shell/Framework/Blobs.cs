using Sitecore.Azure.Diagnostics.UI.Web;
using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Web;

namespace Sitecore.Azure.Diagnostics.UI.Shell.Framework
{
  /// <summary>
  /// Provides utility functions for working with blobs.
  /// </summary>
  public class Blobs
  {
    /// <summary>
    /// Lists the blobs in a modal dialog.
    /// </summary>
    /// <param name="header">The header text of the dialog.</param>
    /// <param name="description">The description text of the dialog.</param>
    /// <param name="icon">The icon of the dialog.</param>
    /// <param name="button">The button label text of the dialog.</param>
    /// <param name="filter">The blob search pattern.</param>
    public static void ListBlobs(string header, string description, string icon, string button, string filter)
    {
      Assert.ArgumentNotNull(header, "header");
      Assert.ArgumentNotNull(description, "description");
      Assert.ArgumentNotNull(icon, "icon");
      Assert.ArgumentNotNull(button, "button");
      Assert.ArgumentNotNull(filter, "filter");
      
      var urlString = new UrlString(UIUtil.GetUri("control:BlobLister"));
      var handle = new UrlHandle();
      
      handle["he"] = header;
      handle["txt"] = description;
      handle["ic"] = icon;
      handle["btn"] = button;
      handle["flt"] = filter;
      handle.Add(urlString);
      
      Context.ClientPage.ClientResponse.ShowModalDialog(urlString.ToString(), true);
    }

    /// <summary>
    /// Downloads the specified BLOB name.
    /// </summary>
    /// <param name="blobName">Name of the BLOB.</param>
    public static void Download(string blobName)
    {
      Assert.ArgumentNotNull(blobName, "parent");

      var urlString = new UrlString("/sitecore/shell/download.aspx");
      urlString.Add("blob", BlobHandle.GetBlobHandle(blobName));  

      Context.ClientPage.ClientResponse.SetLocation(urlString.ToString());
    }
  }
}