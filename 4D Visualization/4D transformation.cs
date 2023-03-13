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
  private void RunScript(List<Point3d> pts_3d, Interval w_range, int w_count, double angle_xw, double angle_yw, double angle_zw, double angle_xy, double angle_xz, double angle_yz, ref object Pts_3d_w0, ref object Pts_3d_wext, ref object W_ext, ref object Pts_new)
  {
    
    // define variables
    List<Point3d> pts_3d_w0 = new List<Point3d>();
    List<Point3d> pts_3d_wext = new List<Point3d>();
    List<Point4d> pts_4d = new List<Point4d>();
    List<Point4d> pts_4d_rot = new List<Point4d>();
    List<double> pts_4d_rw = new List<double>();
    List<double> w_divide = new List<double>();
    List<double> w_ext = new List<double>();
    List<Point3d> pts_3d_new = new List<Point3d>();
    double dist = w_range.Length / (w_count - 1);
    // get the divided points between the w range
    w_divide = int_divide(w_range, w_count);

    //transform the existing 3d points into 4d points and extrude in the w dimension
    for( int i = 0;i < pts_3d.Count;i++)
    {
      double x_w0 = pts_3d[i].X;
      double y_w0 = pts_3d[i].Y;
      double z_w0 = pts_3d[i].Z;
      for (int j = 0; j < w_divide.Count;j++)
      {
        double w_j = w_divide[j];
        Point4d pt_4d = new Point4d(x_w0, y_w0, z_w0, w_j);
        pts_4d.Add(pt_4d);
      }
    }

    // rotation the 4d points
    for(int i = 0; i < pts_4d.Count;i++)
    {
      Matrix m_xy ;
      Matrix m_xz ;
      Matrix m_yz ;
      Matrix m_xw ;
      Matrix m_yw ;
      Matrix m_zw ;
      m_xy = rotate_xy(pts_4d[i], angle_xy);
      Point4d rot_xy = matrix_to_4d(m_xy);

      m_xz = rotate_xz(rot_xy, angle_xz);
      Point4d rot_xz = matrix_to_4d(m_xz);

      m_yz = rotate_yz(rot_xz, angle_yz);
      Point4d rot_yz = matrix_to_4d(m_yz);

      m_xw = rotate_xw(rot_yz, angle_xw);
      Point4d rot_xw = matrix_to_4d(m_xw);

      m_yw = rotate_xw(rot_xw, angle_yw);
      Point4d rot_yw = matrix_to_4d(m_yw);

      m_zw = rotate_zw(rot_yw, angle_zw);
      Point4d rot_zw = matrix_to_4d(m_zw);





      pts_4d_rot.Add(rot_zw);


      //filter the 4d points at w0 hyper plane
      if(rot_zw.W > (-dist / 2  ) && rot_zw.W < (dist / 2) )
      {
        pts_4d_rw.Add(pts_4d[i].W);
        Point3d pt_new = new Point3d(rot_zw.X, rot_zw.Y, rot_zw.Z);
        pts_3d_new.Add(pt_new);
        if(pts_4d[i].W > (-dist / 2) && pts_4d[i].W < (dist / 2))
        {
          Point3d pt_w0 = new Point3d(rot_zw.X, rot_zw.Y, rot_zw.Z);
          pts_3d_w0.Add(pt_w0);
        }
        else
        {
          Point3d pt_wext = new Point3d(rot_zw.X, rot_zw.Y, rot_zw.Z);
          pts_3d_wext.Add(pt_wext);
          w_ext.Add(pts_4d[i].W);
        }

      }
    }

    Pts_3d_w0 = pts_3d_w0;
    Pts_3d_wext = pts_3d_wext;
    Pts_new = pts_3d_new;
    W_ext = w_ext;





  }

  // <Custom additional code> 
    // create a 4d point class
  public class Point4d
  {
    public double X;
    public double Y;
    public double Z;
    public double W;

    public Point4d(double x, double y, double z, double w)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
      this.W = w;
    }

    public Matrix to_matrix()
    {
      Matrix m = new Matrix(4, 1);
      m[0, 0] = this.X;
      m[1, 0] = this.Y;
      m[2, 0] = this.Z;
      m[3, 0] = this.W;
      return m;
    }
  }
  // convert a 1X4 matrix to point4d
  public Point4d matrix_to_4d(Matrix m)
  {
    double x = m[0, 0];
    double y = m[1, 0];
    double z = m[2, 0];
    double w = m[3, 0];
    Point4d pt = new Point4d(x, y, z, w);
    return pt;
  }

  // rotation functions in xw,yw,zw
  public Matrix rotate_xw(Point4d pt, double angle)
  {
    Matrix n = new Matrix(4, 4);
    double[,] m = {{Math.Cos(angle),0,0,-Math.Sin(angle)},{0,1,0,0},{0,0,1,0},
      {Math.Sin(angle),0,0,Math.Cos(angle)}};
    for (int i = 0;i < 4;i++){
      for (int j = 0;j < 4;j++)
      {
        n[i, j] = m[i, j];
      }
    }
    Matrix n2 = pt.to_matrix();
    Matrix rotated = n * n2;

    return rotated;
  }

  public Matrix rotate_yw(Point4d pt, double angle)
  {
    Matrix n = new Matrix(4, 4);
    double[,] m = {{1,0,0,0},{0,Math.Cos(angle),0,-Math.Sin(angle)},{0,0,1,0},
      {0,Math.Sin(angle),0,Math.Cos(angle)}};
    for (int i = 0;i < 4;i++){
      for (int j = 0;j < 4;j++)
      {
        n[i, j] = m[i, j];
      }
    }
    Matrix n2 = pt.to_matrix();
    Matrix rotated = n * n2;

    return rotated;
  }

  public Matrix rotate_zw(Point4d pt, double angle)
  {
    Matrix n = new Matrix(4, 4);
    double[,] m = {{1,0,0,0},{0,1,0,0},{0,0,Math.Cos(angle),-Math.Sin(angle)},
      {0,0,Math.Sin(angle),Math.Cos(angle)}};
    for (int i = 0;i < 4;i++){
      for (int j = 0;j < 4;j++)
      {
        n[i, j] = m[i, j];
      }
    }
    Matrix n2 = pt.to_matrix();
    Matrix rotated = n * n2;

    return rotated;
  }

  public Matrix rotate_xy(Point4d pt, double angle)
  {
    Matrix n = new Matrix(4, 4);
    double[,] m = {{Math.Cos(angle),-Math.Sin(angle),0,0},{Math.Sin(angle),Math.Cos(angle),0,0},{0,0,1,0},
      {0,0,0,1}};
    for (int i = 0;i < 4;i++){
      for (int j = 0;j < 4;j++)
      {
        n[i, j] = m[i, j];
      }
    }
    Matrix n2 = pt.to_matrix();
    Matrix rotated = n * n2;

    return rotated;
  }

  public Matrix rotate_xz(Point4d pt, double angle)
  {
    Matrix n = new Matrix(4, 4);
    double[,] m = {{Math.Cos(angle),0,Math.Sin(angle),0},{0,1,0,0},{-Math.Sin(angle),0,Math.Cos(angle),0},
      {0,0,0,1}};
    for (int i = 0;i < 4;i++){
      for (int j = 0;j < 4;j++)
      {
        n[i, j] = m[i, j];
      }
    }
    Matrix n2 = pt.to_matrix();
    Matrix rotated = n * n2;

    return rotated;
  }

  public Matrix rotate_yz(Point4d pt, double angle)
  {
    Matrix n = new Matrix(4, 4);
    double[,] m = {{1,0,0,0},{0,Math.Cos(angle),-Math.Sin(angle),0},{0,Math.Sin(angle),Math.Cos(angle),0},
      {0,0,0,1}};
    for (int i = 0;i < 4;i++){
      for (int j = 0;j < 4;j++)
      {
        n[i, j] = m[i, j];
      }
    }
    Matrix n2 = pt.to_matrix();
    Matrix rotated = n * n2;

    return rotated;
  }


  // function to divide an interval into given count of numbers
  public List<double> int_divide(Interval interval, int n)
  {
    List<double> nums = new List<double>();
    double d = interval.Length / (n - 1);
    for(int i = 0;i < n;i++){
      double num = interval.Min + i * d;
      nums.Add(num);
    }
    return nums;
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
        List<Point3d> pts_3d = null;
    if (inputs[0] != null)
    {
      pts_3d = GH_DirtyCaster.CastToList<Point3d>(inputs[0]);
    }
    Interval w_range = default(Interval);
    if (inputs[1] != null)
    {
      w_range = (Interval)(inputs[1]);
    }

    int w_count = default(int);
    if (inputs[2] != null)
    {
      w_count = (int)(inputs[2]);
    }

    double angle_xw = default(double);
    if (inputs[3] != null)
    {
      angle_xw = (double)(inputs[3]);
    }

    double angle_yw = default(double);
    if (inputs[4] != null)
    {
      angle_yw = (double)(inputs[4]);
    }

    double angle_zw = default(double);
    if (inputs[5] != null)
    {
      angle_zw = (double)(inputs[5]);
    }

    double angle_xy = default(double);
    if (inputs[6] != null)
    {
      angle_xy = (double)(inputs[6]);
    }

    double angle_xz = default(double);
    if (inputs[7] != null)
    {
      angle_xz = (double)(inputs[7]);
    }

    double angle_yz = default(double);
    if (inputs[8] != null)
    {
      angle_yz = (double)(inputs[8]);
    }



    //3. Declare output parameters
      object Pts_3d_w0 = null;
  object Pts_3d_wext = null;
  object W_ext = null;
  object Pts_new = null;


    //4. Invoke RunScript
    RunScript(pts_3d, w_range, w_count, angle_xw, angle_yw, angle_zw, angle_xy, angle_xz, angle_yz, ref Pts_3d_w0, ref Pts_3d_wext, ref W_ext, ref Pts_new);
      
    try
    {
      //5. Assign output parameters to component...
            if (Pts_3d_w0 != null)
      {
        if (GH_Format.TreatAsCollection(Pts_3d_w0))
        {
          IEnumerable __enum_Pts_3d_w0 = (IEnumerable)(Pts_3d_w0);
          DA.SetDataList(1, __enum_Pts_3d_w0);
        }
        else
        {
          if (Pts_3d_w0 is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(Pts_3d_w0));
          }
          else
          {
            //assign direct
            DA.SetData(1, Pts_3d_w0);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (Pts_3d_wext != null)
      {
        if (GH_Format.TreatAsCollection(Pts_3d_wext))
        {
          IEnumerable __enum_Pts_3d_wext = (IEnumerable)(Pts_3d_wext);
          DA.SetDataList(2, __enum_Pts_3d_wext);
        }
        else
        {
          if (Pts_3d_wext is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(Pts_3d_wext));
          }
          else
          {
            //assign direct
            DA.SetData(2, Pts_3d_wext);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
      }
      if (W_ext != null)
      {
        if (GH_Format.TreatAsCollection(W_ext))
        {
          IEnumerable __enum_W_ext = (IEnumerable)(W_ext);
          DA.SetDataList(3, __enum_W_ext);
        }
        else
        {
          if (W_ext is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(3, (Grasshopper.Kernel.Data.IGH_DataTree)(W_ext));
          }
          else
          {
            //assign direct
            DA.SetData(3, W_ext);
          }
        }
      }
      else
      {
        DA.SetData(3, null);
      }
      if (Pts_new != null)
      {
        if (GH_Format.TreatAsCollection(Pts_new))
        {
          IEnumerable __enum_Pts_new = (IEnumerable)(Pts_new);
          DA.SetDataList(4, __enum_Pts_new);
        }
        else
        {
          if (Pts_new is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(4, (Grasshopper.Kernel.Data.IGH_DataTree)(Pts_new));
          }
          else
          {
            //assign direct
            DA.SetData(4, Pts_new);
          }
        }
      }
      else
      {
        DA.SetData(4, null);
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