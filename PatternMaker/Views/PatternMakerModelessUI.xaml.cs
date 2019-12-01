using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MahApps.Metro.Controls;
using System;
using System.Windows;


namespace PatternMaker.Views
{
    /// <summary>
    /// Interaction logic for PatternMakerModelessUI.xaml
    /// </summary>
    public partial class PatternMakerModelessUI : MetroWindow
    {
        public static PatternMakerModelessUI userInputs;

        private ExternalEvent ExEvent;
        private UIApplication _uiApp;
        private UIDocument _uiDoc;

        bool ClosedWithX;

        public PatternMakerModelessUI(ExternalEvent exEvent, UIApplication uiApp)
        {
            InitializeComponent();
            userInputs = this;

            ExEvent = exEvent;
            _uiApp = uiApp;

            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uiDoc = uiApp.ActiveUIDocument;

            _uiDoc = uiDoc;

            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;

            ClosedWithX = true;
            this.Topmost = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClosedWithX = false;
            Close();
            PatternMakerUI.userInputs.cancelOperation = true;
            ExEvent.Raise();

        }

        private void go_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            PatternMakerUI.userInputs.generatePattern = true;
            ClosedWithX = false;
            ExEvent.Raise();
        }

        private void Reset_Frame_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            PatternMakerUI.userInputs.createFrame = true;
            PatternMakerUI.userInputs.ClosedWithX = true;
            PatternMakerUI.userInputs.Show();
        }

        private void preview_Click(object sender, RoutedEventArgs e)
        {
            PatternMakerUI.userInputs.previewPattern = true;
            ExEvent.Raise();

        }

        private void ModelessWindow_Closed(object sender, EventArgs e)
        {
            if (ClosedWithX)
            {
                PatternMakerUI.userInputs.generatePattern = false;
                PatternMakerUI.userInputs.createFrame = false;
                PatternMakerUI.userInputs.previewPattern = false;
                PatternMakerUI.userInputs.cancelOperation = true;
                ExEvent.Raise();

            }

        }

        private void ModelessWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = sender as Window;

            AppHelpers.SetWindowLocationBasedOnRevit(window);
        }
    }
}
