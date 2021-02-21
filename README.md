# udSDK- Unity Plugin

This project demonstrates the use of Euclideon udSDK developer Kit (udSDK SDK) with the Unity Real Time Development Platform. 

```
Language:              C#
Type:                  Integration
Contributor:           Euclideon udSDK Development Team <support@euclideon.com>
Organization:          Euclideon, https://euclideon.com/
Date:                  2020-08-10
udSDK Version:         1.0.0
Toolsets:              Requires Unity >= 2019.3.4f1
```

## Quickstart guide 

**The Unity-udSDK sample requires a free Euclideon Account to use, licenses can be obtained [here](https://www.euclideon.com/udsdk/)** 
The udSDK is tested with Unity 2019.3.4f1 - it may work in other versions of Unity, but we can't guarantee that it does. Please sneure you have Unity 2019.3.4f1 installed.

### Installation - New or Existing Project
The fastest way to install udSDK for Unity is to go [here](https://Euclideon.com/unity) and follow the onscreen instructions.

### Installation - Github Samples
1. Download and extract the latest udSDK package from [here](https://udstream.euclideon.com) using your license credentials. You can obtain a free account [from our website](https://www.euclideon.com/free-development-resources/) 
2. Clone or Download the Unity udSDK examples from [here](https://github.com/Euclideon/udSDKUnity)
3. Copy the files from _udSDK_2.0\lib\(your operating system here)\ _ to your Unity project working directory
3. Open the udSDK Unity example project by navigating to _the ‘udSDKUnity' directory and opening Basic Render from udSDKUnity/Assets/Plugins/EuclideonUdSDK/Sample Scenes 
5. Open Unity Hub, select add and then the location of the downlaoded project
4. From the Toolbar in the Unity Editor, Navigate to UD > Set User Info - and enter your udgc username/password, then press Save User Info.

udSDK with Unity is now ready to go! Press play!

### Changing UDS model

The UDS can be changed by modifying the path attribute of the Model object in the project hierarchy (by default on the left of the screen) using the inspector (by default on the right)

There is a UDS model included with this example for demonstration purposes, paths to your own model can be pasted into the _path_ field of the US model object
UDS file format developed by Euclideon allowing streamable, unlimited sized 3D datasets requiring only low spec hardware. 

Models can be created from most common photogrammetry and LiDAR point clouds file formats using Euclideon [udStream](https://www.euclideon.com/vault/) available [Here](https://www.euclideon.com/udstream-free/) your vault SDK trial license also gives you access to vault client during your trial period.
You can read about the conversion process [here](https://www.euclideon.com/wp-content/uploads/2019/10/2019_10_31-udSDK-Conversion-Guide-v1.2.pdf) if you have any questions check the [support knowledge base](https://desk.euclideon.com) or on the 

Photogrammetry model of the Gold Coast courtesy of [Aerometrex](https://aerometrex.com.au/)

# Sample Scenes
Each included example is accompanied by a scene demonstrating the use of the objects. The best way to become farmiliar with these unity objects is to explore their usage in those scenes.
Currently there are four sample scenes
- Basic Render - showing importing a UDS and usage of the picking system (extraction of voxel data from the point cloud given a coordinate in screen space)
- Driving demo -Showing the usage of the udSDK collider object to make local mesh colliders for physics simulation
- Filter Demo -  demonstrating the use of a query filter to selectively render volumes of the point cloud
- Raycasting Example - Showing usage of the udSDK Collider to estimate surfaces in front of the camera for raycasting.
- Projects demo - Example of importing udStream Projects including a uds and POIs into Unity

Examples of use of the  API features are located under Assets/Plugins/EuclideonUdSDK/Scenes 
## Basic Example

This is an example demonstrating how to use udSDK with Unity, it includes a minimalist example of a flight camera and an attached collider.
Unlimited detail rendering is implemented as a postprocessing effect that can be applied to cameras in order to display UDS objects.


### Sample Scene Structure
![The Structure of the basic sample scene](./docs/sampleSceneStructure.png "Structure of the Sample Scene")

#### Main Camera

The main view used in the example. It has a flight camera script attached to it to enable user interation. Of importance to this object is 
the implemented post process layer and volume properties which must be included for the camera to view UDS files. 

![Sample UDS camera Settings](./docs/sampleCameraSettings.png "Sample UDS Camera Settings")

Various settings can be passed to the renderer including the point rendering mode (rectangle, cube or point), the desired resolution scaling and example interfaces for picking of voxels

### UDS Model

Each of UDS to be loaded in unity is represented as a one of these models.

### udLogin

This file contains the login logic for the unity example, including login credentials. GlobaludSDKContext contains a ``` udContext``` for managing licensing 
and login information between objects, and a ```udRenderContext```, enabling the rendering of and caching the UDS model information

### UDPPER 

_UDPPES.cs_ contains the implemention of udSDK in Unity as a post processing effect. The associated shader is ```udShader.shader```

### UD Collider

This object demonstrates how to achieve physical collisions between Euclideon UDS models and native Unity colliders. Because of the potential scale of UDS objects it is not practical to construct mesh colliders of UDS objects (espeially if these objects are being streamed externally)
The approach taken is to construct a mesh of the UDS object local to a point of interest (for example a player or the potential colliding object using information available from an instance of ```udRenderView```. 

Because the information contained in UDS files (especially unfiltered point clouds) can be noisy, we have included functionality to smooth the generated surfaces.

_UDCollider.cs_ contains the majority of the logic associated with the example collider system.
 
![Collider Object Structure](./docs/colliderStructure.png "Collider Object Structure")

_Because Unity does not allow collisions between a parent and child object, the collider cannot be a direct decendent of an object it is intended to collide with. 
Instead use the follow target parameter to keep the collider within an objects reference frame_

The UD Colider Script takes the following parameters:

_Follow Target:_ If not none will set the transformation of the collider to match that of the target, useful for meshing locally around 
particular objects. 

_Threshold Follow:_ Determines the rate at which the location of the collider is updated, if true the location of the collider is updated only when the object is _Follow Threshold_ distance from the watcher position

_Watcher Pos:_ The location of the virtual camera in the local frame used to generate the collision mesh. Currently this should be set to 0.5*(ZNear-Zfar) in the direction the cameara is intended to look, this is to prevent unwanted clipping

_width, height:_ the dimensions of the polygon 'sheet' draped over the UDS model in metres, smaller colliders will have a higher polygon density and therefore better collision accuracy at lower computational cost

_Z Near, Z far:_ the locations of the near and far plane along the local z axis for the 'watcher' camera used to generate the collision polygon. 

_Width Pix, Height Pix:_ The number of pixels used to find the position of the vertices in the sheet. Currently this is equal to the number of vertices in the collision mesh. Smoother results may be obtained in future versions by oversampling around the vertex locations and averaging to eliminate noise.
These values have a large impact on frame rate and should be kept as low as possible. Increasing these improves the accuracy of the produced collision mesh.

_Laplacian Smoothing:_ This determines if smoothing should be applied to the collider, this has the effect of removing noise from laser scans at the cost of potentially removing 'sharp' features from the collision mesh.

Smoothing off (note the rougher ground surface due to sensor noise):
![UnSmoothed](./docs/ColliderUnfiltered.png "Unsmoothed Collision Polygon") 

Smoothing on (note that tree branches are no longer captured by the collision model):
![Smoothed](./docs/ColliderFiltered.png "Smoothed Collision Polygon")

As the attached mesh is modified often by this script, baking options for the mesh collider should be turned off for performance reasons



## Android Support

The provided scenes have been tested on Android:
Compiling to Android involves the following:
- Copy udSDK.so from "[udSDK Location]\lib\android_arm64\libudSDK.so" (or android X64 depending on your target platform) to Assets/Plugins/EuclideonUdSDK
- Select the newly added file from the Unity project pane and in the inspector under platform settings select your target platform and check the box "load on startup"
- In File -> Build Settings ensure that the Menu 3d Scene is loaded first, then the forst scene in your application
- In the bottom left of this window click player settings -> Sndroid settings, set the scripting backend to IL2CPP and the target architecture to ARM64 (or X64 if you are using that platform)


