﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SlabOnGradient.Models.Filters;

namespace SlabOnGradient.Models
{
    internal class RevitGeometryUtils
    {
        public static List<Curve> GetCurvesByRectangle(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var selectedElements = sel.PickElementsByRectangle(new DirectShapeClassFilter(), "Select Road Axis");
            var directshapeRoadAxis = selectedElements.OfType<DirectShape>();
            elementIds = ElementIdToString(directshapeRoadAxis);
            var curvesRoadAxis = GetCurvesByDirectShapes(directshapeRoadAxis);

            return curvesRoadAxis;
        }

        // Метод получения списка линий на поверхности дороги
        public static List<Line> GetRoadLines(UIApplication uiapp, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var selectedOnRoadSurface = sel.PickObjects(ObjectType.Element, new DirectShapeClassFilter(), "Select Road Lines");
            var directShapesRoadSurface = selectedOnRoadSurface.Select(r => uiapp.ActiveUIDocument.Document.GetElement(r))
                                                               .OfType<DirectShape>();
            elementIds = ElementIdToString(directShapesRoadSurface);
            var curvesRoadSurface = GetCurvesByDirectShapes(directShapesRoadSurface);
            var linesRoadSurface = curvesRoadSurface.OfType<Line>().ToList();

            return linesRoadSurface;
        }

        // Получение линий границ плиты
        public static List<Curve> GetBoundCurves(UIApplication uiapp, out string elementIds)
        {
            Document doc = uiapp.ActiveUIDocument.Document;
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var boundCurveReferences = sel.PickObjects(ObjectType.Element, new ModelCurveClassFilter(), "Выберете линии границы плиты");
            Options options = new Options();
            var boundCurvesELements = boundCurveReferences.Select(r => doc.GetElement(r));
            elementIds = ElementIdToString(boundCurvesELements);
            var boundCurves = boundCurvesELements.OfType<ModelCurve>()
                                                 .Select(ml => ml.GeometryCurve)
                                                 .ToList();

            return boundCurves;
        }

        // Получение линии из списка, которая пересекается с плоскостью
        public static Line GetIntersectCurve(IEnumerable<Line> lines, Plane plane)
        {
            XYZ originPlane = plane.Origin;
            XYZ directionLine = plane.XVec;

            var lineByPlane = Line.CreateUnbound(originPlane, directionLine);

            foreach (var line in lines)
            {
                XYZ startPoint = line.GetEndPoint(0);
                XYZ finishPoint = line.GetEndPoint(1);

                XYZ startPointOnBase = new XYZ(startPoint.X, startPoint.Y, 0);
                XYZ finishPointOnBase = new XYZ(finishPoint.X, finishPoint.Y, 0);

                var baseLine = Line.CreateBound(startPointOnBase, finishPointOnBase);

                var result = new IntersectionResultArray();
                var compResult = lineByPlane.Intersect(baseLine, out result);
                if (compResult == SetComparisonResult.Overlap)
                {
                    return line;
                }
            }

            return null;
        }

        /* Пересечение линии и плоскости
         * (преобразует линию в вектор, поэтому пересекает любую линию не параллельную плоскости)
         */
        public static XYZ LinePlaneIntersection(Line line, Plane plane, out double lineParameter)
        {
            XYZ planePoint = plane.Origin;
            XYZ planeNormal = plane.Normal;
            XYZ linePoint = line.GetEndPoint(0);

            XYZ lineDirection = (line.GetEndPoint(1) - linePoint).Normalize();

            // Проверка на параллельность линии и плоскости
            if ((planeNormal.DotProduct(lineDirection)) == 0)
            {
                lineParameter = double.NaN;
                return null;
            }

            lineParameter = (planeNormal.DotProduct(planePoint)
              - planeNormal.DotProduct(linePoint))
                / planeNormal.DotProduct(lineDirection);

            return linePoint + lineParameter * lineDirection;
        }


