using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlabOnGradient.Models.Filters
{
    internal class ModelCurveClassFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is ModelCurve || elem.GetType().IsSubclassOf(typeof(ModelCurve)))
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
