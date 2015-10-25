using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Sitecore.Azure.Diagnostics.Storage.RetryPolicies;
using Sitecore.Diagnostics;

namespace Sitecore.Azure.Diagnostics.Storage
{
  /// <summary>
  /// Represent the provider to configure and execute requests against the Azure Blob Storage Service.
  /// </summary>
  public class AzureBlobStorageProvider : ProviderBase
  {
    #region Fields

    /// <summary>
    /// The default container name.
    /// </summary>
    public const string DefaultContainerName = "sitecore-logs";

    /// <summary>
    /// The application setting name that contains the connection string to Azure Storage.
    /// </summary>
    private const string AppSettingName = "Azure.Storage.ConnectionString.AppSettingName";

    /// <summary>
    /// The cloud BLOB client.
    /// </summary>
    private CloudBlobClient cloudBlobClient;

    /// <summary>
    /// The cloud blob container name
    /// </summary>
    public string CloudBlobContainerName { get; protected set; }
    
    /// <summary>
    /// The BLOB container public access type
    /// </summary>
    public BlobContainerPublicAccessType PublicAccessType { get; protected set; }

    /// <summary>
    /// The character encoding.
    /// </summary>
    public Encoding TextEncoding { get; protected set; }

    /// <summary>
    /// The cloud storage account.
    /// </summary>
    public CloudStorageAccount StorageAccount { get; protected set; }

