using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PatternMaker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PatternMaker
{
    public class PatternMakerEventHandler : IExternalEventHandler
    {
        public static int resolution = 2;

        public void Execute(UIApplication uiApp)
        {

            int PICK_COORD_RESOLUTION = 12;

            PatternMakerEventHandler handler = new PatternMakerEventHandler();

            // External Event for the dialog to use (to post requests)
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            // We give the objects to the new dialog;
            // The dialog becomes the owner responsible for disposing them, eventually.
            PatternMakerModelessUI modelessUI = new PatternMakerModelessUI(exEvent, uiApp);

            try
            {
                Document doc = uiApp.ActiveUIDocument.Document;

                var patGridX = (double)Views.PatternMakerUI.userInputs.patGridX.Value * 0.00328084;
                var patGridY = (double)Views.PatternMakerUI.userInputs.patGridY.Value * 0.00328084;

                string patName = Views.PatternMakerUI.userInputs.patName.Text;

                FilteredElementCollector pcollector = new FilteredElementCollector(doc);

                IList<Element> patterns = pcollector.OfClass(typeof(FillPatternElement)).ToElements();
                IList<FillPatternElement> castedPatterns = patterns.Cast<FillPatternElement>().ToList();

                View view = doc.ActiveView;
                ViewDrafting viewDrafting = null;

                List<ElementId> frameElements = new List<ElementId>();

                ViewFamilyType draftingViewType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(q => q.ViewFamily == ViewFamily.Drafting);

                TextNoteType textType = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().FirstOrDefault();

                double gridFactor = patGridX / 10; // an arbitrary factor for creating frame elements and pattern preview
                if (gridFactor < uiApp.Application.ShortCurveTolerance)
                {
                    gridFactor = uiApp.Application.ShortCurveTolerance;
                }


                using (TransactionGroup tGroup = new TransactionGroup(doc, "Generate Pattern"))
                {
                    tGroup.Start();
                    if (Views.PatternMakerUI.userInputs.firstShow)
                    {
                        using (Transaction t = new Transaction(doc, "create a new drafting view"))
                        {
                            // create a fresh drafting view to start
                            t.Start();
                            viewDrafting = ViewDrafting.Create(doc, draftingViewType.Id);
                            viewDrafting.Name = patName + "_TEMP VIEW_Sketch your pattern here";
                            viewDrafting.Scale = 1;
                            doc.Regenerate();
                            t.Commit();
                            uiApp.ActiveUIDocument.RequestViewChange(viewDrafting);

                        }

                        Views.PatternMakerUI.userInputs.firstShow = false;
                        Views.PatternMakerUI.userInputs.patName.Text = patName;
                        Views.PatternMakerUI.userInputs.hatchView = viewDrafting;
                        Views.PatternMakerUI.userInputs.patName.IsEnabled = false;
                    }

                    if (Views.PatternMakerUI.userInputs.createFrame)
                    {
                        using (Transaction t = new Transaction(doc, "create frame"))
                        {
                            t.Start();

                            if (Views.PatternMakerUI.userInputs.frameElements.Count != 0)
                            {
                                foreach (object item in Views.PatternMakerUI.userInputs.frameElements)
                                {
                                    if (null != item)
                                    {
                                        ElementId elemId = item as ElementId;
                                        if (null != elemId) doc.Delete(elemId);
                                    }

                                }
                            }

                            if (viewDrafting == null)
                            {
                                viewDrafting = doc.ActiveView as ViewDrafting;
                            }
                            // create a frame which shows the extent of grid cell
                            XYZ bLCenter = new XYZ(0, 0, 0);
                            XYZ bLBottom = new XYZ(0, -gridFactor, 0);
                            XYZ bLLeft = new XYZ(-gridFactor, 0, 0);

                            XYZ bRCenter = new XYZ(patGridX, 0, 0);
                            XYZ bRBottom = new XYZ(patGridX, -gridFactor, 0);
                            XYZ bRRight = new XYZ(patGridX + gridFactor, 0, 0);

                            XYZ tRCenter = new XYZ(patGridX, patGridY, 0);
                            XYZ tRTop = new XYZ(patGridX, patGridY + gridFactor, 0);
                            XYZ tRRight = new XYZ(patGridX + gridFactor, patGridY, 0);

                            XYZ tLCenter = new XYZ(0, patGridY, 0);
                            XYZ tLTop = new XYZ(0, patGridY + gridFactor, 0);
                            XYZ tLLeft = new XYZ(-gridFactor, patGridY, 0);

                            Line l1 = Line.CreateBound(bLCenter, bLBottom);
                            Line l2 = Line.CreateBound(bLCenter, bLLeft);

                            Line l3 = Line.CreateBound(tRCenter, tRRight);
                            Line l4 = Line.CreateBound(tRCenter, tRTop);

                            Line l5 = Line.CreateBound(bRCenter, bRRight);
                            Line l6 = Line.CreateBound(bRCenter, bRBottom);

                            Line l7 = Line.CreateBound(tLCenter, tLTop);
                            Line l8 = Line.CreateBound(tLCenter, tLLeft);

                            DetailLine line1 = doc.Create.NewDetailCurve(viewDrafting, l1) as DetailLine;
                            DetailLine line2 = doc.Create.NewDetailCurve(viewDrafting, l2) as DetailLine;

                            DetailLine line3 = doc.Create.NewDetailCurve(viewDrafting, l3) as DetailLine;
                            DetailLine line4 = doc.Create.NewDetailCurve(viewDrafting, l4) as DetailLine;

                            DetailLine line5 = doc.Create.NewDetailCurve(viewDrafting, l5) as DetailLine;
                            DetailLine line6 = doc.Create.NewDetailCurve(viewDrafting, l6) as DetailLine;

                            DetailLine line7 = doc.Create.NewDetailCurve(viewDrafting, l7) as DetailLine;
                            DetailLine line8 = doc.Create.NewDetailCurve(viewDrafting, l8) as DetailLine;

                            line1.Pinned = true;
                            line2.Pinned = true;
                            line3.Pinned = true;
                            line4.Pinned = true;
                            line5.Pinned = true;
                            line6.Pinned = true;
                            line7.Pinned = true;
                            line8.Pinned = true;

                            frameElements.Add(line1.Id);
                            frameElements.Add(line2.Id);
                            frameElements.Add(line3.Id);
                            frameElements.Add(line4.Id);
                            frameElements.Add(line5.Id);
                            frameElements.Add(line6.Id);
                            frameElements.Add(line7.Id);
                            frameElements.Add(line8.Id);

                            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings().SetProjectionLineColor(new Color(255, 0, 0));

                            foreach (ElementId item in frameElements)
                            {
                                viewDrafting.SetElementOverrides(item, overrideGraphicSettings);

                            }

                            doc.Regenerate();
                            t.Commit();

                        }

                        Views.PatternMakerUI.userInputs.frameElements = frameElements;
                        Views.PatternMakerUI.userInputs.createFrame = false;
                        modelessUI.Show();


                        TaskDialog.Show("Alright..", "Now start drawing your lines within the marked area. If you go beyond this area repeating modules will overlap.\n\nNOTE: Do not modify the scale of the view!");

                        IList<UIView> uIViews = uiApp.ActiveUIDocument.GetOpenUIViews();
                        UIView uIView = null;

                        foreach (UIView v in uIViews)
                        {
                            if (v.ViewId == Views.PatternMakerUI.userInputs.hatchView.Id)
                            {
                                uIView = v;
                            }
                        }

                        uIView.ZoomToFit();

                        RevitCommandId detLineCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.DetailLine);
                        uiApp.PostCommand(detLineCommandId);

                    }

                    if (Views.PatternMakerUI.userInputs.generatePattern)
                    {
                        List<FillGrid> fillGrids = new List<FillGrid>();

                        // collect all line elements in view
                        frameElements = Views.PatternMakerUI.userInputs.frameElements;

                        FilteredElementCollector lcollector = new FilteredElementCollector(doc, Views.PatternMakerUI.userInputs.hatchView.Id);
                        IList<Element> allElements = lcollector.ToElements();

                        List<Element> hatchElements = allElements.Where(x => !frameElements.Contains(x.Id)).ToList();
                        List<Curve> geomCurves = new List<Curve>();

                        foreach (object lineElem in hatchElements)
                        {
                            CurveElement curveElement = lineElem as CurveElement;
                            try
                            {
                                geomCurves.Add(curveElement.GeometryCurve);
                            }
                            catch
                            {

                            }
                        }

                        List<double[][]> lineTuples = new List<double[][]>();
                        foreach (Curve curve in geomCurves)
                        {
                            if (curve is Line)
                            {
                                XYZ start = curve.GetEndPoint(0);
                                XYZ end = curve.GetEndPoint(1);

                                lineTuples.Add(new double[][] { new double[] { start.X, start.Y }, new double[] { end.X, end.Y } });
                            }

                            else
                            {
                                IList<XYZ> xyzs = curve.Tessellate();
                                var pairs = xyzs.Where((e, i) => i < xyzs.Count - 1).Select((e, i) => new { A = e, B = xyzs[i + 1] }).ToList();

                                foreach (var item in pairs)
                                {
                                    lineTuples.Add(new double[][] { new double[] { item.A.X, item.A.Y }, new double[] { item.B.X, item.B.Y } });
                                }
                            }

                        }
                        double[][] domain = new double[2][];

                        // to check : the 0 index doesn't seem to be correct
                        domain[0] = new double[] { 0, 0 };
                        domain[1] = new double[] { Math.Round(patGridX, PICK_COORD_RESOLUTION), Math.Round(patGridY, PICK_COORD_RESOLUTION) };

                        bool modelPattern;
                        if ((bool)Views.PatternMakerUI.userInputs.isModel.IsChecked)
                        {
                            modelPattern = true;
                        }
                        else
                        {
                            modelPattern = false;
                        }


                        PatUtils.MakePattern(doc, patName, lineTuples, domain, modelPattern: modelPattern);

                        Views.PatternMakerUI.userInputs.generatePattern = false;
                        Views.PatternMakerUI.userInputs.Close();
                        string patType;

                        if (modelPattern)
                        {
                            patType = "Model";
                        }
                        else
                        {
                            patType = "Drafting";
                        }

                        TaskDialog.Show("Good..", "Fill Pattern: " + patName + " created successfully.\n\nType: " + patType);

                        // delete temporary view
                        using (Transaction t = new Transaction(doc, "deleting temporary view"))
                        {
                            t.Start();

                            IList<UIView> uIViews = uiApp.ActiveUIDocument.GetOpenUIViews();
                            UIView uIView = null;

                            foreach (UIView v in uIViews)
                            {
                                if (v.ViewId == Views.PatternMakerUI.userInputs.hatchView.Id)
                                {
                                    uIView = v;
                                }
                            }
                            uIView.Close();
                            doc.Delete(Views.PatternMakerUI.userInputs.hatchView.Id);

                            t.Commit();
                        }

                    }

                    if (Views.PatternMakerUI.userInputs.cancelOperation)
                    {
                        if (Views.PatternMakerUI.userInputs.hatchView != null)
                        {

                            UIView uIView = null;
                            // delete temporary view
                            using (Transaction t = new Transaction(doc, "deleting temporary view"))
                            {
                                t.Start();

                                IList<UIView> uIViews = uiApp.ActiveUIDocument.GetOpenUIViews();


                                foreach (UIView v in uIViews)
                                {
                                    if (v.ViewId == Views.PatternMakerUI.userInputs.hatchView.Id)
                                    {
                                        uIView = v;
                                    }
                                }
                                if (uIView != null)
                                {
                                    uIView.Close();
                                    try
                                    {
                                        doc.Delete(Views.PatternMakerUI.userInputs.hatchView.Id);

                                    }
                                    catch
                                    {

                                    }
                                }

                                t.Commit();
                            }


                        }

                    }

                    if (Views.PatternMakerUI.userInputs.previewPattern)
                    {
                        FilteredElementCollector lcollector = new FilteredElementCollector(doc, Views.PatternMakerUI.userInputs.hatchView.Id);
                        ICollection<ElementId> elementIds = lcollector.OfClass(typeof(CurveElement)).ToElementIds();

                        ICollection<ElementId> patternElements = elementIds.Where(l => !Views.PatternMakerUI.userInputs.frameElements.Any(o => o.IntegerValue == l.IntegerValue)).ToList();
                        ICollection<ElementId> previewElements = new List<ElementId>();
                        //Debug.Print(patternElements.Count.ToString());
                        using (Transaction t = new Transaction(doc, "repeating elements"))
                        {
                            t.Start();
                            //XYZ xYZ = new XYZ(4, 5, 0);
                            //ElementTransformUtils.CopyElements(doc, patternElements, xYZ);


                            for (double ys = patGridY + gridFactor; ys < patGridY * 5; ys += patGridY)
                            {
                                for (double xs = patGridX + gridFactor; xs < patGridX * 5; xs += patGridX)
                                {
                                    XYZ xYZ = new XYZ(xs, ys, 0);
                                    ICollection<ElementId> temp = ElementTransformUtils.CopyElements(doc, patternElements, xYZ);
                                    previewElements = new List<ElementId>(previewElements.Concat(temp));
                                }
                            }
                            t.Commit();

                        }

                        IList<UIView> uIViews = uiApp.ActiveUIDocument.GetOpenUIViews();
                        UIView uIView = null;

                        foreach (UIView v in uIViews)
                        {
                            if (v.ViewId == Views.PatternMakerUI.userInputs.hatchView.Id)
                            {
                                uIView = v;
                            }
                        }

                        uIView.ZoomToFit();

                        TaskDialog.Show("Have a look..", "Close me to go back!");

                        using (Transaction t = new Transaction(doc, "deleting repeated elements"))
                        {
                            t.Start();
                            foreach (ElementId id in previewElements)
                            {
                                doc.Delete(id);
                            }
                            t.Commit();
                        }

                        uIView.ZoomToFit();

                        RevitCommandId detLineCommandId = RevitCommandId.LookupPostableCommandId(PostableCommand.DetailLine);
                        uiApp.PostCommand(detLineCommandId);

                        Views.PatternMakerUI.userInputs.previewPattern = false;

                    }

                    tGroup.Assimilate();

                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error ", ex.Message);
                MessageBox.Show(ex.StackTrace);
            }

        }

        public string GetName()
        {
            return "External Event - Pattern Generator";
        }

        public double RoundCoordinates(double Coordinate)
        {

            return Math.Round(Coordinate, resolution);
        }
    }

 
}
