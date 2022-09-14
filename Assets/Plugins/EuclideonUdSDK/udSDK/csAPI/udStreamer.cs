using System;
using System.Runtime.InteropServices;

namespace udSDK
{
  /// <summary>
  /// Stores returned information from a streamer update
  /// </summary>
  public struct udStreamerInfo
  {
    public UInt32 active; //!< Not 0 if streamer has blocked to load, or models are awaiting destruction
    public Int64 memoryInUse; //!< Total (approximate) memory in use by the streamer (in bytes)
    public int modelsActive; //!< Number of models actively requesting data
    public int starvedTimeMsSinceLastUpdate; //!< Number of milliseconds spent waiting with no work to do since the previous update (ideally should be 0)
  };
  
  public static class udStreamer_f
  {
    /// <summary>
    /// Initialises the UDS streamer
    /// </summary>
    /// <param name="memoryThresholdBytes">Sets the threshold for how much memory the streaming system should *attempt* to stay below in bytes. Set as 0 to use the default amount of memory for the current platform.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udStreamer_Init")]
    public static extern udError Init(UInt64 memoryThresholdBytes); 
    
    /// <summary>
    /// Deinitialises the UDS streamer (reference counted). This must be called once for every call to udStreamer_Init regardless of return code of that function to decrease the reference count.
    /// </summary>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udStreamer_Deinit")]
    public static extern udError Deinit(); 
    
    /// <summary>
    /// Updates the UDS streamer manually (used in conjuction with udRCF_ManualStreamerUpdate)
    /// </summary>
    /// <param name="pStatus">A structure to write streaming information to; Use NULL if the information isn't required</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udStreamer_Update")]
    public static extern udError Update(udStreamerInfo pStatus); 
  }
}