using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PatternMaker.Views
{
    /// <summary>
    /// Interaction logic for PatternMakerUI.xaml
    /// </summary>
    public partial class PatternMakerUI : MetroWindow
    {
        private ExternalEvent exEvent;
        private PatternMakerEventHandler exEventHandler;
        public static PatternMakerUI userInputs;
        public bool firstShow;
        public bool createFrame;
        public bool generatePattern;
        public bool cancelOperation;
        public bool previewPattern;
        public bool ClosedWithX;

        private List<string> ExistingModelPatternNames;
        private List<string> ExistingDraftingPatternNames;

        public ViewDrafting hatchView;
        public List<ElementId> frameElements;


        private UIApplication _uiApp;
        private UIDocument _uiDoc;

        public PatternMakerUI(ExternalEvent exEvent, PatternMakerEventHandler handler, UIApplication uiApp)
        {
            InitializeComponent();
            userInputs = this;
            this.exEvent = exEvent;
            exEventHandler = handler;
            _uiApp = uiApp;

            firstShow = true;
            createFrame = true;
            generatePattern = false;
            cancelOperation = false;
            previewPattern = false;
            ClosedWithX = true;

            frameElements = new List<ElementId>();
            hatchView = null;


            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            var fillPatterns = new List<FillPattern>();

            var fpElems = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().ToList();
            fpElems.ForEach(a=> fillPatterns.Add(a.GetFillPattern()));
            ExistingModelPatternNames = fillPatterns.Where(a => a.Target == FillPatternTarget.Model).Select(a => a.Name).ToList();


            ExistingDraftingPatternNames = fillPatterns.Where(a => a.Target == FillPatternTarget.Drafting).Select(a => a.Name).ToList();

            _uiDoc = uiDoc;

            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                userInputs.ClosedWithX = false;
                exEvent.Raise();
                Hide();
            }
            catch (Exception ex)
            {   
                string message = ex.Message;
                TaskDialog.Show("Error", message.ToString());
                return;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

            userInputs.generatePattern = false;
            userInputs.firstShow = false;
            userInputs.createFrame = false;
            userInputs.cancelOperation = true;
            userInputs.ClosedWithX = false;

            exEvent.Raise();

            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ClosedWithX)
            {
                userInputs.generatePattern = false;
                userInputs.firstShow = false;
                userInputs.createFrame = false;
                userInputs.cancelOperation = true;

                exEvent.Raise();

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = sender as Window;

            AppHelpers.SetWindowLocationBasedOnRevit(window);
        }

        private void PatName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(patName.Text)) Ok.IsEnabled = false;
            else Ok.IsEnabled = true;

            if ((bool)isDrafting.IsChecked)
            {
                if (ExistingDraftingPatternNames.Contains(patName.Text))
                {
                    patName.BorderThickness = new Thickness(2);
                    patName.BorderBrush = Brushes.Yellow;
                    Ok.IsEnabled = false;
                }
                else
                {
                    patName.BorderBrush = null;
                    patName.BorderThickness = new Thickness(0);
                }
            }
            else
            {
                if (ExistingModelPatternNames.Contains(patName.Text))
                {

                    patName.BorderThickness = new Thickness(2);
                    patName.BorderBrush = Brushes.Red;
                    Ok.IsEnabled = false;
                }
                else
                {
                    patName.BorderBrush = null;
                    patName.BorderThickness = new Thickness(0);
                }
            }
        }
    }
}

