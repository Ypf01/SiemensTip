//
// File generated by HDevelop for HALCON/.NET (C#) Version 20.11.1.0
// Non-ASCII strings in this file are encoded in local-8-bit encoding (cp65001).
// 
// Please note that non-ASCII characters in string constants are exported
// as octal codes in order to guarantee that the strings are correctly
// created on all systems, independent on any compiler settings.
// 
// Source files with different encoding should not be mixed in one project.
//

using HalconDotNet;

public partial class HDevelopExport
{
#if !(NO_EXPORT_MAIN || NO_EXPORT_APP_MAIN)
  public HDevelopExport()
  {
    // Default settings used in HDevelop
    HOperatorSet.SetSystem("width", 512);
    HOperatorSet.SetSystem("height", 512);
    if (HalconAPI.isWindows)
      HOperatorSet.SetSystem("use_window_thread","true");
    action();
  }
#endif

  // Procedures 
  // Local procedures 
  public void show_contours (HObject ho_Image, HObject ho_ModelContour, HObject ho_MeasureContour, 
      HObject ho_ShapeModelContours, HTuple hv_WindowHandle, HTuple hv_Message)
  {




    // Local control variables 

    HTuple hv_Number = new HTuple();
    // Initialize local and output iconic variables 
    if (HDevWindowStack.IsOpen())
    {
      HOperatorSet.ClearWindow(HDevWindowStack.GetActive());
    }
    if (HDevWindowStack.IsOpen())
    {
      HOperatorSet.DispObj(ho_Image, HDevWindowStack.GetActive());
    }
    if (HDevWindowStack.IsOpen())
    {
      HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), 2);
    }
    if (HDevWindowStack.IsOpen())
    {
      HOperatorSet.SetColor(HDevWindowStack.GetActive(), "blue");
    }
    if (HDevWindowStack.IsOpen())
    {
      HOperatorSet.DispObj(ho_ModelContour, HDevWindowStack.GetActive());
    }
    if (HDevWindowStack.IsOpen())
    {
      HOperatorSet.SetLineWidth(HDevWindowStack.GetActive(), 1);
    }
    hv_Number.Dispose();
    HOperatorSet.CountObj(ho_MeasureContour, out hv_Number);
    //Determine if the measure contours should be shown
    if ((int)(new HTuple(hv_Number.TupleGreater(0))) != 0)
    {
      if (HDevWindowStack.IsOpen())
      {
        HOperatorSet.SetColor(HDevWindowStack.GetActive(), "gray");
      }
      if (HDevWindowStack.IsOpen())
      {
        HOperatorSet.DispObj(ho_MeasureContour, HDevWindowStack.GetActive());
      }
    }
    hv_Number.Dispose();
    HOperatorSet.CountObj(ho_ShapeModelContours, out hv_Number);
    if ((int)(new HTuple(hv_Number.TupleGreater(0))) != 0)
    {
      if (HDevWindowStack.IsOpen())
      {
        HOperatorSet.SetColor(HDevWindowStack.GetActive(), "white");
      }
      if (HDevWindowStack.IsOpen())
      {
        HOperatorSet.DispObj(ho_ShapeModelContours, HDevWindowStack.GetActive());
      }
    }

    disp_message(hv_WindowHandle, hv_Message, "window", 12, 12, "black", "true");
    disp_continue_message(hv_WindowHandle, "black", "true");

    hv_Number.Dispose();

    return;
  }

