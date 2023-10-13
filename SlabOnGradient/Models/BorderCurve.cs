using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SlabOnGradient.Models
{
    public class BorderCurve
    {
        public Curve PlaneCurve { get; set; }
        public List<XYZ> PlanePoints { get; set; }

        public BorderCurve(Curve planeCurve, double pointStep)
        {
            PlaneCurve = planeCurve;

            int pointCount = (int)(PlaneCurve.Length / pointStep);
            var normalizeParameters = GenerateNormalizeParameters(pointCount);
            PlanePoints = normalizeParameters.Select(p => PlaneCurve.Evaluate(p, true)).ToList();
        }

        public CurveByPoints CreateSlabCurveByPoints(PolyCurve roadAxis,
                                                    IEnumerable<Line> roadLine1,
                                                    IEnumerable<Line> roadLine2,
                                                    double coverageThikness, Document doc)
        {
            var points = ProjectPointsOnSlabSurface(roadAxis, roadLine1, roadLine2, coverageThikness);
            var referencePointsArray = new ReferencePointArray();
            foreach (var point in points)
            {
                referencePointsArray.Append(doc.FamilyCreate.NewReferencePoint(point));
            }

            CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(referencePointsArray);

            return curveByPoints;
        }

        public CurveByPoints CreateRaisedCurve(Document doc)
        {
            double heightUnderBasePlane = UnitUtils.ConvertToInternalUnits(2, UnitTypeId.Meters);
            XYZ upVector = XYZ.BasisZ * heightUnderBasePlane;
            var raisedPoints = PlanePoints.Select(p => p + upVector);
            var referencePointsArray = new ReferencePointArray();
            foreach (var point in raisedPoints)
            {
                referencePointsArray.Append(doc.FamilyCreate.NewReferencePoint(point));
            }

            CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(referencePointsArray);

            return curveByPoints;
        }

        private List<XYZ> ProjectPointsOnSlabSurface(PolyCurve roadAxis,
                                                    IEnumerable<Line> roadLine1,
                                                    IEnumerable<Line> roadLine2,
                                                    double coverageThikness)
        {
            var pointsOnSlab = new List<XYZ>();
            coverageThikness = UnitUtils.ConvertToInternalUnits(coverageThikness, UnitTypeId.Millimeters);

            foreach (var planePoint in PlanePoints)
            {
                Plane normalPlane = roadAxis.GetNormalPlane(planePoint);
                Line lineOnRoad1 = RevitGeometryUtils.GetIntersectCurve(roadLine1, normalPlane);
                Line lineOnRoad2 = RevitGeometryUtils.GetIntersectCurve(roadLine2, normalPlane);

                if (lineOnRoad1 is null || lineOnRoad2 is null)
                {
                    continue;
                }

                XYZ point1 = RevitGeometryUtils.LinePlaneIntersection(lineOnRoad1, normalPlane, out _);
                XYZ point2 = RevitGeometryUtils.LinePlaneIntersection(lineOnRoad2, normalPlane, out _);

                XYZ projectLineDirection = point1 - point2;
                Line projectionLine = Line.CreateUnbound(point1, projectLineDirection);
                Line verticalLine = Line.CreateUnbound(planePoint, XYZ.BasisZ);

                IntersectionResultArray interResult;
                var compResult = projectionLine.Intersect(verticalLine, out interResult);
                XYZ thiknessVector = projectLineDirection.CrossProduct(normalPlane.Normal).Normalize();
                if (thiknessVector.Z > 0)
                {
                    thiknessVector = thiknessVector.Negate();
                }
                XYZ projectPointOnCoverage = null;
                if (compResult == SetComparisonResult.Overlap)
                {
                    foreach (var elem in interResult)
                    {
                        if (elem is IntersectionResult result)
                        {
                            projectPointOnCoverage = result.XYZPoint;
                            XYZ resultPoint = projectPointOnCoverage + thiknessVector * coverageThikness;
                            var projectResult = verticalLine.Project(resultPoint);

                            pointsOnSlab.Add(projectResult.XYZPoint);
                        }
                    }
                }
            }

            return pointsOnSlab;
        }

        // Генератор нормализованных пораметров точек на линии
        private List<double> GenerateNormalizeParameters(int count)
        {
            var parameters = new List<double>(count) { 0, 1 };

            switch (count)
            {
                case 0:
                    return new List<double>() { 0, 1 };
                case 1:
                    return new List<double>() { 0, 1 };
                default:
                    double step = (double)1 / (count - 2);
                    if (count % 2 == 0)
                    {
                        for (double d = 0.5 - step / 2; d > 0; d -= step)
                        {
                            parameters.Add(d);
                        }
                        for (double d = 0.5 + step / 2; d < 1; d += step)
                        {
                            parameters.Add(d);
                        }
                    }
                    else
                    {
                        for (double d = 0.5 - step; d > 0; d -= step)
                        {
                            parameters.Add(d);
                        }
                        for (double d = 0.5; d < 1; d += step)
                        {
                            parameters.Add(d);
                        }
                    }
                    break;
            }

            return parameters.OrderBy(p => p).ToList();
        }
    }
}
