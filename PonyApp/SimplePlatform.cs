using static System.Environment;
using static System.PlatformID;

namespace SimpleMaid
{
  public static class SimplePlatform
  {
    public enum Platform
    {
      Windows,
      Unix
    }

    public static Platform runningPlatform()
    {
      switch (OSVersion.Platform)
      {
        case Unix:
        case MacOSX:
          return Platform.Unix;

        default:
          return Platform.Windows;
      }
    }
  }
}
