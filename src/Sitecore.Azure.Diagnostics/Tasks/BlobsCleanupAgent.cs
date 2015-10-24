using System;
using System.Collections.Generic;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Tasks;

namespace Sitecore.Azure.Diagnostics.Tasks
{
  /// <summary>
  /// Represents the agent to clean up the blobs data.
  /// </summary>
  public class BlobsCleanupAgent : BaseAgent
  {
    #region Fields

    /// <summary>
    /// The blobs cleaners list.
    /// </summary>
    private readonly List<IBlobCleaner> blobsCleaners;

    #endregion 

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobsCleanupAgent"/> class.
    /// </summary>
    public BlobsCleanupAgent()
    {
      this.blobsCleaners = new List<IBlobCleaner>(); 
      this.LogActivity = true;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Runs the <see cref="BlobsCleanupAgent"/> instance.
    /// </summary>
    public virtual void Run()
    {
      this.LogInfo(string.Format("Scheduling.BlobsCleanupAgent: Started. The BlobCleaner count is '{0}'.", this.blobsCleaners.Count));

      foreach (IBlobCleaner cleaner in this.blobsCleaners)
      {
        try
        {
          cleaner.Execute();
        }
        catch (Exception exception)
        {
          Log.Error(string.Format("Scheduling.BlobsCleanupAgent: Exception occurred while cleaning the '{0}' cloud blob container.", cleaner.ContainerName), exception, this);
        }
      }

      this.LogInfo("Scheduling.BlobsCleanupAgent: Done.");
      
      TaskCounters.FileCleanups.Increment();
    }

    /// <summary>
    /// Adds the command.
    /// </summary>
    /// <param name="configNode">The configuration node.</param>
    public virtual void AddCommand(System.Xml.XmlNode configNode)
    {
      Assert.ArgumentNotNull(configNode, "configNode");

      var cleaner = new BlobCleaner(configNode);
      this.blobsCleaners.Add(cleaner);
    }

    #endregion
  }
}