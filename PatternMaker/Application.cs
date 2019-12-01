using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace PatternMaker
{
    // class to disable the button if we are inside a family document 
    public class NonFamilyDocAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            try
            {
                return !a.ActiveUIDocument.Document.IsFamilyDocument;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Get assembly path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var panel = application.CreateRibbonPanel("Pattern Maker");

            // Create push button data
            PushButtonData buttonData = new PushButtonData("PatternMaker", "Pattern" + Environment.NewLine + " Maker ", assemblyPath, "PatternMaker.PatternMakerCommand");

            buttonData.AvailabilityClassName = "PatternMaker.NonFamilyDocAvailability";

            // Create the push button
            PushButton pushButton = panel.AddItem(buttonData) as PushButton;

            // Set button image
            BitmapImage icon = new BitmapImage(new Uri("pack://application:,,,/PatternMaker;component/Resources/PatternMaker-32.png"));
            pushButton.LargeImage = icon;

            // Set button tooltip
            pushButton.ToolTip = "Creates pattern. Ported from pyRevit.";

            return Result.Succeeded;
        }
    }
}
