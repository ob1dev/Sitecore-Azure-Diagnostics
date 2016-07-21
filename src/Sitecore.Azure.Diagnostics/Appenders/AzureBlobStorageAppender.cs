using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net.Appender;
using log4net.spi;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Azure.Diagnostics.Storage;

namespace Sitecore.Azure.Diagnostics.Appenders
{
  /// <summary>
  /// Represents the appender that logs Sitecore diagnostic information to Microsoft Azure Blob storage service.
  /// </summary>
  public class AzureBlobStorageAppender : AppenderSkeleton
  {
    #region Fields

    /// <summary>
    /// Gets or sets the BLOB URI.
    /// </summary>
    /// <value>
    /// The BLOB URI.
    /// </value>
    public string BlobUri { get; set; }

    /// <summary>
    /// Gets or sets the current date.
    /// </summary>
    /// <value>
    /// The current date.
    /// </value>
    public DateTime CurrentDate { get; protected set; }

    /// <summary>
    /// The cloud BLOB for storing log entries.
    /// </summary>
    private ICloudBlob cloudBlob;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the BLOB.
    /// </summary>
    /// <value>
    /// The cloud BLOB.
    /// </value>
    private ICloudBlob Blob
    {
      get
      {
        // Create a new blob if this is the first time it is used.
        if (this.cloudBlob == null)
        {
          this.cloudBlob = this.GetNewBlob();
        }
        // Recreate a blob if a container is no longer exists.
        else if (!this.cloudBlob.Container.Exists())
        {
          this.cloudBlob = LogStorageManager.GetBlob(this.cloudBlob.Name);
        }
        // Create a new blob if the current shouldn't be used.
        else
        {
          DateTime now = DateTime.Now;
          bool needNewBlob = (this.CurrentDate.Day != now.Day || this.CurrentDate.Month != now.Month || this.CurrentDate.Year != now.Year);

          if (needNewBlob)
          {
            this.cloudBlob = this.GetNewBlob();
          }
        }

        return this.cloudBlob;
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageAppender"/> class.
    /// </summary>
    public AzureBlobStorageAppender()
    {
      this.BlobUri = "log.{date}.txt";
      this.CurrentDate = DateTime.Now; 
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Appends the specified logging event.
    /// </summary>
    /// <param name="loggingEvent">The logging event.</param>
    protected override void Append(LoggingEvent loggingEvent)
    {
      Sitecore.Diagnostics.Assert.ArgumentNotNull(loggingEvent, "loggingEvent");

      var blob = this.Blob as CloudAppendBlob;
      string message = this.RenderLoggingEvent(loggingEvent);

      CreateBlobIfNotExists(blob);
      blob.AppendText(message);
    }    

    /// <summary>
    /// Creates a new blob for the specified <see cref="CloudAppendBlob" /> if it does not already exist.
    /// </summary>
    /// <param name="blob">The <see cref="CloudAppendBlob" />.</param>
    /// <param name="accessCondition">The access condition.</param>
    /// <param name="blobRequestOptions">The blob request options.</param>
    /// <param name="operationContext">The operation context.</param>
    protected static void CreateBlobIfNotExists(CloudAppendBlob blob, AccessCondition accessCondition = null, BlobRequestOptions blobRequestOptions = null, OperationContext operationContext = null)
    {
        if (!blob.Exists(blobRequestOptions, operationContext))
        {
            blob.CreateOrReplace(accessCondition, blobRequestOptions, operationContext);
        }
    }

    /// <summary>
    /// Gets the new cloud blob for diagnostic messages.
    /// </summary>
    /// <returns>
    /// The cloud blob.
    /// </returns>
    protected virtual ICloudBlob GetNewBlob()
    {
      var blob = LogStorageManager.CreateBlob(this.ConstructBlobNameByDate());

      if (blob.Exists())
      {
        blob = LogStorageManager.CreateBlob(this.ConstructBlobNameByDateTime());
      }

      return blob;
    }

    /// <summary>
    /// Construct the blob name using the current date.
    /// </summary>
    /// <returns>
    /// The blob name based on the current date.
    /// </returns>
    protected virtual string ConstructBlobNameByDate()
    {
      return this.BlobUri.Replace("{date}", this.CurrentDate.ToString("yyyyMMdd"));
    }

    /// <summary>
    /// Constructs the blob name using the current date and time.
    /// </summary>
    /// <returns>
    /// The BLOB name based on the current date and time.
    /// </returns>
    protected virtual string ConstructBlobNameByDateTime()
    {
      string name = this.ConstructBlobNameByDate();
      int dotIndex = name.LastIndexOf('.');

      if (dotIndex < 0)
      {
        return name;
      }

      return name.Substring(0, dotIndex) + '.' + this.CurrentDate.ToString("HHmmss") + name.Substring(dotIndex);
    }
    
    #endregion
  }
}