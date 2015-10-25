using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Configuration;

namespace Sitecore.Azure.Diagnostics.Storage
{
  /// <summary>
  /// Represents the storage log manger to keep diagnostic information.
  /// </summary>
  public class LogStorageManager
  {
    #region Fields

    /// <summary>
    /// The provider helper for the LogStorageManager.
    /// </summary>
    private static readonly ProviderHelper<AzureBlobStorageProvider, AzureBlobStorageProviderCollection> Helper;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes the <see cref="LogStorageManager"/> class.
    /// </summary>
    static LogStorageManager()
    {
      Helper = new ProviderHelper<AzureBlobStorageProvider, AzureBlobStorageProviderCollection>("logStorageManager");
    }

    #endregion

    #region Static Properties

    /// <summary>
    /// Gets the provider.
    /// </summary>
    /// <value>
    /// The provider.
    /// </value>
    public static AzureBlobStorageProvider Provider
    {
      get
      {
        return Helper.Provider;
      }
    }

    /// <summary>
    /// Gets the collection of the storage providers.
    /// </summary>
    /// <value>
    /// The defined providers.
    /// </value>
    public static AzureBlobStorageProviderCollection Providers
    {
      get
      {
        return Helper.Providers;
      }
    }

    /// <summary>
    /// Gets the default container.
    /// </summary>
    /// <value>
    /// The default container.
    /// </value>
    public static CloudBlobContainer DefaultContainer
    {
      get
      {
        return Provider.CloudBlobContainer;
      }
    }

    /// <summary>
    /// Gets the default BLOB container public access type.
    /// </summary>
    /// <value>
    /// The BLOB container public access.
    /// </value>
    public static BlobContainerPublicAccessType DefaultPublicAccessType
    {
      get
      {
        return Provider.PublicAccessType;
      }
    }

    /// <summary>
    /// The character encoding for blob files.
    /// </summary>
    public static Encoding DefaultTextEncoding
    {
      get
      {
        return Provider.TextEncoding;
      }
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Gets the cloud blob container.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <returns>The cloud BLOB container.</returns>
    public static CloudBlobContainer GetContainer(string containerName)
    {
      return Provider.GetContainer(containerName);
    }

    /// <summary>
    /// Creates the cloud blob in the default container.
    /// </summary>
    /// <param name="blobName">Name of the blob.</param>
    /// <returns>The specified cloud blob.</returns>
    public static ICloudBlob CreateBlob(string blobName)
    {
      return Provider.CreateBlob(blobName);
    }

    /// <summary>
    /// Gets the cloud blob from the default container.
    /// </summary>
    /// <param name="blobName">Name of the BLOB.</param>
    /// <returns>The specified cloud blob.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public static ICloudBlob GetBlob(string blobName)
    {
      return Provider.GetBlob(blobName);
    }

    /// <summary>
    /// Gets the cloud blob from the specified container..
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <param name="blobName">Name of the BLOB.</param>
    /// <returns>The specified cloud blob.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public static ICloudBlob GetBlob(string containerName, string blobName)
    {
      return Provider.GetBlob(containerName, blobName);
    }

    /// <summary>
    /// Lists the cloud blobs from the default container.
    /// </summary>
    /// <param name="searchPattern">The search pattern of a BLOB name.</param>
    /// <returns>The collection of cloud blobs.</returns>
    public static ICollection<ICloudBlob> ListBlobs(string searchPattern)
    {
      return Provider.ListBlobs(searchPattern);
    }

    /// <summary>
    /// Lists the cloud blobs from the specified container.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <param name="searchPattern">The search pattern of a BLOB name.</param>
    /// <returns>The collection of cloud blobs.</returns>
    public virtual ICollection<ICloudBlob> ListBlobs(string containerName, string searchPattern)
    {
      return Provider.ListBlobs(containerName, searchPattern);
    }

    /// <summary>
    /// Lists the cloud blobs from the specified container.
    /// </summary>
    /// <param name="container">The cloud blob container.</param>
    /// <param name="searchPattern">The search pattern of a BLOB name.</param>
    /// <returns>The collection of cloud blobs.</returns>
    public static ICollection<ICloudBlob> ListBlobs(CloudBlobContainer container, string searchPattern)
    {
      return Provider.ListBlobs(container, searchPattern);
    }

    #endregion
  }
}