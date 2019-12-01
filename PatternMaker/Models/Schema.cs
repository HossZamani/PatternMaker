using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PatternMaker
{

    public class PatternPoint
    {
        public double U;
        public double V;

        public PatternPoint(double u, double v)
        {
            this.U = Math.Round(u, 8);
            this.V = Math.Round(v, 8);
        }

        public PatternPoint Add(PatternPoint p)
        {
            return (new PatternPoint(this.U + p.U, this.V + p.V));
        }

        public PatternPoint Sub(PatternPoint p)
        {
            return (new PatternPoint(this.U - p.U, this.V - p.V));

        }

        public double DistanceTo(PatternPoint p)
        {
            return Math.Sqrt(Math.Pow(p.U - this.U, 2) + Math.Pow(p.V - this.V, 2));
        }

        public void Rotate(PatternPoint origin, double angle)
        {
            double tu = this.U - origin.U;
            double tv = this.V - origin.V;
            this.U = origin.U + (tu * Math.Cos(angle) - tv * Math.Sin(angle));
            this.V = origin.V + (tu * Math.Sin(angle) + tv * Math.Cos(angle));

        }
    }

    public class PatternLine
    {
        public PatternPoint StartPoint;
        public PatternPoint EndPoint;
        public UV UVector;

        public double ZeroTolerance = 5e-06;

        public PatternLine(PatternPoint startPoint, PatternPoint endPoint)
        {
            if (startPoint.V <= endPoint.V)
            {
                this.StartPoint = startPoint;
                this.EndPoint = endPoint;
            }
            else
            {
                this.StartPoint = endPoint;
                this.EndPoint = startPoint;
            }
            this.UVector = new UV(1, 0);

        }

        public PatternPoint Direction
        {
            get { return new PatternPoint(this.EndPoint.U - this.StartPoint.U, this.EndPoint.V - this.StartPoint.V); }
        }

        public double Angle
        {
            get { return this.UVector.AngleTo(new UV(this.Direction.U, this.Direction.V)); }
        }

        public PatternPoint CentrePoint
        {
            get { return new PatternPoint((this.EndPoint.U + this.StartPoint.U) / 2, (this.EndPoint.V + this.StartPoint.V) / 2); }
        }

        public double Length
        {
            get { return Math.Abs(Math.Sqrt(Math.Pow(this.Direction.U, 2) + Math.Pow(this.Direction.V, 2))); }
        }

        public bool PointOnLine(PatternPoint p)
        {
            if (0.0 <= Math.Abs((this.StartPoint.U - p.U) * (this.EndPoint.V - p.V) - (this.StartPoint.V - p.V) * (this.EndPoint.U - p.U)) & Math.Abs((this.StartPoint.U - p.U) * (this.EndPoint.V - p.V) - (this.StartPoint.V - p.V) * (this.EndPoint.U - p.U)) <= ZeroTolerance)

            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public PatternPoint Intersection(PatternLine patternLine)
        {
            PatternPoint xdiff = new PatternPoint(this.StartPoint.U - this.EndPoint.U, this.StartPoint.U - this.EndPoint.U);
            PatternPoint ydiff = new PatternPoint(this.StartPoint.V - this.EndPoint.V, this.StartPoint.V - this.EndPoint.V);
            double div = xdiff.U * ydiff.V - xdiff.V * ydiff.U;
            PatternPoint temp;
            if (div == 0)
            {
                temp = null;
            }

            else
            {
                temp = new PatternPoint(StartPoint.U * EndPoint.V - StartPoint.V * EndPoint.U, patternLine.StartPoint.U * patternLine.EndPoint.V - patternLine.StartPoint.V * patternLine.EndPoint.U);
            }

            double int_XPoint = temp.U * xdiff.V - temp.V * xdiff.U / div;
            double int_YPoint = temp.U * ydiff.V - temp.V * ydiff.U / div;

            return new PatternPoint(int_XPoint, int_YPoint);
        }

        public void Rotate(PatternPoint origin, double angle)
        {
            this.StartPoint.Rotate(origin, angle);
            this.EndPoint.Rotate(origin, angle);
        }
    }

    public class PatternSafeGrid
    {
        public PatternPoint Domain;
        public bool Flipped;
        public double DiagonalAngle;
        public PatternLine AxisLine;
        public double OffsetDirection;
        public double Angle;
        public double UTiles;
        public double VTiles;
        public double DomainU;
        public double DomainV;

        public PatternSafeGrid(PatternPoint domain, double diag_angle, double u_tiles, double v_tiles, bool flipped = false)
        {
            this.Domain = domain;
            this.Flipped = flipped;
            this.DiagonalAngle = diag_angle;

            // find out the axis line to calculate angle and length
            this.AxisLine = new PatternLine(new PatternPoint(0, 0), new PatternPoint(this.Domain.U * u_tiles, this.Domain.V * v_tiles));

            // now determine the parameters necessary to calculate span, offset, and shift
            this.DetermineAbstractParams(u_tiles, v_tiles);
        }

        public void DetermineAbstractParams(double u_tiles, double v_tiles)
        {
            if (this.AxisLine.Angle <= this.DiagonalAngle)
            {
                if (!this.Flipped)
                {
                    this.OffsetDirection = -1.0;
                }
                else
                {
                    this.OffsetDirection = 1.0;
                }

                this.Angle = this.AxisLine.Angle;
                this.UTiles = u_tiles;
                this.VTiles = v_tiles;
                this.DomainU = this.Domain.U;
                this.DomainV = this.Domain.V;
            }
            else
            {
                if (!this.Flipped)
                {
                    this.OffsetDirection = 1.0;
                    this.Angle = (Math.PI / 2) - this.AxisLine.Angle;
                }
                else
                {
                    this.OffsetDirection = -1.0;
                    this.Angle = this.AxisLine.Angle - (Math.PI / 2);
                }
                this.UTiles = v_tiles;
                this.VTiles = u_tiles;
                this.DomainU = this.Domain.V;
                this.DomainV = this.Domain.U;

            }
        }

        public double GridAngle
        {
            get
            {
                if (!this.Flipped)
                {
                    return this.AxisLine.Angle;
                }
                else
                {
                    return Math.PI - this.AxisLine.Angle;
                }
            }
        }

        public double Span
        {
            get { return this.AxisLine.Length; }
        }

        public double Offset
        {
            get
            {
                if (this.Angle == 0.0)
                {
                    return this.DomainV * this.OffsetDirection;
                }
                else
                {
                    //TaskDialog.Show("test","pattern safe grid offset" + (Math.Abs(this.DomainU * Math.Sin(this.Angle) / this.VTiles) * this.OffsetDirection).ToString());
                    return Math.Abs(this.DomainU * Math.Sin(this.Angle) / this.VTiles) * this.OffsetDirection;
                }
            }
        }

        public double? Shift
        {
            get
            {
                if (this.Angle == 0.0)
                {
                    return 0.0;
                }
                if (this.UTiles == this.VTiles & this.UTiles == 1)
                {
                    return Math.Abs(this.DomainU * Math.Cos(this.Angle));
                }
                else
                {
                    // calculate the abstract offset axis
                    double offsetU = Math.Abs(this.Offset * Math.Sin(this.Angle));
                    double offsetV = -Math.Abs(this.Offset * Math.Cos(this.Angle));
                    PatternPoint OffsetVector = new PatternPoint(offsetU, offsetV);
                    //find the offset line

                    PatternPoint abstractAxisStartPoint = new PatternPoint(0, 0);
                    PatternPoint abstractAxisEndPoint = new PatternPoint(this.DomainU * this.UTiles, this.DomainV * this.VTiles);
                    PatternPoint offsetVectorStart = abstractAxisStartPoint.Add(OffsetVector);
                    PatternPoint offsetVectorEnd = abstractAxisEndPoint.Add(OffsetVector);

                    PatternLine offsetAxis = new PatternLine(offsetVectorStart, offsetVectorEnd);

                    //try to find the next occurrence on the abstract offset axis
                    PatternPoint nextGridPoint = FindNextGridPoint(offsetAxis);

                    if (nextGridPoint != null)
                    {
                        return offsetAxis.StartPoint.DistanceTo(nextGridPoint);
                    }
                    else
                    {
                        return null;
                    }

                }
            }
        }

        public bool IsValid()
        {
            if (this.Shift == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public PatternPoint FindNextGridPoint(PatternLine offsetLine)
        {
            double uMult = 0.0;
            PatternPoint gridPoint = null;
            while (uMult < this.UTiles)
            {
                foreach (int vMult in Enumerable.Range(0, (int)this.VTiles))
                {
                    gridPoint = new PatternPoint(this.DomainU * uMult, this.DomainV * vMult);
                    if (offsetLine.PointOnLine(gridPoint))
                    {
                        return gridPoint;
                    }
                }
                uMult += 1;
            }
            if (uMult >= this.UTiles)
            {
                return gridPoint;
            }
            return gridPoint;
        }

    }

    public class PatternDomain
    {
        public PatternPoint Origin;
        public PatternPoint Corner;
        public PatternPoint Bounds;
        public PatternPoint NormalisedDomain;
        public PatternLine Uvec;
        public PatternLine Vvec;
        public bool Expandable;
        public double MaxDomain;
        public double TargetDomain;
        public PatternLine Diagonal;

        public List<PatternSafeGrid> safeAngles;

        // 0.5 < MODEL < 848.5 inches, source: http://hatchkit.com.au/faq.php#Tip7
        public const double MAX_MODEL_DOMAIN = 100.0;

        // 0.002 < DRAFTING < 84.85 inches
        public const double MAX_DETAIL_DOMAIN = MAX_MODEL_DOMAIN / 10.0;
        //public const double MAX_DETAIL_DOMAIN = 10;

        public const double MAX_DOMAIN_MULT = 8;

        public const int RATIO_RESOLUTION = 4;
        public const double ANGLE_CORR_RATIO = 0.01;

        public PatternDomain(double StartU, double StartV, double EndU, double EndV, bool ModelPattern, bool Expandable)
        {

            this.Origin = new PatternPoint(Math.Min(StartU, EndU), Math.Min(StartV, EndV));
            this.Corner = new PatternPoint(Math.Max(StartU, EndU), Math.Max(StartV, EndV));
            this.Bounds = this.Corner.Sub(this.Origin);
            this.NormalisedDomain = new PatternPoint(1.0, 1.0 * (this.Bounds.V / this.Bounds.U));
            this.Uvec = new PatternLine(new PatternPoint(0, 0), new PatternPoint(this.Bounds.U, 0));
            this.Vvec = new PatternLine(new PatternPoint(0, 0), new PatternPoint(0, this.Bounds.V));
            this.Expandable = Expandable;

            if (ModelPattern)
            {
                this.MaxDomain = MAX_MODEL_DOMAIN;
            }
            else
            {
                this.MaxDomain = MAX_DETAIL_DOMAIN;
            }

            this.TargetDomain = this.MaxDomain;
            this.Diagonal = new PatternLine(new PatternPoint(0, 0), new PatternPoint(this.Bounds.U, this.Bounds.V));

            this.CalculateSafeAngles();
        }
        public void CalculateSafeAngles()
        {
            // setup tile counters
            int uMult = 1;
            int vMult = 1;

            this.safeAngles = new List<PatternSafeGrid>();
            List<double> processedRatios = new List<double> { 1.0 };

            // add standard angles to the list
            this.safeAngles.Add(new PatternSafeGrid(this.Bounds, this.Diagonal.Angle, uMult, 0));
            this.safeAngles.Add(new PatternSafeGrid(this.Bounds, this.Diagonal.Angle, uMult, 0, true));
            this.safeAngles.Add(new PatternSafeGrid(this.Bounds, this.Diagonal.Angle, uMult, vMult));
            this.safeAngles.Add(new PatternSafeGrid(this.Bounds, this.Diagonal.Angle, uMult, vMult, true));
            this.safeAngles.Add(new PatternSafeGrid(this.Bounds, this.Diagonal.Angle, 0, vMult));

            while ((this.Bounds.U * uMult) <= (this.TargetDomain / 2.0))
            {
                vMult = 1;
                while ((this.Bounds.V * vMult) <= (this.TargetDomain / 2.0))
                {
                    double ratio = Math.Round(vMult / (float)uMult, RATIO_RESOLUTION);
                    if (!processedRatios.Contains(ratio))
                    {
                        // for every tile, also add the mirrored tile
                        PatternSafeGrid angle1 = new PatternSafeGrid(this.Bounds, this.Diagonal.Angle, uMult, vMult);
                        PatternSafeGrid angle2 = new PatternSafeGrid(this.Bounds, this.Diagonal.Angle, uMult, vMult, true);

                        double? shang1 = angle1.Shift;
                        double? shang2 = angle2.Shift;

                        if (angle1.IsValid() & angle2.IsValid())
                        {
                            this.safeAngles.Add(angle1);
                            this.safeAngles.Add(angle2);
                            processedRatios.Add(ratio);

                            //Debug.Print(this.safeAngles.Count.ToString());
                        }
                        else
                        {
                            // giving warning to user not implemented
                        }
                    }
                    vMult += 1;
                }
                uMult += 1;
            }

        }

        public bool Expand()
        {
            // expand target domain for more safe angles
            if (this.TargetDomain > this.MaxDomain * MAX_DOMAIN_MULT)
            {
                return false;
            }
            else
            {
                this.TargetDomain += (this.MaxDomain / 2);
                this.CalculateSafeAngles();
                return true;
            }
        }

        public PatternLine GetDomainCoordinates(PatternLine patLine)
        {

            //TaskDialog.Show("test", "U:" + this.Origin.U.ToString() + " V:" + this.Origin.V.ToString());
            return new PatternLine(patLine.StartPoint.Sub(this.Origin), patLine.EndPoint.Sub(this.Origin));
        }
        public PatternSafeGrid GetGridParams(double axisAngle)
        {
            PatternSafeGrid test = this.safeAngles.OrderByDescending(x => Math.Abs(x.GridAngle - axisAngle)).ToList().Last();
            return this.safeAngles.OrderByDescending(x => Math.Abs(x.GridAngle - axisAngle)).ToList().Last();
        }

        public double GetRequiredCorrection(double axisAngle)
        {
            double test = Math.Abs(axisAngle - this.GetGridParams(axisAngle).GridAngle);
            return Math.Abs(axisAngle - this.GetGridParams(axisAngle).GridAngle);
        }

        public PatternSafeGrid GetBestAngle(double axisAngle)
        {
            if (this.Expandable)
            {
                while (this.GetRequiredCorrection(axisAngle) >= ANGLE_CORR_RATIO)
                {
                    if (!this.Expand())
                    {
                        break;
                    }
                }
            }

            PatternSafeGrid test = this.GetGridParams(axisAngle);
            return this.GetGridParams(axisAngle);
        }
    }

    public class PatternGrid
    {
        public PatternDomain Domain;
        public PatternSafeGrid Grid;
        public double Angle;
        public double Span;
        public double Offset;
        public double? Shift;
        public List<PatternLine> SegmentLines;

        public PatternGrid(PatternDomain domain, PatternLine initLine)
        {
            this.Domain = domain;
            this.Grid = this.Domain.GetBestAngle(initLine.Angle);

            //TaskDialog.Show("test", "closest safe angle is: GA " + this.Grid.GridAngle.ToString() + " A: " + this.Grid.Angle.ToString() + " U:" + this.Grid.UTiles.ToString() + " V:" + this.Grid.VTiles.ToString() + " DU:" + this.Grid.DomainU + " DV:" + this.Grid.DomainV + " OD:" + this.Grid.OffsetDirection + " SP:" + this.Grid.Span.ToString() + " OF:" + this.Grid.Offset.ToString() + " SH:" + this.Grid.Shift.ToString());

            this.Angle = this.Grid.GridAngle;
            this.Span = this.Grid.Span;
            this.Offset = this.Grid.Offset;
            this.Shift = this.Grid.Shift;
            this.SegmentLines = new List<PatternLine>();

            initLine.Rotate(initLine.CentrePoint, this.Angle - initLine.Angle);
            this.SegmentLines.Add(initLine);

        }

        public PatternPoint Origin()
        {
            // collect all line segment points
            List<PatternPoint> pointList = new List<PatternPoint>();
            foreach (PatternLine segLine in this.SegmentLines)
            {
                pointList.Add(segLine.StartPoint);
                pointList.Add(segLine.EndPoint);
            }
            // origin is the point that is closest to zero
            if (this.Angle <= Math.PI / 2)
            {
                // debug
                //PatternPoint p = pointList.OrderByDescending(x => x.DistanceTo(new PatternPoint(0, 0))).ToList().Last();
                //Debug.Print("origin U: " + p.U.ToString() + " V: " + p.V.ToString());

                return pointList.OrderByDescending(x => x.DistanceTo(new PatternPoint(0, 0))).ToList().Last();
            }
            else
            {
                // debug
                //PatternPoint p = pointList.OrderByDescending(x => x.DistanceTo(new PatternPoint(this.Domain.Uvec.Length, 0))).ToList().Last();

                return pointList.OrderByDescending(x => x.DistanceTo(new PatternPoint(this.Domain.Uvec.Length, 0))).ToList().Last();
            }
        }

        public double[] Segments()
        {
            double penDown = this.SegmentLines[0].Length;
            return new double[] { penDown, this.Span - penDown };
        }

        public List<PatternLine> SegmentAsLines
        {
            get { return this.SegmentLines; }
        }
    }
}
