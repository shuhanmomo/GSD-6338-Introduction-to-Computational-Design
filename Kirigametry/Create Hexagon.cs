using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(List<Curve> Cuts, List<Point3d> CutCenter, Mesh TriMesh, int Xcount, int Ycount, double dx, double a, ref object HexBound, ref object B)
  {
        // bounding hexagon
    List<Point3d> pts_bound = new List<Point3d> ();
    List<Box> box_bound = new List<Box> ();
    List<Point3d> hex_cens = new List<Point3d> ();
    List<Point3d> ver_and_cen = add_tricen(TriMesh);
    List<Point3d> hex_bound = new List<Point3d>();
    List<Polyline> hexagon = new List<Polyline> ();
    List<Object> o_test = new List<Object>();
    int ver_coun = TriMesh.Vertices.Count - 1;
    Print(ver_coun.ToString());
    List<Point3d> pts_test = new List<Point3d>();
    double thres_ver = dx * Math.Sqrt(3) / 6;
    hexagon.Clear();


    for (int i = 0;i < Cuts.Count;i++)
    {
      double axis_x = 0;
      hex_bound.Clear();
      // find the center axis
      axis_x = find_axis_x(CutCenter[i], dx, TriMesh);

      // create bounding box
      Box box_b = new Box(Cuts[i].GetBoundingBox(true));
      box_bound.Add(box_b);
      Interval box_i = box_b.X;
      Point3f box_cen = new Point3f((float) box_b.Center.X, (float) box_b.Center.Y, (float) box_b.Center.Z);

      // find the hex center
      Point3d hex_cen = find_hex_cen(Cuts[i], dx, TriMesh, axis_x);
      hex_cens.Add(hex_cen);
      axis_x = hex_cen.X;

      // find vertical boundary point
      Point3d hex_vboun = find_hex_vboun(Cuts[i], axis_x, ver_and_cen, dx, hex_cen);

      // find the upper and bottom boundary point
      // need to calculate boundary condition

      Point3d hex_upboun;
      Point3d hex_btboun;
      find_up_and_bt_boun(Cuts[i], TriMesh, dx, hex_cen, out hex_upboun, out hex_btboun);

      // find the inclined
      Point3d hex_verup;
      Point3d hex_verbt;
      find_ver_up_and_bt(Cuts[i], ver_and_cen, dx, hex_vboun, hex_cen, hex_upboun, hex_btboun, out hex_verup, out hex_verbt);
      
      
      
      hex_bound.Add(hex_upboun);
      hex_bound.Add(hex_verup);
      hex_bound.Add(hex_verbt);
      hex_bound.Add(hex_btboun);
      Point3d hex_l1 = new Point3d(2 * hex_cen.X - hex_verbt.X, hex_verbt.Y, 0);
      Point3d hex_l2 = new Point3d(2 * hex_cen.X - hex_verup.X, hex_verup.Y, 0);
      hex_bound.Add(hex_l1);
      hex_bound.Add(hex_l2);

      Polyline hex_h = new Polyline(hex_bound);
      pts_test.Add(hex_upboun);
      hex_h.Add(hex_upboun);

      hexagon.Add(hex_h);
    }




    HexBound = hexagon;
    B = pts_test;
  }

  // <Custom additional code> 
  
  public Box box_b;
  public Interval box_i;
  public Point3f box_cen;


  public List<Point3d> add_tricen(Mesh TriMesh)
  {
    List<Point3d> pts = new List<Point3d> ();
    for(int i = 0;i < TriMesh.Vertices.Count;i++)
    {
      pts.Add(TriMesh.Vertices[i]);
    }

    for(int i = 0;i < TriMesh.Faces.Count;i++)
    {
      pts.Add(TriMesh.Faces.GetFaceCenter(i));
    }


    return pts;
  }

  // find the axis_x
  public double find_axis_x(Point3d CutCenter, double dx, Mesh TriMesh)
  {
    Point3d pt_cen = CutCenter;
    double x_dist = pt_cen.X - TriMesh.Vertices[0].X;
    int count_axis = Convert.ToInt32((x_dist - x_dist % dx) / dx);
    double axis_x = count_axis * dx + TriMesh.Vertices[0].X;
    return axis_x;
  }

  //create bounding box
  public void find_bounding_box(Curve Cuts)
  {
    box_b = new Box(Cuts.GetBoundingBox(true));
    box_i = box_b.X;
    box_cen = new Point3f((float) box_b.Center.X, (float) box_b.Center.Y, (float) box_b.Center.Z);
  }



  public Point3d find_hex_cen (Curve Cuts, double dx, Mesh TriMesh, double axis_x)
  {
    find_bounding_box(Cuts);

    // find center of hexagon
    double dist_c = dx;
    Point3d hex_cen = TriMesh.Vertices[0];
    for(int j = 0;j < TriMesh.Vertices.Count;j++)
    {
      if(Math.Abs(TriMesh.Vertices[j].X - axis_x) < dx)
      {
        double dist_pts = TriMesh.Vertices[j].DistanceTo(box_cen);
        if(dist_pts < dist_c)
        {
          dist_c = dist_pts;
          hex_cen = TriMesh.Vertices[j];
        }
      }
    }
    return hex_cen;
  }

  public Point3d find_hex_vboun(Curve Cuts, double axis_x, List<Point3d> ver_and_cen, double dx, Point3d hex_cen)
  {
    // find vertical boundary point
    find_bounding_box(Cuts);

    Point3d hex_vboun = new Point3d(0, 0, 0);
    double x_br = box_cen.X + box_i.Length / 2;
    double x_bl = box_cen.X - box_i.Length / 2;
    double dist_r = Math.Abs(x_br - axis_x);
    double dist_l = Math.Abs(x_bl - axis_x);
    double x_b = 0;
    bool left = true;
    if(dist_r < dist_l)
    {
      x_b = x_bl;
    }
    else
    {
      x_b = x_br;
      left = false;
    }
    Print(left.ToString());
    double dist_b = dx;
    for(int j = 0;j < ver_and_cen.Count;j++)
    {
      if(Math.Abs(ver_and_cen[j].Y - hex_cen.Y) < dx)
      {
        if(left)
        {
          if(Math.Abs(ver_and_cen[j].X - x_b) < dist_b && x_b - ver_and_cen[j].X > 0)
          {
            dist_b = Math.Abs(ver_and_cen[j].X - x_b);
            hex_vboun = ver_and_cen[j];
          }
        }
        else if(Math.Abs(ver_and_cen[j].X - x_b) < dist_b && ver_and_cen[j].X - x_b > 0)
        {
          dist_b = Math.Abs(ver_and_cen[j].X - x_b);
          Print(dist_b.ToString());
          hex_vboun = ver_and_cen[j];
        }
      }
    }
    return hex_vboun;
  }

  public void find_up_and_bt_boun(Curve Cuts, Mesh TriMesh, double dx, Point3d hex_cen, out Point3d hex_upboun, out Point3d hex_btboun)
  {

    find_bounding_box(Cuts);
    double y_bu = box_cen.Y + box_b.Y.Length / 2;
    double y_bb = box_cen.Y - box_b.Y.Length / 2;
    hex_upboun = new Point3d(hex_cen.X, TriMesh.Vertices[0].Y, 0);
    int ver_coun = TriMesh.Vertices.Count - 1;
    hex_btboun = new Point3d(hex_cen.X, TriMesh.Vertices[ver_coun].Y, 0);
    double dist_bu = ver_coun * dx;
    double dist_bb = ver_coun * dx;
    // here use default equi triangle
    double thres_ver = dx * Math.Sqrt(3) / 6;
    for(int j = 0;j < TriMesh.Vertices.Count;j++)
    {
      if(Math.Abs(TriMesh.Vertices[j].X - hex_cen.X) < dx / 10)
      {

        if(TriMesh.Vertices[j].Y - y_bu > 0 && (TriMesh.Vertices[j].Y - y_bu < dist_bu))
        {
          dist_bu = TriMesh.Vertices[j].Y - y_bu;
          hex_upboun = TriMesh.Vertices[j];
        }


        if(y_bb - TriMesh.Vertices[j].Y > 0 && ( y_bb - TriMesh.Vertices[j].Y < dist_bb))
        {
          dist_bb = y_bb - TriMesh.Vertices[j].Y;
          hex_btboun = TriMesh.Vertices[j];
        }
      }
    }
  }

  public void find_ver_up_and_bt(Curve Cuts, List<Point3d> ver_and_cen, double dx, Point3d hex_vboun, Point3d hex_cen, Point3d hex_upboun, Point3d hex_btboun, out Point3d hex_verup, out Point3d hex_verbt)
  {
    find_bounding_box(Cuts);
    hex_verup = new Point3d(hex_vboun.X, hex_upboun.Y, 0);
    hex_verbt = new Point3d(hex_vboun.X, hex_btboun.Y, 0);
    double dist_y2 = 0;
    double dist_y1 = 0;
    for(int j = 0;j < ver_and_cen.Count;j++)
    {
      if(Math.Abs(ver_and_cen[j].X - hex_vboun.X) < dx / 10)
      {
        // find the upper vertical bound
        if(ver_and_cen[j].Y < hex_upboun.Y && ver_and_cen[j].Y > box_cen.Y)
        {
          Point3d up1 = ver_and_cen[j];
          Point3d up1_mr = new Point3d(2 * hex_cen.X - up1.X, up1.Y, 0);
          Line l_up1 = new Line(up1, hex_upboun);
          Line l_up2 = new Line(up1_mr, hex_upboun);
          var inters_up1 = Rhino.Geometry.Intersect.Intersection.CurveLine(Cuts, l_up1, 0.001, 0.0);
          var inters_up2 = Rhino.Geometry.Intersect.Intersection.CurveLine(Cuts, l_up2, 0.001, 0.0);

          if(inters_up1.Count < 2 && inters_up2.Count < 2)
          {
            double dist = hex_upboun.Y - up1.Y;
            if(dist > dist_y1)
            {
              dist_y1 = dist;
              hex_verup = up1;
            }
          }
        }
        // find the bottom vertical bound
        if(ver_and_cen[j].Y > hex_btboun.Y && ver_and_cen[j].Y < box_cen.Y)
        {
          Point3d bt1 = ver_and_cen[j];
          Point3d bt1_mr = new Point3d(2 * hex_cen.X - bt1.X, bt1.Y, 0);
          Line l_bt1 = new Line(hex_btboun, bt1);
          Line l_bt2 = new Line(hex_btboun, bt1_mr);
          var inters_bt1 = Rhino.Geometry.Intersect.Intersection.CurveLine(Cuts, l_bt1, 0.001, 0.0);
          var inters_bt2 = Rhino.Geometry.Intersect.Intersection.CurveLine(Cuts, l_bt2, 0.001, 0.0);
          double dist = bt1.Y - hex_btboun.Y;

          if(dist > dist_y2 && inters_bt1.Count < 2 && inters_bt2.Count < 2)
          {
            dist_y2 = dist;
            hex_verbt = bt1;
          }
        }
      }
    }
  }


  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        List<Curve> Cuts = null;
    if (inputs[0] != null)
    {
      Cuts = GH_DirtyCaster.CastToList<Curve>(inputs[0]);
    }
    List<Point3d> CutCenter = null;
    if (inputs[1] != null)
    {
      CutCenter = GH_DirtyCaster.CastToList<Point3d>(inputs[1]);
    }
    Mesh TriMesh = default(Mesh);
    if (inputs[2] != null)
    {
      TriMesh = (Mesh)(inputs[2]);
    }

    int Xcount = default(int);
    if (inputs[3] != null)
    {
      Xcount = (int)(inputs[3]);
    }

    int Ycount = default(int);
    if (inputs[4] != null)
    {
      Ycount = (int)(inputs[4]);
    }

    double dx = default(double);
    if (inputs[5] != null)
    {
      dx = (double)(inputs[5]);
    }

    double a = default(double);
    if (inputs[6] != null)
    {
      a = (double)(inputs[6]);
    }



    //3. Declare output parameters
      object HexBound = null;
  object B = null;


    //4. Invoke RunScript
    RunScript(Cuts, CutCenter, TriMesh, Xcount, Ycount, dx, a, ref HexBound, ref B);
      
    try
    {
      //5. Assign output parameters to component...
            if (HexBound != null)
      {
        if (GH_Format.TreatAsCollection(HexBound))
        {
          IEnumerable __enum_HexBound = (IEnumerable)(HexBound);
          DA.SetDataList(1, __enum_HexBound);
        }
        else
        {
          if (HexBound is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(HexBound));
          }
          else
          {
            //assign direct
            DA.SetData(1, HexBound);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (B != null)
      {
        if (GH_Format.TreatAsCollection(B))
        {
          IEnumerable __enum_B = (IEnumerable)(B);
          DA.SetDataList(2, __enum_B);
        }
        else
        {
          if (B is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(B));
          }
          else
          {
            //assign direct
            DA.SetData(2, B);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}