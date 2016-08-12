using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Tasks;
using System;
using System.Collections.Generic;

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
      this.LogInfo($"Scheduling.BlobsCleanupAgent: Started. The BlobCleaner count is '{this.blobsCleaners.Count}'.");

      foreach (IBlobCleaner cleaner in this.blobsCleaners)
      {
        try
        {
          cleaner.Execute();
        }
        catch (Exception exception)
        {
          Log.Error($"Scheduling.BlobsCleanupAgent: Exception occurred while cleaning the '{cleaner.ContainerName}' cloud blob container.", exception, this);
        }
      }

      this.LogInfo("Scheduling.BlobsCleanupAgent: Done.");
      
      JobsCount.TasksFileCleanups.Increment();
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