    /// <summary>
    /// The cloud BLOB container.
    /// </summary>
    public CloudBlobContainer CloudBlobContainer { get; protected set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageProvider"/> class.
    /// </summary>
    public AzureBlobStorageProvider()
    {
      string appSetting = Configuration.Settings.GetSetting(AppSettingName);
      string storageConnectionString = CloudConfigurationManager.GetSetting(appSetting);

      // Retrieve storage account from connection string.
      this.StorageAccount = CloudStorageAccount.Parse(storageConnectionString);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Sets the name of the container.
    /// </summary>
    /// <value>
    /// The name of the container.
    /// </value>
    public string ContainerName
    {
      get
      {
        return this.CloudBlobContainerName;
      }

      protected set
      {
        if (!string.IsNullOrWhiteSpace(value))
        {
          this.CloudBlobContainerName = this.NormalizeContainerName(value);
          this.CloudBlobContainer = this.CloudBlobClient.GetContainerReference(this.CloudBlobContainerName);
        }
      }
    }

    /// <summary>
    /// Gets the cloud BLOB client.
    /// </summary>
    /// <value>
    /// The cloud BLOB client.
    /// </value>
    public CloudBlobClient CloudBlobClient
    {
      get
      {
        return this.cloudBlobClient ?? (this.cloudBlobClient = this.StorageAccount.CreateCloudBlobClient());
      }
    }

    /// <summary>
    /// Sets the BLOB container public access.
    /// </summary>
    /// <value>
    /// The BLOB container public access type.
    /// </value>
    protected string BlobContainerPublicAccessType
    {
      set
      {
        switch (value)
        {
          case "Blob":
            this.PublicAccessType = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType.Blob;
            break;

          case "Container":
            this.PublicAccessType = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType.Container;
            break;

          // The 'case "Off":' and 'default:' cases turn off the public access type.
          default:
            this.PublicAccessType = Microsoft.WindowsAzure.Storage.Blob.BlobContainerPublicAccessType.Off;
            break;
        }
      }
    }

    /// <summary>
    /// Sets the character encoding.
    /// </summary>
    /// <value>
    /// The encoding.
    /// </value>
    protected string Encoding
    {
      set
      {
        switch (value)
        {
          case "ASCII":
            this.TextEncoding = System.Text.Encoding.ASCII;
            break;

          case "BigEndianUnicode":
            this.TextEncoding = System.Text.Encoding.BigEndianUnicode;
            break;

          case "Default":
            this.TextEncoding = System.Text.Encoding.Default;
            break;

          case "Unicode":
            this.TextEncoding = System.Text.Encoding.Unicode;
            break;

          case "UTF32":
            this.TextEncoding = System.Text.Encoding.UTF32;
            break;

          case "UTF7":
            this.TextEncoding = System.Text.Encoding.UTF7;
            break;

          // The 'case "UTF8":' and 'default:' cases uses the UTF-8 format.
          default:
            this.TextEncoding = System.Text.Encoding.UTF8;
            break;
        }
      }
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the provider.
    /// </summary>
    /// <param name="name">The friendly name of the provider.</param>
    /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
    public override void Initialize(string name, NameValueCollection config)
    {
      Assert.ArgumentNotNullOrEmpty(name, "name");
      Assert.ArgumentNotNull(config, "config");

      base.Initialize(name, config);

      this.ContainerName = StringUtil.GetString((object)config["blobContainerName"], DefaultContainerName);
      this.BlobContainerPublicAccessType = StringUtil.GetString((object)config["blobContainerPublicAccess"]);
      this.Encoding = StringUtil.GetString((object)config["textEncoding"]);
    }

    /// <summary>
    /// Gets the cloud blob container.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <returns>The cloud BLOB container.</returns>
    public virtual CloudBlobContainer GetContainer(string containerName)
    {
      Assert.ArgumentNotNull(containerName, "containerName");

      var container = this.CloudBlobClient.GetContainerReference(containerName);

      var options = new BlobRequestOptions
      {
        RetryPolicy = new ContainerBeingDeletedRetryPolicy(),
      };
     
      container.CreateIfNotExists(this.PublicAccessType, options);
      
      return container;
    }

    /// <summary>
    /// Creates the cloud blob in the default container.
    /// </summary>
    /// <param name="blobName">Name of the blob.</param>
    /// <returns>The specified cloud blob.</returns>
    public virtual ICloudBlob CreateBlob(string blobName)
    {
      Assert.ArgumentNotNull(blobName, "blobName");

      string webRoleRelativeAddress = this.GetWebRoleRelativeAddress();

      // Build blob name for a Role Environment using the following format: {DeploymentId}/{RoleInstanceId}/{BlobName}.
      blobName = string.IsNullOrEmpty(webRoleRelativeAddress) ? blobName : string.Format("{0}/{1}", webRoleRelativeAddress, blobName);
      
      ICloudBlob blob = this.CloudBlobContainer.Exists() ? 
        this.CloudBlobContainer.GetBlockBlobReference(blobName) : 
        this.GetContainer(this.ContainerName).GetBlockBlobReference(blobName);

      return blob;
    }

    /// <summary>
    /// Gets the cloud blob in the default container.
    /// </summary>
    /// <param name="blobName">Name of the blob.</param>
    /// <returns>The specified cloud blob.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public virtual ICloudBlob GetBlob(string blobName)
    {
      Assert.ArgumentNotNull(blobName, "blobName");

      ICloudBlob blob = this.CloudBlobContainer.Exists() ? 
        this.CloudBlobContainer.GetBlobReferenceFromServer(blobName) : 
        this.GetContainer(this.ContainerName).GetBlobReferenceFromServer(blobName);

      return blob;
    }

    /// <summary>
    /// Gets the cloud blob from the specified container..
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <param name="blobName">Name of the BLOB.</param>
    /// <returns>The specified cloud blob.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public virtual ICloudBlob GetBlob(string containerName, string blobName)
    {
      Assert.ArgumentNotNull(containerName, "containerName");
      Assert.ArgumentNotNull(blobName, "blobName");

      return this.GetContainer(containerName).GetBlobReferenceFromServer(blobName);
    }

    /// <summary>
    /// Lists the cloud blobs from the default container.
    /// </summary>
    /// <param name="searchPattern">The search pattern of a BLOB name.</param>
    /// <returns>The collection of cloud blobs.</returns>
    public virtual ICollection<ICloudBlob> ListBlobs(string searchPattern)
    {
      Assert.ArgumentNotNull(searchPattern, "searchPattern");

      return this.ListBlobs(this.CloudBlobContainer, searchPattern);
    }

    /// <summary>
    /// Lists the cloud blobs from the specified container.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <param name="searchPattern">The search pattern of a BLOB name.</param>
    /// <returns>The collection of cloud blobs.</returns>
    public virtual ICollection<ICloudBlob> ListBlobs(string containerName, string searchPattern)
    {
      Assert.ArgumentNotNull(containerName, "containerName");
      Assert.ArgumentNotNull(searchPattern, "searchPattern");

      var container = this.GetContainer(containerName);
      return this.ListBlobs(container, searchPattern);
    }

    /// <summary>
    /// Lists the cloud blobs from the specified container.
    /// </summary>
    /// <param name="container">The cloud blob container.</param>
    /// <param name="searchPattern">The search pattern of a BLOB name.</param>
    /// <returns>The collection of cloud blobs.</returns>
    public virtual ICollection<ICloudBlob> ListBlobs(CloudBlobContainer container, string searchPattern)
    {
      Assert.ArgumentNotNull(container, "container");
      Assert.ArgumentNotNull(searchPattern, "searchPattern");

      string webRoleRelativeAddress = this.GetWebRoleRelativeAddress();
      var blobList = container.ListBlobs(webRoleRelativeAddress, true).Cast<ICloudBlob>().ToList();

      var filteredBlobList = new List<ICloudBlob>();

      if (blobList.Any())
      {
        const char maskSymbol = '*';
        string startPattern = searchPattern.Split(maskSymbol).First();
        string endPattern = searchPattern.Split(maskSymbol).Last();

        filteredBlobList = blobList.Where(w => w.Uri.Segments.Last().StartsWith(startPattern) && w.Uri.Segments.Last().EndsWith(endPattern)).ToList();
      }

      return filteredBlobList;
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Gets the deployment relative address.
    /// </summary>
    /// <returns>The deployment address that includes {DeploymentId}.</returns>
    protected virtual string GetDeploymentRelativeAddress()
    {
      return RoleEnvironment.IsAvailable ? RoleEnvironment.DeploymentId : string.Empty;
    }

    /// <summary>
    /// Gets the WebRole relative address.
    /// </summary>
    /// <returns>
    /// The WebRole address that includes {DeploymentId}/{RoleInstanceId}.
    /// </returns>
    protected virtual string GetWebRoleRelativeAddress()
    {
      string webRoleRelativeAddress = string.Empty;
      string deploymentRelativeAddress = this.GetDeploymentRelativeAddress();

      if (!string.IsNullOrEmpty(deploymentRelativeAddress))
      {
        webRoleRelativeAddress = string.Format("{0}/{1}", deploymentRelativeAddress, RoleEnvironment.CurrentRoleInstance.Id);
      }

      return webRoleRelativeAddress;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Normalizes the name of the container.
    /// </summary>
    /// <param name="sourceContainerName">The name.</param>
    /// <returns>
    /// Normalized name of the container.
    /// </returns>
    private string NormalizeContainerName(string sourceContainerName)
    {
      // Read the following article for more details on a container name conversion 
      // http://msdn.microsoft.com/en-us/library/dd135715.aspx

      // 1. Container names must start with a letter or number, and can contain only letters, numbers, and the dash (-) character.
      sourceContainerName = sourceContainerName.Replace(" ", "-");

      // 2. Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.
      sourceContainerName = sourceContainerName.Trim('-');

      // 3. All letters in a container name must be lowercase.
      sourceContainerName = sourceContainerName.ToLower();

      // 4. Container names must be from 3 through 63 characters long.
      const int minCharacterLong = 3;
      const int maxCharacterLong = 63;

      var lenght = sourceContainerName.Length;
      if (lenght < minCharacterLong)
      {
        sourceContainerName = this.CloudBlobContainerName;
      }
      else if (lenght > maxCharacterLong)
      {
        sourceContainerName = sourceContainerName.Substring(0, maxCharacterLong);
      }

      return sourceContainerName;
    }

    #endregion
  }
}