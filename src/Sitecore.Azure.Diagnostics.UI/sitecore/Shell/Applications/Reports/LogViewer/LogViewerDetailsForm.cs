using System;
using System.Web;
using System.Web.UI;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Azure.Diagnostics.Storage;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Azure.Diagnostics.UI.Shell.Applications.Reports.LogViewer
{
  /// <summary>
  /// Represents a LogViewerDetailsForm.
  /// </summary>
  public class LogViewerDetailsForm : BaseForm
  {
    /// <summary>
    /// The log viewer.
    /// </summary>
    protected Scrollbox LogViewer;

    /// <summary>
    /// The text panel.
    /// </summary>
    protected Border TextPanel;
    
    /// <summary>
    /// Raises the load event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
    /// <contract>
    ///   <requires name="e" condition="not null" />
    /// </contract>
    protected override void OnLoad(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      Assert.CanRunApplication("/sitecore/content/Applications/Control Panel/Reports/Log Viewer");

      base.OnLoad(e);

      if (Context.ClientPage.IsEvent)
      {
        return;
      }

      string blobName = WebUtil.GetQueryString("blob");
      if (string.IsNullOrEmpty(blobName))
      {
        return;
      }

      this.TextPanel.Visible = false;

      var blob = LogStorageManager.GetBlob(blobName);
      var data = string.Empty;

      if (blob.BlobType == BlobType.AppendBlob)
      {
        data = ((CloudAppendBlob)blob).DownloadText(LogStorageManager.DefaultTextEncoding);
      }
      
      if (string.IsNullOrEmpty(data))
      {
        this.LogViewer.Controls.Add(new LiteralControl(Translate.Text(Texts.THIS_FILE_IS_EMPTY_OR_CANNOT_BE_OPENED_FOR_READING)));
        return;
      }

      data = HttpUtility.HtmlEncode(data).Replace("\n", "<br/>");
      this.LogViewer.Controls.Add(new LiteralControl(data));
    }
  }
}