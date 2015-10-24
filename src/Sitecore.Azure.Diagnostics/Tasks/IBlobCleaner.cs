namespace Sitecore.Azure.Diagnostics.Tasks
{
  /// <summary>
  /// An interface required for BlobCleaner type.
  /// </summary>
  public interface IBlobCleaner
  {
    #region Properties

    /// <summary>
    /// Sets the container name..
    /// </summary>
    /// <value>
    /// The container name.
    /// </value>
    string ContainerName { get; }
    
    /// <summary>
    /// Sets the BLOB search pattern.
    /// </summary>
    /// <value>
    /// The BLOB search pattern.
    /// </value>
    string BlobSearchPattern { get; }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Execute cleanup operation
    /// </summary>
    void Execute();

    #endregion
  }
}