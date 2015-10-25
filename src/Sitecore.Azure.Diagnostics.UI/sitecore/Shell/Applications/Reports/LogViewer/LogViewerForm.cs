using System;
using System.IO;
using System.Linq;
using System.Web;
using Sitecore.Azure.Diagnostics.Storage;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.Azure.Diagnostics.UI.Shell.Applications.Reports.LogViewer
{
  /// <summary>
  /// Represents a Log Viewer form.
  /// </summary>
  public class LogViewerForm : BaseForm
  {
    #region Fields

    /// <summary>
    /// The document.
    /// </summary>
    protected Frame Document;

    /// <summary>
    /// Has the file.
    /// </summary>
    protected Sitecore.Web.UI.HtmlControls.Action HasFile;

    #endregion

    #region base overrides

    /// <summary>
    /// Raises the load event.
    /// </summary>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected override void OnLoad([NotNull] EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");

      base.OnLoad(e);

      Assert.CanRunApplication("/sitecore/content/Applications/Control Panel/Reports/Log Viewer");
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Opens the specified args.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <contract>
    ///   <requires name="args" condition="not null" />
    /// </contract>
    [HandleMessage("logviewer:open", true)]
    public void Open([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      if (args.IsPostBack)
      {
        if (args.Result.Length > 0 && args.Result != "undefined")
        {
          if (string.IsNullOrEmpty(args.Result))
          {
            SheerResponse.Alert(Texts.YOU_CAN_ONLY_OPEN_LOG_FILES);
            return;
          }

          this.SetBlob(args.Result);
        }
      }
      else
      {
        Framework.Blobs.ListBlobs(Texts.OPEN_LOG_FILE, Texts.SELECT_A_LOG_FILE, "Software/32x32/text_code_colored.png", Texts.OPEN, "*log*.txt");
        args.WaitForPostBack();
      }
    }

    /// <summary>
    /// Deletes the specified args.
    /// </summary>
    /// <param name="args">
    /// The arguments.
    /// </param>
    /// <contract>
    ///   <requires name="args" condition="not null" />
    /// </contract>
    [HandleMessage("logviewer:delete", true)]
    public void Delete([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      var blobName = StringUtil.GetString(Context.ClientPage.ServerProperties["Blob"]);

      if (args.IsPostBack)
      {
        if (args.Result == "yes")
        {
          if (string.IsNullOrEmpty(blobName))
          {
            return;
          }

          try
          {
            var blob = LogStorageManager.GetBlob(blobName); 
            blob.DeleteAsync();

            Log.Audit(string.Format("Delete the '{0}' cloud blob.", blob.Name), this);
            this.SetBlob(string.Empty);
          }
          catch (Exception ex)
          {
            SheerResponse.Alert(Texts.THE_FILE_COULD_NOT_BE_DELETED_ERROR_MESSAGE + ex.Message);
          }
        }
      }
      else
      {
        if (Context.ClientPage.ServerProperties["Blob"] == null)
        {
          Context.ClientPage.ClientResponse.Alert(Texts.YOU_MUST_OPEN_A_LOG_FILE_FIRST);
          return;
        }

        var blob = LogStorageManager.GetBlob(blobName);
        blobName = blob.Uri.Segments.Last();
        SheerResponse.Confirm(Translate.Text(Texts.ARE_YOU_SURE_YOU_WANT_TO_DELETE_0, blobName));

        args.WaitForPostBack();
      }
    }

    /// <summary>
    /// Executes the  event.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <contract>
    ///   <requires name="message" condition="not null" />
    /// </contract>
    [HandleMessage("logviewer:download")]
    public void Download([NotNull] Message message)
    {
      Assert.ArgumentNotNull(message, "message");

      var blobName = StringUtil.GetString(Context.ClientPage.ServerProperties["Blob"]);
      if (blobName == null)
      {
        Context.ClientPage.ClientResponse.Alert(Texts.YOU_MUST_OPEN_A_LOG_FILE_FIRST);
        return;
      }
      
      if (string.IsNullOrEmpty(blobName))
      {
        return;
      }

      Framework.Blobs.Download(blobName);
    }

    /// <summary>
    /// Refreshes the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <contract>
    ///   <requires name="message" condition="not null" />
    /// </contract>
    [HandleMessage("logviewer:refresh")]
    public void Refresh([NotNull] Message message)
    {
      Assert.ArgumentNotNull(message, "message");

      this.SetBlob(StringUtil.GetString(Context.ClientPage.ServerProperties["Blob"]));
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Sets the file.
    /// </summary>
    /// <param name="blobName">The filename.</param>
    /// <contract>
    ///   <requires name="filename" condition="none" />
    /// </contract>
    private void SetBlob([CanBeNull] string blobName)
    {
      if (string.IsNullOrEmpty(blobName))
      {
        Context.ClientPage.ServerProperties["Blob"] = null;

        this.Document.SetSource("control:LogViewerDetails", string.Empty);

        Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarTitle", Translate.Text(Texts.LOG_FILES1));
        Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarDescription", Translate.Text(Texts.THIS_TOOL_DISPLAYS_THE_CONTENT_OF_LOG_FILES));

        this.HasFile.Disabled = true;

        return;
      }

      Context.ClientPage.ServerProperties["Blob"] = blobName;

      this.Document.SetSource("control:LogViewerDetails", "blob=" + HttpUtility.UrlEncode(blobName));

      var blob = LogStorageManager.GetBlob(blobName);

      Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarTitle", Path.GetFileNameWithoutExtension(blob.Name));
      var lastModified = blob.Properties.LastModified.HasValue ? blob.Properties.LastModified.Value.LocalDateTime : DateTime.Now;
      Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarDescription", Translate.Text(Translate.Text(Texts.LAST_ACCESS_0) + "<br/>", DateUtil.FormatShortDateTime(lastModified)) + Translate.Text(Texts.SIZE_0, MainUtil.FormatSize(blob.Properties.Length)));

      this.HasFile.Disabled = false;
    }

    #endregion
  }
}