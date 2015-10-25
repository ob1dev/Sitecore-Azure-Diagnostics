using System;
using System.Linq;
using Sitecore.Azure.Diagnostics.Storage;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Shell.Web;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;

namespace Sitecore.Azure.Diagnostics.UI.Shell.Applications.Blobs.BlobLister
{
  /// <summary>
  /// Represents a File Lister form.
  /// </summary>
  public class BlobListerForm : DialogForm
  {
    #region Fields

    /// <summary>
    /// The dialog.
    /// </summary>
    protected XmlControl Dialog;

    /// <summary>
    /// The file lister.
    /// </summary>
    protected Listview FileLister;
    
    #endregion

    #region Protected methods

    /// <summary>
    /// Raises the load event.
    /// </summary>
    /// <param name="e">
    /// The <see cref="System.EventArgs"/> instance containing the event data.
    /// </param>
    /// <remarks>
    /// This method notifies the server control that it should perform actions common to each HTTP
    /// request for the page it is associated with, such as setting up a database query. At this
    /// stage in the page lifecycle, server controls in the hierarchy are created and initialized,
    /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
    /// property to determine whether the page is being loaded in response to a client postback,
    /// or if it is being loaded and accessed for the first time.
    /// </remarks>
    protected override void OnLoad([NotNull] EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");

      this.CheckSecurity();

      base.OnLoad(e);

      if (Context.ClientPage.IsEvent)
      {
        return;
      }

      var urlHandle = UrlHandle.Get();

      var icon = urlHandle["ic"];
      if (!string.IsNullOrEmpty(icon))
      {
        this.Dialog["Icon"] = icon;
      }

      var header = WebUtil.SafeEncode(urlHandle["he"]);
      if (header.Length > 0)
      {
        this.Dialog["Header"] = header;
      }

      var text = WebUtil.SafeEncode(urlHandle["txt"]);
      if (text.Length > 0)
      {
        this.Dialog["Text"] = text;
      }

      var button = WebUtil.SafeEncode(urlHandle["btn"]);
      if (button.Length > 0)
      {
        this.Dialog["OKButton"] = button;
      }

      var filter = urlHandle["flt"];
      var blobsList = LogStorageManager.ListBlobs(filter);

      foreach (var blob in blobsList)
      {
        var item = new ListviewItem();
        this.FileLister.Controls.Add(item);

        item.ID = Control.GetUniqueID("I");
        item.Header = blob.Uri.Segments.Last();
        item.Icon = "Applications/16x16/document.png";
        item.ServerProperties["Blob"] = blob.Name;
        item.ColumnValues["size"] = MainUtil.FormatSize(blob.Properties.Length);
        item.ColumnValues["modified"] = blob.Properties.LastModified.HasValue ? blob.Properties.LastModified.Value.LocalDateTime : DateTime.Now;
      }
    }

    /// <summary>
    /// Checks the security.
    /// </summary>
    /// <exception cref="AccessDeniedException">
    /// Application access denied.
    /// </exception>
    protected virtual void CheckSecurity()
    {
      ShellPage.IsLoggedIn();

      var user = Context.User;

      if (user.IsAdministrator)
      {
        return;
      }

      var isDeveloping = user.IsInRole("sitecore\\Sitecore Client Developing");
      var isMaintaining = user.IsInRole("sitecore\\Sitecore Client Maintaining");
      var isAccessDenied = !isDeveloping && !isMaintaining;

      if (isAccessDenied)
      {
        throw new AccessDeniedException("Application access denied.");
      }
    }

    /// <summary>
    /// Executes the OK event.
    /// </summary>
    protected void DoOk()
    {
      var selected = this.FileLister.SelectedItems;

      if (selected.Length == 0)
      {
        SheerResponse.Alert(Texts.PLEASE_SELECT_A_FILE);
        return;
      }

      if (selected.Length > 1)
      {
        SheerResponse.Alert(Texts.PLEASE_SELECT_A_SINGLE_FILE);
        return;
      }

      var result = selected[0].ServerProperties["Blob"] as string;
      SheerResponse.SetDialogValue(result);
      SheerResponse.CloseWindow();
    }

    /// <summary>
    /// Called when the file lister DBL has click.
    /// </summary>
    protected void OnFileListerDblClick()
    {
      this.DoOk();
    }

    /// <summary>
    /// Handles a click on the OK button.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="args">
    /// The arguments.
    /// </param>
    /// <remarks>
    /// When the user clicks OK, the dialog is closed by calling
    /// the <see cref="Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.
    /// </remarks>
    protected override void OnOK([NotNull] object sender, [NotNull] EventArgs args)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(args, "args");

      this.DoOk();
    }
    
    #endregion Protected methods
  }
}