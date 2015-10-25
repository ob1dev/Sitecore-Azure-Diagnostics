using System.Configuration.Provider;

namespace Sitecore.Azure.Diagnostics.Storage
{
  /// <summary>
  /// Represents a collection of providers that inherit from ProviderBase.
  /// </summary>
  public class AzureBlobStorageProviderCollection : ProviderCollection
  {
    /// <summary>
    /// Gets the provider with the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public new AzureBlobStorageProvider this[string name]
    {
      get
      {
        return (base[name] as AzureBlobStorageProvider);
      }
    }
  }
}