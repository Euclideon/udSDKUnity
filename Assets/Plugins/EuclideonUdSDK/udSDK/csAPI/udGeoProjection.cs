using System;
using System.Runtime.InteropServices;

namespace udSDK
{
  public class udGeoProjection
  {
    /// <summary>
    /// Loads a set of zones from a JSON file where each member is defined as "AUTHORITY:SRID" (eg. "EPSG:32756")
    /// </summary>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeoProjection_LoadZonesFromJSON")]
    public static extern udError LoadZonesFromJSON(string pJSONStr, ref int pLoaded, ref int pFailed);

    /// <summary>
    /// Unloads all loaded zones (only needs to be called once to unload all previously loaded zones from udGeoProjection_LoadZonesFromJSON)
    /// </summary>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udGeoProjection_UnloadZones")]
    public static extern udError udGeoProjection_UnloadZones(); 
  }
}