using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using SlabOnGradient.Models;
using System.IO;

namespace SlabOnGradient
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        #region Ось трассы
        public PolyCurve RoadAxis { get; set; }

        private string _roadAxisElemIds;
        public string RoadAxisElemIds
        {
            get => _roadAxisElemIds;
            set => _roadAxisElemIds = value;
        }

        public void GetPolyCurve()
        {
            var curves = RevitGeometryUtils.GetCurvesByRectangle(Uiapp, out _roadAxisElemIds);
            RoadAxis = new PolyCurve(curves);
        }
        #endregion

        #region Проверка на то существуют линии оси и линии на поверхности в модели
        public bool IsLinesExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(DirectShape));
        }
        #endregion



        #region Получение оси трассы из Settings
        public void GetAxisBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            var lines = RevitGeometryUtils.GetCurvesById(Doc, elemIds);
            RoadAxis = new PolyCurve(lines);
        }
        #endregion

        #region Линия на поверхности 1
        public List<Line> RoadLines1 { get; set; }

        private string _roadLineElemIds1;
        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => _roadLineElemIds1 = value;
        }

        public void GetRoadLine1()
        {
            RoadLines1 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds1);
        }
        #endregion

        #region Получение линии на поверхности 1 из Settings
        public void GetRoadLines1BySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            RoadLines1 = RevitGeometryUtils.GetCurvesById(Doc, elemIds).OfType<Line>().ToList();
        }
        #endregion

        #region Линия на поверхности 2
        public List<Line> RoadLines2 { get; set; }

        private string _roadLineElemIds2;
        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => _roadLineElemIds2 = value;
        }

        public void GetRoadLine2()
        {
            RoadLines2 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds2);
        }
        #endregion

        #region Получение линии на поверхности 2 из Settings
        public void GetRoadLines2BySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            RoadLines2 = RevitGeometryUtils.GetCurvesById(Doc, elemIds).OfType<Line>().ToList();
        }
        #endregion

        public List<Curve> BorderSlabLines { get; set; }

        private string _borderSlabLinesElemIds;
        public string BorderSlabLinesElemIds
        {
            get => _borderSlabLinesElemIds;
            set => _borderSlabLinesElemIds = value;
        }

        public void GetBorderSlabLines()
        {
            BorderSlabLines = RevitGeometryUtils.GetBoundCurves(Uiapp, out _borderSlabLinesElemIds);
        }

        #region Проверка на то существует линия границы плиты
        public bool IsBoundLinesExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(ModelCurve));
        }
        #endregion

        #region Получение границ плиты из Settings
        public void GetBorderSlabBySettings(string elemIdsInSettings)
        {
            BorderSlabLines = RevitGeometryUtils.GetBoundCurvesById(Doc, elemIdsInSettings);
        }
        #endregion
    }
}
