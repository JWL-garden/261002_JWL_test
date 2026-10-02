using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Modless;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace Modless
{
    public class FloorATT
    {
        public static List<floor> GetFloorData(Document doc, UIDocument uiDoc, string name)
        {
            List<floor> floors = new List<floor>();

            IList<Reference> refs = uiDoc.Selection.PickObjects(ObjectType.Face, "구조바닥을 선택하세요");

            Element e1 = doc.GetElement(refs[0]);
            Floor levelFloor = e1 as Floor;
            Parameter levelparam = levelFloor.get_Parameter(BuiltInParameter.LEVEL_PARAM);
            ElementId level = levelparam.AsElementId();

            foreach (Reference i in refs)
            {
                floor fclass = new floor();
                fclass.m_level = level;

                Element e = doc.GetElement(i);
                Face f = e.GetGeometryObjectFromReference(i) as Face;
                EdgeArrayArray eaa = f.EdgeLoops;
                IList<CurveLoop> cls = new List<CurveLoop>();

                foreach (EdgeArray ea in eaa)
                {
                    CurveLoop cl = new CurveLoop();
                    foreach (Edge ed in ea)
                    {
                        Curve c = ed.AsCurveFollowingFace(f);
                        cl.Append(c);
                    }
                    cls.Add(cl);
                }
                fclass.m_CurveLoop = cls;
                FloorType ft = Util.FindFloorTypeByName(doc, name);
                if (ft == null)
                {
                    Autodesk.Revit.UI.TaskDialog.Show("경고", "바닥 유형이 없습니다");
                }
                fclass.m_FloorType = ft.Id;

                Parameter param = ft.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM);
                if (param == null)
                {
                    Autodesk.Revit.UI.TaskDialog.Show("경고", "두께 파라미터가 없습니다");
                }
                double t = param.AsDouble();
                fclass.m_FloorTypeTHK = t;

                floors.Add(fclass);

            }



            return floors;
        }

    }

    public class floor
    {
        public ElementId m_level { get; set; }
        public ElementId m_FloorType { get; set; }
        public IList<CurveLoop> m_CurveLoop { get; set; }
        public Double m_FloorTypeTHK { get; set; }
    }
}
