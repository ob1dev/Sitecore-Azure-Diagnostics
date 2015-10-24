using System;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sitecore.Azure.Diagnostics.Storage.RetryPolicies
{
  /// <summary>
  /// Represents a container being deleted retry policy that performs a specified number of retries, using a specified fixed time interval between retries.
  /// See details here: http://msdn.microsoft.com/en-us/library/azure/dd179408.aspx
  /// </summary>
  public class ContainerBeingDeletedRetryPolicy : IRetryPolicy
  {
    #region Fields

    /// <summary>
    /// The default client back off interval.
    /// </summary>
    private static readonly TimeSpan DefaultClientBackoff = TimeSpan.FromSeconds(5.0);

    /// <summary>
    /// The minimum back off interval.
    /// </summary>
    private static readonly TimeSpan MinBackoff = TimeSpan.FromSeconds(5.0);

    /// <summary>
    /// The maximum back off interval.
    /// </summary>
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(120.0);

    /// <summary>
    /// Gets or sets back off interval between retries.
    /// </summary>
    /// <value>
    /// The delta back off interval.
    /// </value>
    public TimeSpan DeltaBackoff { get; protected set; }

    /// <summary>
    /// Gets or sets the retry attempts.
    /// </summary>
    /// <value>
    /// The maximum the maximum number retry attempts.
    /// </value>
    public int MaxRetryAttempts { get; protected set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerBeingDeletedRetryPolicy"/> class.
    /// </summary>
    public ContainerBeingDeletedRetryPolicy()
      : this(DefaultClientBackoff, 5)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerBeingDeletedRetryPolicy"/> class.
    /// </summary>
    /// <param name="deltaBackoff">The back off interval between retries.</param>
    /// <param name="maxAttempts">The maximum number of retry attempts.</param>
    public ContainerBeingDeletedRetryPolicy(TimeSpan deltaBackoff, int maxAttempts)
    {
      this.DeltaBackoff = deltaBackoff;
      this.MaxRetryAttempts = maxAttempts;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Generates a new retry policy for the current request attempt.
    /// </summary>
    /// <returns>
    /// An <see cref="T:Microsoft.WindowsAzure.Storage.RetryPolicies.IRetryPolicy" /> object that represents the retry policy for the current request attempt.
    /// </returns>
    public IRetryPolicy CreateInstance()
    {
      return new ContainerBeingDeletedRetryPolicy();
    }

    /// <summary>
    /// Determines if the operation should be retried and how long to wait until the next retry.
    /// </summary>
    /// <param name="currentRetryCount">The number of retries for the given operation. A value of zero signifies this is the first error encountered.</param>
    /// <param name="statusCode">The status code for the last operation.</param>
    /// <param name="lastException">An <see cref="T:System.Exception" /> object that represents the last exception encountered.</param>
    /// <param name="retryInterval">The interval to wait until the next retry.</param>
    /// <param name="operationContext">An <see cref="T:Microsoft.WindowsAzure.Storage.OperationContext" /> object for tracking the current operation.</param>
    /// <returns>
    ///   <c>true</c> if the operation should be retried; otherwise, <c>false</c>.
    /// </returns>
    public bool ShouldRetry(int currentRetryCount, int statusCode, Exception lastException, out TimeSpan retryInterval,
      OperationContext operationContext)
    {
      //retryInterval = this.ContainerDeletingTime;

      retryInterval = TimeSpan.Zero;
      
      if (currentRetryCount >= this.MaxRetryAttempts)
      {
        return false;
      }

      // HTTP status code = Conflict (409)
      if ((HttpStatusCode) statusCode != HttpStatusCode.Conflict)
      {
        return false;
      }

      // We're only interested in storage exceptions so if there's any other exception, let's not retry it.
      if (lastException.GetType() != typeof(StorageException))
      {
        return false;
      }
      
      var storageException = (StorageException) lastException;
      string errorCode = storageException.RequestInformation.ExtendedErrorInformation.ErrorCode;

      // The specified container is being deleted. 
      if (errorCode.Equals("ContainerBeingDeleted"))
      {
        var random = new Random();
        double num = (Math.Pow(2.0, currentRetryCount) - 1.0) * random.Next((int)(this.DeltaBackoff.TotalMilliseconds * 0.8), (int)(this.DeltaBackoff.TotalMilliseconds * 1.2));
        retryInterval = (num < 0.0) ? MaxBackoff : TimeSpan.FromMilliseconds(Math.Min(MaxBackoff.TotalMilliseconds, MinBackoff.TotalMilliseconds + num));

        return true;
      }
      
      return false;
    }

    #endregion
  }
}