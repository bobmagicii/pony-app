using static Microsoft.Win32.Registry;

namespace SimpleMaid
{
  public static class SimpleApp
  {
    public static bool IsElevated()
    {
      try { using (LocalMachine.OpenSubKey("Software\\", true)) ; }
      catch { return false; }
      return true;
    }

    public static void SwitchAutorun(string appName, string appPath = null)
    {
      string regPath = null;
      using (var regKey = CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", false))
      {
        regPath = regKey.GetValue(appName)?.ToString();
      }

      if (SimpleIO.Path.Equals(appPath, regPath))
        return;

      using (var regKey = CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run"))
      {
        if (appPath != null)
          regKey.SetValue(appName, appPath);
        else
          regKey.DeleteValue(appName);
      }
    }

    public static bool VerifyAutorun(string appName, string appPath)
    {
      string regPath = null;
      using (var regKey = CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", false))
      {
        regPath = regKey.GetValue(appName)?.ToString();
      }

      return SimpleIO.Path.Equals(appPath, regPath);
    }
  }
}
