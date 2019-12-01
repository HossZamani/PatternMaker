using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace PatternMaker
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PatternMakerCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiApp = commandData.Application;

            View activeView = uiApp.ActiveUIDocument.Document.ActiveView;

            PatternMakerEventHandler handler = new PatternMakerEventHandler();

            // External Event for the dialog to use (to post requests)
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            // We give the objects to the new dialog;
            // The dialog becomes the owner responsible for disposing them, eventually.
            var mainWindow = new Views.PatternMakerUI(exEvent, handler, uiApp);

            mainWindow.ShowDialog();

            return Result.Succeeded;

        }

    }
}
