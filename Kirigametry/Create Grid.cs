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
  private void RunScript(Line LineL, Line LineS, double dx, double a, ref object Pts, ref object TriMesh, ref object Xcount, ref object Ycount)
  {
        Point3d pt0 = LineL.PointAt(1);
    int x_count = Convert.ToInt32((LineL.Length - LineL.Length % dx) / dx);
    double dy = Math.Sin(Math.Acos(a)) * dx*0.5/ a;
    int y_count = Convert.ToInt32((LineS.Length - LineS.Length % dy) / dy);

    // create triangular grid
    List<Point3d> pts = Tri_grid(x_count, y_count, pt0, dx, dy);

    // create mesh from the points
    Mesh mesh = Tri_mesh(x_count, y_count, pts);





    Pts = pts;
    TriMesh = mesh;
    Xcount = x_count;
    Ycount = y_count;


  }

  // <Custom additional code> 
  
  // created triangular grid
  List<Point3d> Tri_grid (int x_count, int y_count, Point3d pt0, double dx, double dy)
  {
    List<Point3d> pts = new List<Point3d> ();
    int i,j;
    for( j = 0;j < y_count + 1;j++)
    {
      for(i = 0;i < x_count + 1;i++)
      {
        if(j % 2 == 0)
        {
          pts.Add(new Point3d(pt0.X + dx * i, pt0.Y - dy * j, 0));
        }
        else if(i == 0)
        {
          pts.Add(new Point3d(pt0.X + dx * i, pt0.Y - dy * j, 0));
          pts.Add(new Point3d(pt0.X + dx / 2 + dx * i, pt0.Y - dy * j, 0));
        }
        else if(i != x_count)
        {
          pts.Add(new Point3d(pt0.X + dx / 2 + dx * i, pt0.Y - dy * j, 0));
        }
        else
        {
          pts.Add(new Point3d(pt0.X + dx * i, pt0.Y - dy * j, 0));
        }
      }
    }
    return pts;
  }

  Mesh Tri_mesh (int x_count, int y_count, List<Point3d>pts)
  {
    Mesh mesh = new Mesh();
    for(int i = 0; i < pts.Count;i++)
    {
      mesh.Vertices.Add(pts[i]);
    }
    for( int j = 0;j < y_count + 1;j++)
    {
      for(int i = 0;i < x_count + 1;i++)
      {
        if(j % 2 == 0 && j != y_count )
        {
          int k = i + (x_count * 2 + 3) * j / 2;
          if(i != x_count)
          {
            mesh.Faces.AddFace(k, k + x_count + 2, k + 1);
            mesh.Faces.AddFace(k + x_count + 1, k, k + x_count + 2);
          }
          else
          {
            mesh.Faces.AddFace(k + x_count + 1, k, k + x_count + 2);
          }
        }
        else if (j != y_count)
        {
          int k = i + (x_count * 2 + 3) * j / 2;

          if(k != (x_count * 2 + 3) * j / 2)
          {
            mesh.Faces.AddFace(k + x_count + 1, k, k + x_count + 2);
          }
          mesh.Faces.AddFace(k, k + x_count + 2, k + 1);

        }
      }
    }
    return mesh;
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
        Line LineL = default(Line);
    if (inputs[0] != null)
    {
      LineL = (Line)(inputs[0]);
    }

    Line LineS = default(Line);
    if (inputs[1] != null)
    {
      LineS = (Line)(inputs[1]);
    }

    double dx = default(double);
    if (inputs[2] != null)
    {
      dx = (double)(inputs[2]);
    }

    double a = default(double);
    if (inputs[3] != null)
    {
      a = (double)(inputs[3]);
    }



    //3. Declare output parameters
      object Pts = null;
  object TriMesh = null;
  object Xcount = null;
  object Ycount = null;


    //4. Invoke RunScript
    RunScript(LineL, LineS, dx, a, ref Pts, ref TriMesh, ref Xcount, ref Ycount);
      
    try
    {
      //5. Assign output parameters to component...
            if (Pts != null)
      {
        if (GH_Format.TreatAsCollection(Pts))
        {
          IEnumerable __enum_Pts = (IEnumerable)(Pts);
          DA.SetDataList(1, __enum_Pts);
        }
        else
        {
          if (Pts is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(Pts));
          }
          else
          {
            //assign direct
            DA.SetData(1, Pts);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (TriMesh != null)
      {
        if (GH_Format.TreatAsCollection(TriMesh))
        {
          IEnumerable __enum_TriMesh = (IEnumerable)(TriMesh);
          DA.SetDataList(2, __enum_TriMesh);
        }
        else
        {
          if (TriMesh is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(TriMesh));
          }
          else
          {
            //assign direct
            DA.SetData(2, TriMesh);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
      }
      if (Xcount != null)
      {
        if (GH_Format.TreatAsCollection(Xcount))
        {
          IEnumerable __enum_Xcount = (IEnumerable)(Xcount);
          DA.SetDataList(3, __enum_Xcount);
        }
        else
        {
          if (Xcount is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(3, (Grasshopper.Kernel.Data.IGH_DataTree)(Xcount));
          }
          else
          {
            //assign direct
            DA.SetData(3, Xcount);
          }
        }
      }
      else
      {
        DA.SetData(3, null);
      }
      if (Ycount != null)
      {
        if (GH_Format.TreatAsCollection(Ycount))
        {
          IEnumerable __enum_Ycount = (IEnumerable)(Ycount);
          DA.SetDataList(4, __enum_Ycount);
        }
        else
        {
          if (Ycount is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(4, (Grasshopper.Kernel.Data.IGH_DataTree)(Ycount));
          }
          else
          {
            //assign direct
            DA.SetData(4, Ycount);
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