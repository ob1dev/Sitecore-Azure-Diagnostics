using System;
using System.Globalization;
using System.IO;
using System.Web;
using Sitecore.Azure.Diagnostics.Storage;
using Sitecore.Azure.Diagnostics.UI.Web;
using Sitecore.Diagnostics;
using Sitecore.Web;

namespace Sitecore.Azure.Diagnostics.UI.Shell
{
  /// <summary>
  /// Represents the Download Page.
  /// </summary>
  public class DownloadPage : Sitecore.Shell.DownloadPage
  {
    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
    /// <contract>
    ///   <requires name="e" condition="not null" />
    /// </contract>
    protected override void OnLoad([NotNull] EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");

      base.OnLoad(e);

      string blobHandle = WebUtil.GetQueryString("blob");

      if (!string.IsNullOrEmpty(blobHandle))
      {
        var blobName = BlobHandle.GetBlobName(blobHandle);
      
        if (!string.IsNullOrEmpty(blobName))
        {
          var blob = LogStorageManager.GetBlob(blobName);
          var response = HttpContext.Current.Response;

          using (var stream = new MemoryStream())
          {
            blob.DownloadToStream(stream);

            response.ClearHeaders();
            response.ContentType = blob.Properties.ContentType;

            response.AddHeader("Content-Disposition", "attachment; filename=\"" + blob.Name + "\"");
            response.AddHeader("Content-Length", blob.Properties.Length.ToString(CultureInfo.InvariantCulture));

            response.AddHeader("Content-Transfer-Encoding", "binary");
            response.CacheControl = "private";

            response.BinaryWrite(stream.ToArray());
            response.End();
          }
        }
      }
    }
  }
}