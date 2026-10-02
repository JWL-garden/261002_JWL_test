using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Structure;
using System.Data;

namespace Modless
{
    public class Util
    {
        public static List<Curve> GetCrvFrFace(Face face)
        {
            List<Curve> curves = new List<Curve>();

            EdgeArrayArray edgeArrays = face.EdgeLoops;

            foreach (EdgeArray edgeArray in edgeArrays)
            {
                foreach (Edge edge in edgeArray)
                {
                    Curve c = edge.AsCurve();
                    curves.Add(c);
                }
            }
            return curves;
        }


        public static FamilySymbol GetFamilySymbolByName(string name, Document doc)
        {

            FilteredElementCollector collecter = new FilteredElementCollector(doc);
            collecter.OfCategory(BuiltInCategory.OST_StructuralFraming);
            collecter.OfClass(typeof(FamilySymbol));

            FamilySymbol fs = null;

            foreach (FamilySymbol item in collecter)
            {
                if (name == item.Name)
                {
                    fs = item;
                    break;
                }

            }
            return fs;
        }

        public static List<Curve> GetCurveListFromPts(List<XYZ> points)
        {
            List<Curve> curves = new List<Curve>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                curves.Add(line);
            }
            return curves;
        }

        public static void CreateFamilyInstanceFromCurve(List<Curve> c, FamilySymbol fs, Level level, Document doc)
        {
            foreach (Curve item in c)
            {
                using (Transaction trans = new Transaction(doc, "보 생성"))
                {
                    trans.Start();
                    fs.Activate();
                    FamilyInstance fi = doc.Create.NewFamilyInstance
                        (item, fs, level, StructuralType.Beam);
                    trans.Commit();

                }
            }
        }

        public static CurveLoop GetCurveLoopFromPTS(List<XYZ> points)
        {
            CurveLoop cl = new CurveLoop();
            for (int i = 0; i < points.Count; i++)
            {
                if (i < points.Count - 1)
                {
                    Line line = Line.CreateBound(points[i], points[i + 1]);
                    cl.Append(line);
                }
                else if (i == points.Count - 1)
                {
                    Line line = Line.CreateBound(points[i], points[0]);
                    cl.Append(line);
                }
            }
            return cl;
        }

        public static void CreateFloor(Document doc, IList<CurveLoop> cl, ElementId floorid, ElementId levelid, double t)
        {
            //Transaction: Revit에서 데이터베이스 변경 작업을 수행할 때 사용. Transaction을 시작하고 Commit 또는 Rollback으로 종료해야 함.
            using (Transaction trans = new Transaction(doc, "바닥을 생성합니다."))
            {
                trans.Start();
                Floor f = Floor.Create(doc, cl, floorid, levelid);
                // 바닥 두께만큼 높이를 올려서 바닥을 생성
                Parameter heightParam = f.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                heightParam.Set(t);
                trans.Commit();
            }
        }

        public static FloorType FindFloorTypeByName(Document doc, string name)
        {
            FloorType ft = null;
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfClass(typeof(FloorType));
            foreach (FloorType floorType in col)
            {
                if (floorType.Name == name)
                {
                    ft = floorType;
                    break;
                }
            }
            return ft;
        }

        public static WallType GetWallTypebyName(Document doc, string name)
        {
            WallType wallType = null;
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfCategory(BuiltInCategory.OST_Walls);
            col.OfClass(typeof(WallType));

            foreach (WallType wt in col)
            {
                if (wt.Name == name)
                {
                    wallType = wt;
                    break;
                }

            }
            return wallType;

        }

        public static void CreatWall(Document doc, Curve c, WallType wt, Level level, double t, bool isSTR)
        {
            using (Transaction trans = new Transaction(doc, "벽 작성하기"))
            {
                trans.Start();

                Wall wall = Wall.Create(doc, c, wt.Id, level.Id, t / 304.8, 0, false, isSTR);

                trans.Commit();
            }

        }

        public static double GetWallTHK(WallType wt)
        {

            Parameter param = wt.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM);
            double t = param.AsDouble();

            return t;
        }

        public static DataTable GetDataTableFromStringArr(List<string> strs)
        {
            DataTable dt = new DataTable();
            foreach (string item in strs)
            {
                dt.Columns.Add(item);
            }
            return dt;
        }

        public static Level GetLevelByName(Document doc, string name)
        {
            Level findlevel = null;
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfClass(typeof(Level));

            foreach (Level item in col)
            {
                if (item.Name == name)
                {
                    findlevel = item;
                    break;
                }
            }
            return findlevel;
        }

        public static FloorType GetFloorTypeByName(Document doc, string name)
        {
            FloorType ft = null;
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfClass(typeof(FloorType));

            foreach (FloorType item in col)
            {
                if (item.Name == name)
                {
                    ft = item;
                    break;
                }
            }
            return ft;
        }

        public static CeilingType GetCeilingTypeByName(Document doc, string name)
        {
            CeilingType ct = null;
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfClass(typeof(CeilingType));

            foreach (CeilingType item in col)
            {
                if (item.Name == name)
                {
                    ct = item;
                    break;
                }
            }
            return ct;
        }

        public static FamilySymbol GetDoorTypeByName(Document doc, string name)
        {
            FamilySymbol fs = null;
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfCategory(BuiltInCategory.OST_Doors);
            col.OfClass(typeof(FamilySymbol));

            foreach (FamilySymbol item in col)
            {
                if (item.Name == name)
                {
                    fs = item;
                    break;
                }
            }
            return fs;
        }


    }
}