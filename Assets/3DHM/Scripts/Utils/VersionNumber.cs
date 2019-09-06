using UnityEngine;
using System.Reflection;

/// <summary>
/// Helper class to get a version number string, created as a combination of
///  the standard Unity version string (Edit | Project Settings | Player) and
///  an automated assembly-based build code. The build code consists of
///  
////    <day count><time of day in seconds>
/// 
//// Where <day count> seems to be days since 1/1/2000 or so.
////  I haven't fully figured out when <time of day in seconds> gets updated - 
////  It gets updated when a build is performed, and I think also when the editor
///   recompiles source files.
/// 
/// The automated build code method is from https://xeophin.net/en/blog/2014/05/09/simple-version-numbering-unity3d
/// </summary>
/// <remarks>
/// AssemblyVersion:
/// The first two numbers are set manually here, but we will skip them in our output
/// The * generates two numbers automatically, the build number (which is increased automatically
///  once a day (seems to be days from 1/1/2000?), and the revision number which is increased every second). 
///  In the output of this class, we'll call those two numbers together the build.
/// </remarks>
[assembly: AssemblyVersion("1.0.*")]
public class VersionNumber : MonoBehaviorSingleton<VersionNumber>
{ 
    string version;
 
    /// <summary>
    /// Gets the version.
    /// </summary>
    /// <value>The version.</value>
    public string Version
    {
        get
        {
            if (version == null)
            {
                version = Application.version;
                string build = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //Remove the first two numbers, then append to Unity version string
                version += build.Substring(3);
            }
            return version;
        }
    }


    /// Use this for initialization
    void Start()
    {
        // Log current version in log file
        Debug.Log(string.Format("Currently running version is {0}", Version));
    }

}