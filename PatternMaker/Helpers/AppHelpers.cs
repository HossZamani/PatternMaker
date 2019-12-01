using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace PatternMaker
{
    public static class AppHelpers
    {
        public static Window GetRevitWindow()
        {
            try
            {
                Process revitProcess = Process.GetCurrentProcess();
                IntPtr handle = revitProcess.MainWindowHandle;
                HwndSource hwndSource = HwndSource.FromHwnd(handle);
                Window revitWindow = hwndSource.RootVisual as Window;

                if (revitWindow.Title.StartsWith("Autodesk"))
                {
                    return revitWindow;
                }
                else
                {
                    return null;
                }
            }
            catch
            {

                return null;
            }
        }

        public static void SetWindowLocationBasedOnRevit(Window window)
        {
            Window revitWindow = GetRevitWindow();

            if (null != revitWindow)
            {
                double[] leftTop = new double[2];

                if (revitWindow.WindowState == WindowState.Maximized)
                {
                    var leftField = typeof(Window).GetField("_actualLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    window.Left = (double)leftField.GetValue(revitWindow) + 5;

                    var topField = typeof(Window).GetField("_actualTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    window.Top = (double)topField.GetValue(revitWindow) + 180;
                }
                else
                {
                    window.Left = revitWindow.Left + 2;
                    window.Top = revitWindow.Top + 180;
                }
            }

        }

    }
}
