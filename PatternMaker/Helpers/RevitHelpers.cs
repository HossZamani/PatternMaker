using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PatternMaker
{

    public class RevitFillGrid
    {
        public FillGrid RvtFillGrid;
        public double Scale;

        public RevitFillGrid(FillGrid rvtFillGrid, double scale)
        {
            this.RvtFillGrid = rvtFillGrid;
            this.Scale = scale;
        }

        public PatternPoint Origin
        {
            get { return new PatternPoint(this.RvtFillGrid.Origin.U, this.RvtFillGrid.Origin.V); }
        }

        public double Angle
        {
            get { return this.RvtFillGrid.Angle; }
        }

        public double Offset
        {
            get { return this.RvtFillGrid.Offset; }
        }

        public double Shift
        {
            get { return this.RvtFillGrid.Shift; }

        }

        public IList<double> Segments
        {
            get { return this.RvtFillGrid.GetSegments(); }
        }

        public FillGrid GetRvtFillGrid()
        {
            FillGrid rvtFillGrid = new FillGrid();
            rvtFillGrid.Origin = new UV(this.Origin.U * this.Scale, this.Origin.V * this.Scale);
            rvtFillGrid.Angle = this.Angle;
            rvtFillGrid.Offset = this.Offset * this.Scale;
            rvtFillGrid.Shift = this.Shift * this.Scale;

            IList<double> scaledSegments = new List<double>();

            foreach (double x in this.RvtFillGrid.GetSegments())
            {
                scaledSegments.Add(x * this.Scale);
            }
            rvtFillGrid.SetSegments(scaledSegments);

            return rvtFillGrid;


        }
    }

    public class RevitPattern
    {
        public PatternDomain Domain;
        public List<object> PatternGrids;
        public List<FillGrid> Input_FillGrids;
        public string Name;
        public bool ModelPattern;
        public double Scale;

        public RevitPattern(PatternDomain patDomain, string patName, bool modelPat = true, double scale = 1.0)
        {

            this.Domain = patDomain;
            this.PatternGrids = new List<object>();
            this.Input_FillGrids = new List<FillGrid>();
            this.Name = patName;
            this.ModelPattern = modelPat;
            this.Scale = scale;

        }

        public void Append_FillGrid(FillGrid fillGrid)
        {
            this.PatternGrids.Add(new RevitFillGrid(fillGrid, this.Scale));
        }

        public void Append_Line(PatternLine patternLine)
        {
            PatternLine domainLine = this.Domain.GetDomainCoordinates(patternLine);

            // TaskDialog.Show("test", "DomainLine: ST: " + domainLine.StartPoint.U.ToString() + " " + domainLine.StartPoint.V.ToString() + " ED: " + domainLine.EndPoint.U.ToString() + " " + domainLine.EndPoint.V.ToString() + " LG:" + domainLine.Length.ToString() + " AG: " + domainLine.Angle.ToString());


            PatternGrid newGrid = new PatternGrid(this.Domain, domainLine);
            //TaskDialog.Show("test", "PatternGrid: AN:" + newGrid.Angle.ToString() + " SP:" + newGrid.Span.ToString() + " OF: " + newGrid.Offset.ToString() + " SH: " + newGrid.Shift.ToString());

            this.PatternGrids.Add(newGrid);
        }

        public FillGrid MakeFillGrid(object patternGrid)
        {
            FillGrid fillGrid;
            if (patternGrid is RevitFillGrid)
            {
                RevitFillGrid rFG = patternGrid as RevitFillGrid;
                fillGrid = rFG.GetRvtFillGrid();
            }
            else
            {
                PatternGrid pG = patternGrid as PatternGrid;
                double scale = this.Scale;
                fillGrid = new FillGrid();
                fillGrid.Angle = pG.Angle;
                fillGrid.Origin = new UV(pG.Origin().U * scale, pG.Origin().V * scale);
                fillGrid.Offset = pG.Offset * scale;
                fillGrid.Shift = (double)pG.Shift * scale;
                if (pG.Segments().Any())
                {
                    IList<double> scaledSegments = new List<double>();
                    foreach (double seg in pG.Segments())
                    {
                        scaledSegments.Add(seg * scale);
                    }
                    fillGrid.SetSegments(scaledSegments);
                }
            }
            return fillGrid;
        }

        public FillPatternElement MakeFillPatternElement(Document doc, FillPattern fillPattern)
        {
            // find existing filled pattern element matching name and pattern target
            FilteredElementCollector fCollector = new FilteredElementCollector(doc);
            IList<Element> existingFillPatternElements = fCollector.OfClass(typeof(FillPatternElement)).ToElements();

            FillPatternElement fillPatternElement = null;
            foreach (Element el in existingFillPatternElements)
            {
                FillPatternElement fPE = el as FillPatternElement;
                FillPattern exFP = fPE.GetFillPattern();
                if (fillPattern.Name == exFP.Name & fillPattern.Target == exFP.Target)
                {
                    fillPatternElement = fPE;
                }
            }

            if (null != fillPatternElement)
            {
                using (Transaction t = new Transaction(doc, "Create Fill Pattern"))
                {
                    t.Start();
                    fillPatternElement.SetFillPattern(fillPattern);

                    // debug
                    //ElementId id = fillPatternElement.Id;

                    t.Commit();
                }
            }

            else
            {
                using (Transaction t = new Transaction(doc, "Create Fill Pattern"))
                {
                    t.Start();
                    fillPatternElement = FillPatternElement.Create(doc, fillPattern);

                    // debug
                    //ElementId id = fillPatternElement.Id;


                    t.Commit();
                }
            }

            // debug
            //string nameCheck = fillPatternElement.Name;
            //FillPattern patternCheck = fillPatternElement.GetFillPattern();
            //IList<FillGrid> fillGridsCheck = patternCheck.GetFillGrids();
            //foreach( FillGrid t in patternCheck.GetFillGrids())
            //{
            //Debug.Print("Fill Grid, origin:" + t.Origin.ToString() + " angle:" + t.Angle.ToString() + " shift:" + t.Shift.ToString() + " offset:" + t.Offset.ToString() + " segments:" + t.GetSegments()[0] + " " + t.GetSegments()[1]);
            //}


            return fillPatternElement;

        }

        public FillPatternElement CreatePattern(Document doc)
        {
            List<FillGrid> fillGrids = new List<FillGrid>();
            foreach (object x in this.PatternGrids)
            {
                fillGrids.Add(this.MakeFillGrid(x));
            }
            FillPatternTarget fpTarget;
            if (this.ModelPattern)
            {
                fpTarget = FillPatternTarget.Model;
            }
            else
            {
                fpTarget = FillPatternTarget.Drafting;
            }

            FillPattern fillPattern = new FillPattern(this.Name, fpTarget, FillPatternHostOrientation.ToHost);
            fillPattern.SetFillGrids(fillGrids);

            return this.MakeFillPatternElement(doc, fillPattern);
        }

    }

    public class PatUtils
    {
        public static FilledRegionType MakeFilledRegion(Document doc, string name, ElementId elementId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilledRegionType sourceFilledRegionType = collector.OfClass(typeof(FilledRegionType)).ToElements().First() as FilledRegionType;
            FilledRegionType newFilledRegionType;
            using (Transaction t = new Transaction(doc, "Create Filled Region"))
            {
                t.Start();
                newFilledRegionType = sourceFilledRegionType.Duplicate(name) as FilledRegionType;
#if RELEASE2019 || TESTING2019 || RELEASE2018 || TESTING2018
                newFilledRegionType.FillPatternId = elementId;
#endif
#if RELEASE2020 || TESTING2020
                newFilledRegionType.ForegroundPatternId = elementId;
#endif
                t.Commit();

            }

            return newFilledRegionType;
        }

        public static FillPatternElement CreateFillPattern(Document doc, RevitPattern revitPattern, bool createFilledRegion = false)
        {

            FillPatternElement fPE = revitPattern.CreatePattern(doc);
            if (createFilledRegion)
            {
                MakeFilledRegion(doc, fPE.Name, fPE.Id);
            }
            return fPE;

        }

        public static RevitPattern MakeRevitPattern(string patName, List<double[][]> patternLines, double[][] domain, List<FillGrid> fillGrids = null, double scale = 1.0, bool modelPattern = true, bool allowExpansion = false)
        {
            PatternDomain patternDomain = new PatternDomain(domain[0][0], domain[0][1], domain[1][0], domain[1][1], modelPattern, allowExpansion);
            //TaskDialog.Show("test", "new pattern domain: U:" + patternDomain.Bounds.U.ToString() + " V:" + patternDomain.Bounds.V.ToString() + "SA" + patternDomain.safeAngles.Count.ToString());

            RevitPattern revitPattern = new RevitPattern(patternDomain, patName, modelPattern, scale);

            //TaskDialog.Show("test", "new revit pattern: N: " + revitPattern.Name + " M: " + revitPattern.ModelPattern.ToString() + " S: " + revitPattern.Scale.ToString());

            foreach (double[][] lineCoordinates in patternLines)
            {
                PatternPoint startP = new PatternPoint(lineCoordinates[0][0], lineCoordinates[0][1]);
                PatternPoint endP = new PatternPoint(lineCoordinates[1][0], lineCoordinates[1][1]);
                PatternLine patternLine = new PatternLine(startP, endP);
                revitPattern.Append_Line(patternLine);
            }

            if (fillGrids != null)
            {
                foreach (FillGrid fillGrid in fillGrids)
                {
                    revitPattern.Append_FillGrid(fillGrid);
                }
            }

            return revitPattern;

        }

        public static FillPatternElement MakePattern(Document doc, string patName, List<double[][]> patternLines, double[][] domain, List<FillGrid> fillGrids = null, double scale = 1.0, bool modelPattern = true, bool allowExpansion = false, bool createFilledRegion = false)
        {
            RevitPattern revitPattern = MakeRevitPattern(patName, patternLines, domain, fillGrids, scale, modelPattern, allowExpansion);
            return CreateFillPattern(doc, revitPattern, createFilledRegion);
        }
    }
}