#if !NO_EXPORT_MAIN
  // Main procedure 
  private void action()
  {


    // Local iconic variables 

    HObject ho_template_img, ho_ShapeModelImage;
    HObject ho_ShapeModelRegion, ho_ShapeModel, ho_match_img;
    HObject ho_ObjectXLD=null, ho_ModelContour=null, ho_MeasureContour=null;
    HObject ho_Cross=null, ho_Contour=null, ho_rect=null, ho_match_top=null;
    HObject ho_edges=null, ho_edge=null;

    // Local control variables 

    HTuple hv_ModelID = new HTuple(), hv_RowCheck = new HTuple();
    HTuple hv_ColumnCheck = new HTuple(), hv_AngleCheck = new HTuple();
    HTuple hv_Score = new HTuple(), hv_template_top_line = new HTuple();
    HTuple hv_mat_i = new HTuple(), hv_mat_t = new HTuple();
    HTuple hv_mat_r = new HTuple(), hv_line_start_y = new HTuple();
    HTuple hv_line_start_x = new HTuple(), hv_line_end_y = new HTuple();
    HTuple hv_line_end_x = new HTuple(), hv_obj_idx = new HTuple();
    HTuple hv_MovementOfObject = new HTuple(), hv_obj_line_start_y = new HTuple();
    HTuple hv_obj_line_start_x = new HTuple(), hv_obj_line_end_y = new HTuple();
    HTuple hv_obj_line_end_x = new HTuple(), hv_MetroHandle = new HTuple();
    HTuple hv_top_line = new HTuple(), hv_LineIndices = new HTuple();
    HTuple hv_Row = new HTuple(), hv_Column = new HTuple();
    HTuple hv_Params = new HTuple(), hv_n_edge = new HTuple();
    HTuple hv_v_lx = new HTuple(), hv_v_ly = new HTuple();
    HTuple hv_max_dist = new HTuple(), hv_max_r = new HTuple();
    HTuple hv_max_c = new HTuple(), hv_i = new HTuple(), hv_edge_row = new HTuple();
    HTuple hv_edge_col = new HTuple(), hv_j = new HTuple();
    HTuple hv_dist = new HTuple(), hv_p_x = new HTuple(), hv_p_y = new HTuple();
    HTuple hv_WindowHandle = new HTuple();
    // Initialize local and output iconic variables 
    HOperatorSet.GenEmptyObj(out ho_template_img);
    HOperatorSet.GenEmptyObj(out ho_ShapeModelImage);
    HOperatorSet.GenEmptyObj(out ho_ShapeModelRegion);
    HOperatorSet.GenEmptyObj(out ho_ShapeModel);
    HOperatorSet.GenEmptyObj(out ho_match_img);
    HOperatorSet.GenEmptyObj(out ho_ObjectXLD);
    HOperatorSet.GenEmptyObj(out ho_ModelContour);
    HOperatorSet.GenEmptyObj(out ho_MeasureContour);
    HOperatorSet.GenEmptyObj(out ho_Cross);
    HOperatorSet.GenEmptyObj(out ho_Contour);
    HOperatorSet.GenEmptyObj(out ho_rect);
    HOperatorSet.GenEmptyObj(out ho_match_top);
    HOperatorSet.GenEmptyObj(out ho_edges);
    HOperatorSet.GenEmptyObj(out ho_edge);
    //------------------------------------------------------------------------------------------------
    //This example program uses shape-based matching to align ROIs for the measure
    //tool, which then inspects individual razor blades.
    //The program can be run in two modes: (1) with the full affine transformation
    //                                                               (2) using translate_measure
    //Modify the next line to switch between the modes.

    ho_template_img.Dispose();
    HOperatorSet.ReadImage(out ho_template_img, "E:/BMS_tip_head_defect/0424_template.png");
    //threshold (img, th, 0, 240)
    //paint_region (th, img, ImageResult, 255.0, 'fill')

    hv_ModelID.Dispose();
    HOperatorSet.CreateShapeModel(ho_template_img, 3, -.4, 0.8, 0.008, "none", "use_polarity", 
        30, 25, out hv_ModelID);
    ho_ShapeModelImage.Dispose();ho_ShapeModelRegion.Dispose();
    HOperatorSet.InspectShapeModel(ho_template_img, out ho_ShapeModelImage, out ho_ShapeModelRegion, 
        1, 30);
    ho_ShapeModel.Dispose();
    HOperatorSet.GetShapeModelContours(out ho_ShapeModel, hv_ModelID, 1);
    HOperatorSet.WriteShapeModel(hv_ModelID, "E:/BMS_tip_head_defect/template.hmodel");
    hv_ModelID.Dispose();
    HOperatorSet.ReadShapeModel("E:/BMS_tip_head_defect/template.hmodel", out hv_ModelID);

    hv_RowCheck.Dispose();hv_ColumnCheck.Dispose();hv_AngleCheck.Dispose();hv_Score.Dispose();
    HOperatorSet.FindShapeModel(ho_template_img, hv_ModelID, -0.4, 0.8, 0.9, 0, 0.5, 
        "least_squares", 2, 0.7, out hv_RowCheck, out hv_ColumnCheck, out hv_AngleCheck, 
        out hv_Score);
    hv_template_top_line.Dispose();
    hv_template_top_line = new HTuple();
    hv_template_top_line[0] = 910;
    hv_template_top_line[1] = 260;
    hv_template_top_line[2] = 915;
    hv_template_top_line[3] = 416;
    hv_mat_i.Dispose();
    HOperatorSet.HomMat2dIdentity(out hv_mat_i);
    using (HDevDisposeHelper dh = new HDevDisposeHelper())
    {
    hv_mat_t.Dispose();
    HOperatorSet.HomMat2dTranslate(hv_mat_i, -(hv_RowCheck.TupleSelect(0)), -(hv_ColumnCheck.TupleSelect(
        0)), out hv_mat_t);
    }
    using (HDevDisposeHelper dh = new HDevDisposeHelper())
    {
    hv_mat_r.Dispose();
    HOperatorSet.HomMat2dRotate(hv_mat_t, -(hv_AngleCheck.TupleSelect(0)), 0, 0, 
        out hv_mat_r);
    }
    using (HDevDisposeHelper dh = new HDevDisposeHelper())
    {
    hv_line_start_y.Dispose();hv_line_start_x.Dispose();
    HOperatorSet.AffineTransPoint2d(hv_mat_r, hv_template_top_line.TupleSelect(0), 
        hv_template_top_line.TupleSelect(1), out hv_line_start_y, out hv_line_start_x);
    }
    using (HDevDisposeHelper dh = new HDevDisposeHelper())
    {
    hv_line_end_y.Dispose();hv_line_end_x.Dispose();
    HOperatorSet.AffineTransPoint2d(hv_mat_r, hv_template_top_line.TupleSelect(2), 
        hv_template_top_line.TupleSelect(3), out hv_line_end_y, out hv_line_end_x);
    }

    ho_match_img.Dispose();
    HOperatorSet.ReadImage(out ho_match_img, "E:/BMS_tip_head_defect/0424_test2.bmp");
    hv_RowCheck.Dispose();hv_ColumnCheck.Dispose();hv_AngleCheck.Dispose();hv_Score.Dispose();
    HOperatorSet.FindShapeModel(ho_match_img, hv_ModelID, -0.4, 0.8, 0.3, 0, 0.5, 
        "least_squares", 2, 0.8, out hv_RowCheck, out hv_ColumnCheck, out hv_AngleCheck, 
        out hv_Score);

    for (hv_obj_idx=0; (int)hv_obj_idx<=(int)((new HTuple(hv_Score.TupleLength()))-1); hv_obj_idx = (int)hv_obj_idx + 1)
    {
      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      hv_MovementOfObject.Dispose();
      HOperatorSet.VectorAngleToRigid(0, 0, 0, hv_RowCheck.TupleSelect(hv_obj_idx), 
          hv_ColumnCheck.TupleSelect(hv_obj_idx), hv_AngleCheck.TupleSelect(hv_obj_idx), 
          out hv_MovementOfObject);
      }
      ho_ObjectXLD.Dispose();
      HOperatorSet.AffineTransContourXld(ho_ShapeModel, out ho_ObjectXLD, hv_MovementOfObject);
      hv_obj_line_start_y.Dispose();hv_obj_line_start_x.Dispose();
      HOperatorSet.AffineTransPoint2d(hv_MovementOfObject, hv_line_start_y, hv_line_start_x, 
          out hv_obj_line_start_y, out hv_obj_line_start_x);
      hv_obj_line_end_y.Dispose();hv_obj_line_end_x.Dispose();
      HOperatorSet.AffineTransPoint2d(hv_MovementOfObject, hv_line_end_y, hv_line_end_x, 
          out hv_obj_line_end_y, out hv_obj_line_end_x);

      hv_MetroHandle.Dispose();
      HOperatorSet.CreateMetrologyModel(out hv_MetroHandle);
      hv_top_line.Dispose();
      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      hv_top_line = new HTuple();
      hv_top_line = hv_top_line.TupleConcat(hv_obj_line_start_y, hv_obj_line_start_x, hv_obj_line_end_y, hv_obj_line_end_x);
      }
      hv_LineIndices.Dispose();
      HOperatorSet.AddMetrologyObjectGeneric(hv_MetroHandle, "line", hv_top_line, 
          10, 2, 2, 50, new HTuple(), new HTuple(), out hv_LineIndices);
      //Inspect the shapes that have been added to the metrology model
      HOperatorSet.ApplyMetrologyModel(ho_match_img, hv_MetroHandle);
      ho_ModelContour.Dispose();
      HOperatorSet.GetMetrologyObjectModelContour(out ho_ModelContour, hv_MetroHandle, 
          "all", 1);
      ho_MeasureContour.Dispose();hv_Row.Dispose();hv_Column.Dispose();
      HOperatorSet.GetMetrologyObjectMeasures(out ho_MeasureContour, hv_MetroHandle, 
          "all", "all", out hv_Row, out hv_Column);
      ho_Cross.Dispose();
      HOperatorSet.GenCrossContourXld(out ho_Cross, hv_Row, hv_Column, 5, 0);
      hv_Params.Dispose();
      HOperatorSet.GetMetrologyObjectResult(hv_MetroHandle, "all", "all", "result_type", 
          "all_param", out hv_Params);
      ho_Contour.Dispose();
      HOperatorSet.GetMetrologyObjectResultContour(out ho_Contour, hv_MetroHandle, 
          "all", "all", 1.5);

      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      ho_rect.Dispose();
      HOperatorSet.GenRectangle1(out ho_rect, hv_obj_line_start_y-20, hv_obj_line_start_x, 
          hv_obj_line_end_y+20, hv_obj_line_end_x);
      }
      ho_match_top.Dispose();
      HOperatorSet.ReduceDomain(ho_match_img, ho_rect, out ho_match_top);
      ho_edges.Dispose();
      HOperatorSet.EdgesSubPix(ho_match_top, out ho_edges, "deriche1", 4, 25, 30);
      hv_n_edge.Dispose();
      HOperatorSet.CountObj(ho_edges, out hv_n_edge);

      hv_v_lx.Dispose();
      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      hv_v_lx = (hv_Params.TupleSelect(
          3))-(hv_Params.TupleSelect(1));
      }
      hv_v_ly.Dispose();
      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      hv_v_ly = (hv_Params.TupleSelect(
          2))-(hv_Params.TupleSelect(0));
      }

      hv_max_dist.Dispose();
      hv_max_dist = 0;
      hv_max_r.Dispose();
      hv_max_r = 0;
      hv_max_c.Dispose();
      hv_max_c = 0;
      HTuple end_val56 = hv_n_edge;
      HTuple step_val56 = 1;
      for (hv_i=1; hv_i.Continue(end_val56, step_val56); hv_i = hv_i.TupleAdd(step_val56))
      {
        ho_edge.Dispose();
        HOperatorSet.SelectObj(ho_edges, out ho_edge, hv_i);
        hv_edge_row.Dispose();hv_edge_col.Dispose();
        HOperatorSet.GetContourXld(ho_edge, out hv_edge_row, out hv_edge_col);
        for (hv_j=1; (int)hv_j<=(int)((new HTuple(hv_edge_row.TupleLength()))-1); hv_j = (int)hv_j + 1)
        {
          using (HDevDisposeHelper dh = new HDevDisposeHelper())
          {
          hv_dist.Dispose();
          HOperatorSet.DistancePl(hv_edge_row.TupleSelect(hv_j), hv_edge_col.TupleSelect(
              hv_j), hv_Params.TupleSelect(0), hv_Params.TupleSelect(1), hv_Params.TupleSelect(
              2), hv_Params.TupleSelect(3), out hv_dist);
          }
          hv_p_x.Dispose();
          using (HDevDisposeHelper dh = new HDevDisposeHelper())
          {
          hv_p_x = (hv_edge_col.TupleSelect(
              hv_j))-hv_obj_line_start_x;
          }
          hv_p_y.Dispose();
          using (HDevDisposeHelper dh = new HDevDisposeHelper())
          {
          hv_p_y = (hv_edge_row.TupleSelect(
              hv_j))-hv_obj_line_start_y;
          }
          if ((int)(new HTuple((((hv_v_ly*hv_p_x)-(hv_v_lx*hv_p_y))).TupleLess(0))) != 0)
          {
            //            * if (edge_col[j] < Params[1])
            if ((int)(new HTuple(hv_dist.TupleGreater(hv_max_dist))) != 0)
            {
              hv_max_dist.Dispose();
              hv_max_dist = new HTuple(hv_dist);
              hv_max_r.Dispose();
              using (HDevDisposeHelper dh = new HDevDisposeHelper())
              {
              hv_max_r = hv_edge_row.TupleSelect(
                  hv_j);
              }
              hv_max_c.Dispose();
              using (HDevDisposeHelper dh = new HDevDisposeHelper())
              {
              hv_max_c = hv_edge_col.TupleSelect(
                  hv_j);
              }
            }
          }
        }
      }

      HOperatorSet.SetWindowAttr("background_color","white");
      HOperatorSet.OpenWindow(0,0,3000,3000,0,"visible","",out hv_WindowHandle);
      HDevWindowStack.Push(hv_WindowHandle);
      if (HDevWindowStack.IsOpen())
      {
        HOperatorSet.DispObj(ho_match_img, HDevWindowStack.GetActive());
      }
      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      HOperatorSet.DispLine(hv_WindowHandle, hv_Params.TupleSelect(0), hv_Params.TupleSelect(
          1), hv_Params.TupleSelect(2), hv_Params.TupleSelect(3));
      }
      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      HOperatorSet.DispLine(hv_WindowHandle, hv_max_r, hv_max_c, hv_Params.TupleSelect(
          2), hv_Params.TupleSelect(3));
      }
      using (HDevDisposeHelper dh = new HDevDisposeHelper())
      {
      HOperatorSet.DispLine(hv_WindowHandle, hv_max_r, hv_max_c, hv_Params.TupleSelect(
          0), hv_Params.TupleSelect(1));
      }
      if (HDevWindowStack.IsOpen())
      {
        using (HDevDisposeHelper dh = new HDevDisposeHelper())
        {
        HOperatorSet.DispText(HDevWindowStack.GetActive(), hv_max_dist, "window", 
            hv_max_r-20, hv_max_c-20, "red", new HTuple(), new HTuple());
        }
      }
      HOperatorSet.CloseWindow(hv_WindowHandle);
    }

    ho_template_img.Dispose();
    ho_ShapeModelImage.Dispose();
    ho_ShapeModelRegion.Dispose();
    ho_ShapeModel.Dispose();
    ho_match_img.Dispose();
    ho_ObjectXLD.Dispose();
    ho_ModelContour.Dispose();
    ho_MeasureContour.Dispose();
    ho_Cross.Dispose();
    ho_Contour.Dispose();
    ho_rect.Dispose();
    ho_match_top.Dispose();
    ho_edges.Dispose();
    ho_edge.Dispose();

    hv_ModelID.Dispose();
    hv_RowCheck.Dispose();
    hv_ColumnCheck.Dispose();
    hv_AngleCheck.Dispose();
    hv_Score.Dispose();
    hv_template_top_line.Dispose();
    hv_mat_i.Dispose();
    hv_mat_t.Dispose();
    hv_mat_r.Dispose();
    hv_line_start_y.Dispose();
    hv_line_start_x.Dispose();
    hv_line_end_y.Dispose();
    hv_line_end_x.Dispose();
    hv_obj_idx.Dispose();
    hv_MovementOfObject.Dispose();
    hv_obj_line_start_y.Dispose();
    hv_obj_line_start_x.Dispose();
    hv_obj_line_end_y.Dispose();
    hv_obj_line_end_x.Dispose();
    hv_MetroHandle.Dispose();
    hv_top_line.Dispose();
    hv_LineIndices.Dispose();
    hv_Row.Dispose();
    hv_Column.Dispose();
    hv_Params.Dispose();
    hv_n_edge.Dispose();
    hv_v_lx.Dispose();
    hv_v_ly.Dispose();
    hv_max_dist.Dispose();
    hv_max_r.Dispose();
    hv_max_c.Dispose();
    hv_i.Dispose();
    hv_edge_row.Dispose();
    hv_edge_col.Dispose();
    hv_j.Dispose();
    hv_dist.Dispose();
    hv_p_x.Dispose();
    hv_p_y.Dispose();
    hv_WindowHandle.Dispose();

  }

#endif


}
#if !(NO_EXPORT_MAIN || NO_EXPORT_APP_MAIN)
public class HDevelopExportApp
{
  static void Main(string[] args)
  {
    new HDevelopExport();
  }
}
#endif

