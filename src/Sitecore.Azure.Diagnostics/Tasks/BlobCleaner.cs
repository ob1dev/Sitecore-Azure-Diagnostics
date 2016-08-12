using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Azure.Diagnostics.Storage;
using Sitecore.Diagnostics;
using Sitecore.Xml;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;

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
      Assert.ArgumentNotNull(configNode, nameof(configNode));

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
        Log.Warn($"Scheduling.BlobsCleanupAgent: The 'maxAge' attribute equals to '{TimeSpan.Zero:dd\\.hh\\:mm\\:ss}'. All blobs will be deleted imitatively.", this);
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
        Log.Warn($"Scheduling.BlobsCleanupAgent: The '{container.Name}' cloud blob container has not been found.", this);
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
        Log.Info($"Scheduling.BlobsCleanupAgent: The '{container.Name}' cloud blob container does not have any out-to-date blobs that match the '{this.BlobSearchPattern}' search pattern.", this);
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
      Log.Info($"Scheduling.BlobsCleanupAgent: The '{container.Name}' cloud blob container includes '{blobList.Count()}' blobs that match the '{searchPattern}' search pattern.", this);

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
        Log.Info($"Scheduling.BlobsCleanupAgent: The '{container.Name}' cloud blob container includes '{candidateBlobList.Count()}' out-to-date blobs that match the '{searchPattern}' search pattern.", this);
      }
      
      return candidateBlobList;
    }

    /// <summary>
    /// Gets the BLOB age. The time since last modification.
    /// </summary>
    /// <param name="blob">The cloud BLOB.</param>
    /// <returns></returns>
    protected TimeSpan GetBlobAge(ICloudBlob blob)
    {
      Assert.ArgumentNotNull(blob, "blob");

      return DateTime.UtcNow - this.GetBlobLastModifiedDate(blob);
    }

    /// <summary>
    /// Gets the BLOB time (max of last modified)
    /// </summary>
    /// <param name="blob">The cloud BLOB.</param>
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

        Log.Info($"Scheduling.BlobsCleanupAgent: The '{blob.Name}' cloud blob is being deleted by cleanup task (Last Modified UTC Date: '{this.GetBlobLastModifiedDate(blob)}', Age: '{age:dd\\.hh\\:mm\\:ss}', Max allowed age: '{this.maxAge}'.", this);        
      }
    }

    #endregion
  }
}