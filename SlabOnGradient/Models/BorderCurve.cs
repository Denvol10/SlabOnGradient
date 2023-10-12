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
