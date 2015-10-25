using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Azure.Diagnostics.Storage;
using Sitecore.Diagnostics;
using Sitecore.Xml;

namespace Sitecore.Azure.Diagnostics.Tasks
{
  /// <summary>
  /// Represents the blob cleaner.
  /// </summary>
  public class BlobCleaner : IBlobCleaner
  {
    #region Fields

    /// <summary>
    /// The BLOB name.
    /// </summary>
    public string BlobSearchPattern { get; private set; }

    /// <summary>
    /// The maximum age of BLOBs.
    /// </summary>
    private readonly TimeSpan maxAge;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobCleaner" /> class.
    /// </summary>
    /// <param name="configNode">The config node.</param>
    public BlobCleaner(XmlNode configNode)
    {
      Assert.ArgumentNotNull(configNode, "configNode");

      NameValueCollection configSettings = XmlUtil.GetAttributes(configNode);

      this.BlobSearchPattern = StringUtil.GetString((object)configSettings["blobSearchPattern"]);
      if (this.BlobSearchPattern.Equals(String.Empty))
      {
        Log.Warn("Scheduling.BlobsCleanupAgent: The 'blobSearchPattern' attribute is not specified. All blobs will be searchable.", this);
        this.BlobSearchPattern = "*";
      }

      this.maxAge = DateUtil.ParseTimeSpan(configSettings["maxAge"], TimeSpan.FromDays(7));
      if (this.maxAge == TimeSpan.Zero)
      {
        Log.Warn(string.Format("Scheduling.BlobsCleanupAgent: The 'maxAge' attribute equals to '{0:dd\\.hh\\:mm\\:ss}'. All blobs will be deleted imitatively.", TimeSpan.Zero), this);
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// The container name.
    /// </summary>
    public string ContainerName
    {
      get
      {
        return LogStorageManager.DefaultContainer.Name;  
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Execute cleanup operation
    /// </summary>
    public virtual void Execute()
    {
      var container = LogStorageManager.DefaultContainer;

      if (container.Exists())
      {
        this.Cleanup(container);
      }
      else
      {
        Log.Warn(string.Format("Scheduling.BlobsCleanupAgent: The '{0}' cloud blob container has not been found.", container.Name), this);
      }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Execute simple cleanup operation against the BLOB container.
    /// </summary>
    /// <param name="container">The cloud blob container.</param>
    protected void Cleanup(CloudBlobContainer container)
    {
      var candidateBlobs = this.GetCandidateBlobs(container, this.BlobSearchPattern);
      if (candidateBlobs.Any())
      {
        this.DeleteBlobs(candidateBlobs);
      }
      else
      {
        Log.Info(string.Format("Scheduling.BlobsCleanupAgent: The '{0}' cloud blob container does not have any out-to-date blobs that match the '{1}' search pattern.", container.Name, this.BlobSearchPattern), this);
      }
    }

    /// <summary>
    /// Gets the candidate blobs for cleaning up.
    /// </summary>
    /// <param name="container">The cloud blob container.</param>
    /// <param name="searchPattern">The search pattern of a BLOB name.</param>
    /// <returns></returns>
    protected ICollection<ICloudBlob> GetCandidateBlobs(CloudBlobContainer container, string searchPattern)
    {
      Assert.ArgumentNotNull(container, "container");
      Assert.ArgumentNotNull(searchPattern, "searchPattern");

      var blobList = LogStorageManager.ListBlobs(container, searchPattern);
      Log.Info(string.Format("Scheduling.BlobsCleanupAgent: The '{0}' cloud blob container includes '{1}' blobs that match the '{2}' search pattern.", container.Name, blobList.Count(), searchPattern), this);

      var candidateBlobList = new List<ICloudBlob>();

      foreach (ICloudBlob blob in blobList)
      {
        if (blob != null)
        {
          DateTime utcMaxTime = this.GetBlobLastModifiedDate(blob).Add(this.maxAge);

          if (DateTime.UtcNow > utcMaxTime)
          {
            candidateBlobList.Add(blob);
          }
        }
      }

      if (candidateBlobList.Any())
      {
        Log.Info(string.Format("Scheduling.BlobsCleanupAgent: The '{0}' cloud blob container includes '{1}' out-to-date blobs that match the '{2}' search pattern.", container.Name, candidateBlobList.Count(), searchPattern), this);
      }
      
      return candidateBlobList;
    }

    /// <summary>
    /// Gets the BLOB age. The time since last modification.
    /// </summary>
    /// <param name="blob">The cloud block BLOB.</param>
    /// <returns></returns>
    protected TimeSpan GetBlobAge(ICloudBlob blob)
    {
      Assert.ArgumentNotNull(blob, "blob");

      return DateTime.UtcNow - this.GetBlobLastModifiedDate(blob);
    }

    /// <summary>
    /// Gets the BLOB time (max of last modified)
    /// </summary>
    /// <param name="blob">The cloud block BLOB.</param>
    /// <returns></returns>
    protected DateTime GetBlobLastModifiedDate(ICloudBlob blob)
    {
      Assert.ArgumentNotNull(blob, "blob");

      return blob.Properties.LastModified.HasValue ? blob.Properties.LastModified.Value.UtcDateTime : DateTime.UtcNow;
    }

    /// <summary>
    /// Deletes the blobs.
    /// </summary>
    /// <param name="blobsList">The blobs list.</param>
    protected void DeleteBlobs(IEnumerable<ICloudBlob> blobsList)
    {
      Assert.ArgumentNotNull(blobsList, "blobsList");

      foreach (ICloudBlob blob in blobsList)
      {
        blob.DeleteAsync();

        TimeSpan age = this.GetBlobAge(blob);

        Log.Info(string.Format("Scheduling.BlobsCleanupAgent: The '{0}' cloud blob is being deleted by cleanup task (Last Modified UTC Date: '{1}', Age: '{2:dd\\.hh\\:mm\\:ss}', Max allowed age: '{3}'.", 
         blob.Name, this.GetBlobLastModifiedDate(blob), age, this.maxAge), this);
      }
    }

    #endregion
  }
}