using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Azure.Diagnostics.Storage;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System;
using System.Web;
using System.Web.UI;

namespace Sitecore.Azure.Diagnostics.UI.Shell.Applications.Reports.LogViewer
{
  /// <summary>
  /// Represents a LogViewerDetailsForm.
  /// </summary>
  public class LogViewerDetailsForm : BaseForm
  {
    #region Protected fields

    /// <summary>
    /// The log viewer.
    /// </summary>
    protected Scrollbox LogViewer;

    /// <summary>
    /// The text panel.
    /// </summary>
    protected Border TextPanel;

    #endregion Protected fields

    #region Protected methods

    /// <summary>
    /// Raises the load event.
    /// </summary>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    /// <contract>
    ///   <requires name="e" condition="not null"/>
    ///   </contract>
    protected override void OnLoad([NotNull] EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      Assert.CanRunApplication("/sitecore/content/Applications/Tools/Log Viewer");

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
     
      var blob = LogStorageManager.GetBlob(blobName);
      var data = string.Empty;

      if (blob.BlobType == BlobType.AppendBlob)
      {
        this.TextPanel.Visible = false;
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

    #endregion
  }
}