        // Получение id элементов на основе списка в виде строки
        public static List<int> GetIdsByString(string elems)
        {
            if (string.IsNullOrEmpty(elems))
            {
                return null;
            }

            var elemIds = elems.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => int.Parse(s.Remove(0, 2)))
                         .ToList();

            return elemIds;
        }

        // Проверка на то существуют ли элементы с данным Id в модели
        public static bool IsElemsExistInModel(Document doc, IEnumerable<int> elems, Type type)
        {
            if (elems is null)
            {
                return false;
            }

            foreach (var elem in elems)
            {
                ElementId id = new ElementId(elem);
                Element curElem = doc.GetElement(id);
                if (curElem is null || !(curElem.GetType().IsSubclassOf(type) || curElem.GetType() == type))
                {
                    return false;
                }
            }

            return true;
        }

        // Получение линий по их id
        public static List<Curve> GetCurvesById(Document doc, IEnumerable<int> ids)
        {
            var directShapeLines = new List<DirectShape>();
            foreach (var id in ids)
            {
                ElementId elemId = new ElementId(id);
                DirectShape line = doc.GetElement(elemId) as DirectShape;
                directShapeLines.Add(line);
            }

            var lines = GetCurvesByDirectShapes(directShapeLines).OfType<Curve>().ToList();

            return lines;
        }

        // Получение линии границ плиты по Id
        public static List<Curve> GetBoundCurvesById(Document doc, string elemIdInSettings)
        {
            var elemId = GetIdsByString(elemIdInSettings);
            var modelCurvesId = elemId.Select(id => new ElementId(id));
            var modelCurveElems = modelCurvesId.Select(id => doc.GetElement(id)).OfType<ModelCurve>();
            var curves = modelCurveElems.Select(c => c.GeometryCurve).ToList();

            return curves;
        }

        // Получение точек на линиях границ плиты
        public static List<List<XYZ>> GetPointsOnBoundCurves(IEnumerable<Curve> curves, double step)
        {
            double interUnitsStep = UnitUtils.ConvertToInternalUnits(step, UnitTypeId.Meters);
            var pointsOnCurves = new List<List<XYZ>>();

            foreach (var curve in curves)
            {
                double length = curve.Length;
                int countPoints = (int)(length / interUnitsStep);
                var normParameters = GenerateNormalizeParameters(countPoints);
                var points = normParameters.Select(p => curve.Evaluate(p, true)).ToList();
                pointsOnCurves.Add(points);
            }

            return pointsOnCurves;
        }

        // Получение линий на основе элементов DirectShape
        private static List<Curve> GetCurvesByDirectShapes(IEnumerable<DirectShape> directShapes)
        {
            var curves = new List<Curve>();

            Options options = new Options();
            var geometries = directShapes.Select(d => d.get_Geometry(options)).SelectMany(g => g);

            foreach (var geom in geometries)
            {
                if (geom is PolyLine polyLine)
                {
                    var polyCurve = GetCurvesByPolyline(polyLine);
                    curves.AddRange(polyCurve);
                }
                else
                {
                    curves.Add(geom as Curve);
                }
            }

            return curves;
        }

        // Метод получения списка линий на основе полилинии
        private static IEnumerable<Curve> GetCurvesByPolyline(PolyLine polyLine)
        {
            var curves = new List<Curve>();

            for (int i = 0; i < polyLine.NumberOfCoordinates - 1; i++)
            {
                var line = Line.CreateBound(polyLine.GetCoordinate(i), polyLine.GetCoordinate(i + 1));
                curves.Add(line);
            }

            return curves;
        }

        // Метод получения строки с ElementId
        private static string ElementIdToString(IEnumerable<Element> elements)
        {
            var stringArr = elements.Select(e => "Id" + e.Id.IntegerValue.ToString()).ToArray();
            string resultString = string.Join(", ", stringArr);

            return resultString;
        }

        // Генератор нормализованных пораметров точек на линии
        private static List<double> GenerateNormalizeParameters(int count)
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
