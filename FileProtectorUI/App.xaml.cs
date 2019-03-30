using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using FileProtectorUI.Utils;
using MS.WindowsAPICodePack.Internal;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using static FileProtectorUI.CommonResources.Constants;

namespace FileProtectorUI
{
    public partial class App : Application
    {
        public App() : base()
        {
            TryCreateShortcut();
        }

        private bool TryCreateShortcut()
        {
            String shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\File Protector.lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                return true;
            }
            return false;
        }

        private void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(APP_ID))
            {
                ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }
    }
}
