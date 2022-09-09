____Guide for updating wrapper____

The files in this folder ought to map to the files in the wrapper 1:1, with a few exceptions being: 
- udSDKAPI.cs 
  - This contains the library name reference for DLLImport
- UnitySupport.cs
  - This contains objects adapted for use in Unity scripts, and Unity serialization

Some things do not come across into C# from the SDK neatly, including struct unions and typedefs. 
As such they are omitted where they occur, as is the whole file of 'udDLLExport'.

Functions are brought across as static members of a public static class; this greatly simplifies implementation, while observing the requirement that DLLImport be attributed to a member function.
As such:
- Any number of handler objects can be specified without reimplementing the function call 
- The function can now be called from anywhere, allowing more flexibility 
- Functions can be lined up according to their declaration in the equivalent header file

