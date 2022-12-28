using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using HalconDotNet;
using System.Collections;
using System.Runtime.InteropServices;


namespace BMS_tip_wrapper
{
    public class Params
    {
        public KeyPoint[] metro_line { get; set; }
        public List<List<Point>> bottom_standard_points { get; set; }
        public float[] lateral_th { get; set; }
        public float[] radius_th { get; set; }
        public float[] axial_distance_th { get; set; }

        public Params()
        {
            metro_line = new KeyPoint[2];
            bottom_standard_points = new List<List<Point>>();
            lateral_th = new float[2] { 0, 100 };
            radius_th = new float[2] { 0, 100 };
            axial_distance_th = new float[2] { 0, 100 };
        }
    }
    public class Pointsd
    {

    }
    public class Solution
    {
        public struct Circle
        {
            public double x { get; }
            public double y { get; }
            public double r { get; }
            public Circle(double x, double y, double r)
            {
                this.x = x;
                this.y = y;
                this.r = r;
            }
        }

        private HTuple bottom_template_model_id = new HTuple();
        private HTuple lateral_template_model_id = new HTuple();
        public Params solution_params = new Params();
        public KeyPoint[] metro_line = new KeyPoint[2];
        public int round = 0;
        public List<List<Point>> bottom_standard_points = new List<List<Point>>();


        // Calculate point to point distance
        public static float p2p_distance(Point pt1, Point pt2)
        {
            return (float)Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
        }

        public static void draw_cross(Mat img, Point pt, float size, Scalar color)
        {
            Cv2.Line(img, new Point(pt.X, pt.Y - (int)size / 2), new Point(pt.X, pt.Y + (int)size / 2), color, 2);
            Cv2.Line(img, new Point(pt.X - (int)size / 2, pt.Y), new Point(pt.X + (int)size / 2, pt.Y), color, 2);
        }

        // Convert Halcon image to CV Mat
        public static void HObect2cvMat(HObject h_mat, out Mat cv_mat)
        {
            HTuple htChannels;
            HTuple cType = null;
            HTuple width, height;


            htChannels = null;
            HOperatorSet.CountChannels(h_mat, out htChannels);

            if (htChannels[0].I == 1)
            {
                HTuple ptr;
                HOperatorSet.GetImagePointer1(h_mat, out ptr, out cType, out width, out height);

                byte[] dst = new byte[width.I * height.I];
                Marshal.Copy(ptr.IP, dst, 0, width.I * height.I);
                cv_mat = new Mat(height.I, width.I, MatType.CV_8UC1, dst);
            }
            else if (htChannels[0].I == 3)
            {
                HTuple ptrRed;
                HTuple ptrGreen;
                HTuple ptrBlue;

                HOperatorSet.GetImagePointer3(h_mat, out ptrRed, out ptrGreen, out ptrBlue, out cType, out width, out height);
                byte[] dst_r = new byte[width.I * height.I];
                Marshal.Copy(ptrRed.IP, dst_r, 0, width.I * height.I);
                Mat cvRed = new Mat(height.I, width.I, MatType.CV_8UC1, dst_r);
                byte[] dst_g = new byte[width.I * height.I];
                Marshal.Copy(ptrRed.IP, dst_r, 0, width.I * height.I);
                Mat cvGreen = new Mat(height.I, width.I, MatType.CV_8UC1, dst_g);
                byte[] dst_b = new byte[width.I * height.I];
                Marshal.Copy(ptrRed.IP, dst_r, 0, width.I * height.I);
                Mat cvBlue = new Mat(height.I, width.I, MatType.CV_8UC1, dst_b);

                cv_mat = new Mat(height.I, width.I, MatType.CV_8UC3);
                Mat[] multi = new Mat[] { cvBlue, cvGreen, cvRed };
                Cv2.Merge(multi, cv_mat);
                cvRed.Dispose();
                cvGreen.Dispose();
                cvBlue.Dispose();
            }
            else
            {
                cv_mat = null;
            }
        }


        // Convert CV Mat to Halcon image
        public static void cvMat2HObject(Mat cv_mat, out HObject h_mat)
        {
            HOperatorSet.GenEmptyObj(out h_mat);
            int channels = cv_mat.Channels();
            int width, height;

            if (channels == 1)
            {
                width = cv_mat.Cols;
                height = cv_mat.Rows;
                /*				byte[] dst = new byte[width * height];
                                for (int r = 0; r < height; r++)
                                {
                                    Marshal.Copy(cv_mat.Data + r * width, dst, 0, width);
                                }
                                IntPtr img_data = Marshal.UnsafeAddrOfPinnedArrayElement(dst, 0);*/
                HOperatorSet.GenImage1(out h_mat, "byte", width, height, cv_mat.Data);
            }
            else if (channels == 3)
            {
                width = cv_mat.Cols;
                height = cv_mat.Rows;
                Mat[] bgr = Cv2.Split(cv_mat);
                HOperatorSet.GenImage3(out h_mat, "byte", width, height, bgr[2].Data, bgr[1].Data, bgr[0].Data);
            }
        }


        // Load Halcon template model
        public void load_template(string template_path, string template_name)
        {
            if (template_name.Equals("top"))
            {
                HOperatorSet.ReadShapeModel(template_path, out lateral_template_model_id);
            }
            else if (template_name.Equals("circle"))
            {
                HOperatorSet.ReadShapeModel(template_path, out bottom_template_model_id);
            }

        }


        // Create bottom template model
        public void create_bottom_template(Mat img, string template_path)
        {
            HObject template_img;
            HOperatorSet.GenEmptyObj(out template_img);
            cvMat2HObject(img, out template_img);

            HOperatorSet.CreateShapeModel(template_img, 3, -.4, 0.8, 0.008, "none", "use_polarity", 100,
                90, out bottom_template_model_id);
            HOperatorSet.WriteShapeModel(bottom_template_model_id, template_path);

            template_img.Dispose();
        }


        // Bottom measurements
        public bool measure_circles(Mat img, List<Point> standard_positions, out Mat ret_img, out List<Circle> circles, out List<bool> results)
        {
            results = new List<bool>();
            HObject h_img;
            HOperatorSet.GenEmptyObj(out h_img);
            cvMat2HObject(img, out h_img);
            circles = new List<Circle>();

            HTuple row, column, angle, score;
            HOperatorSet.FindShapeModel(h_img, bottom_template_model_id, -0.4, 0.8, 0.3, 0, 0, "least_squares",
                4, 0.9, out row, out column, out angle, out score);

            List<Tuple<float, int>> seq = new List<Tuple<float, int>>();
            HTuple h_n_obj = (score.TupleLength()) - 1;
            for (HTuple h_obj_idx = 0; h_obj_idx.Continue(h_n_obj, 1); h_obj_idx += 1)
            {
                seq.Add(Tuple.Create((float)column.TupleSelect(h_obj_idx).D, h_obj_idx.I));
            }
            seq.Sort((a, b) => (int)(a.Item1 - b.Item1));


            {
                for (int obj_idx = 0; obj_idx < seq.Count; obj_idx++)
                //for (HTuple i_obj = 0; i_obj.Continue((score.TupleLength()) - 1, 1); i_obj += 1)
                {
                    HTuple h_obj_moment;
                    //HOperatorSet.VectorAngleToRigid(0, 0, 0, row[i_obj], column[i_obj],
                    //    angle[i_obj], out h_obj_moment);
                    HOperatorSet.VectorAngleToRigid(0, 0, 0, row.TupleSelect(seq[obj_idx].Item2), column.TupleSelect(seq[obj_idx].Item2),
                        angle.TupleSelect(seq[obj_idx].Item2), out h_obj_moment);

                    HTuple h_metro_handle;
                    HOperatorSet.CreateMetrologyModel(out h_metro_handle);
                    HTuple h_circles_idxs;
                    HTuple h_circle_metro_params;
                    //HOperatorSet.TupleConcat(row[i_obj], column[i_obj], out h_circle_metro_params);
                    HOperatorSet.TupleConcat(row.TupleSelect(seq[obj_idx].Item2), column.TupleSelect(seq[obj_idx].Item2), out h_circle_metro_params);
                    HOperatorSet.TupleConcat(h_circle_metro_params, 50, out h_circle_metro_params);
                    HOperatorSet.AddMetrologyObjectGeneric(h_metro_handle, "circle", h_circle_metro_params, 10, 2, 2, 10, new HTuple(), new HTuple(), out h_circles_idxs);
                    //Inspect the shapes that have been added to the metrology model
                    HOperatorSet.ApplyMetrologyModel(h_img, h_metro_handle);
                    //GetMetrologyObjectModelContour(&ho_ModelContour, hv_MetroHandle, "all", 1);
                    //GetMetrologyObjectMeasures(&ho_MeasureContour, hv_MetroHandle, "all", "all",
                    //    &hv_Row, &hv_Column);
                    //GenCrossContourXld(&ho_Cross, hv_Row, hv_Column, 5, 0);
                    HTuple h_circle_params;
                    HOperatorSet.GetMetrologyObjectResult(h_metro_handle, "all", "all", "result_type", "all_param",
                        out h_circle_params);
                    circles.Add(new Circle(h_circle_params[1].D, h_circle_params[0].D, h_circle_params[2].D));
                }
            }

            bool pass = true;
            ret_img = new Mat(img.Rows, img.Cols, MatType.CV_8UC3);
            Cv2.CvtColor(img, ret_img, ColorConversionCodes.GRAY2BGR);
            foreach (Circle circle in circles)
            {
                bool radius_pass = circle.r >= solution_params.radius_th[0] && circle.r < solution_params.radius_th[1];
                bool position_pass = false;
                float distance = 1000;
                foreach (Point stand_position in standard_positions)
                {
                    float dist = p2p_distance(new Point(circle.x, circle.y), stand_position);
                    distance = dist < distance ? dist : distance;
                    if (dist >= solution_params.axial_distance_th[0] && dist < solution_params.axial_distance_th[1])
                    {
                        position_pass = true;
                    }
                }

                Scalar draw_color;
                draw_color = radius_pass && position_pass ? new Scalar(0, 255, 0) : new Scalar(0, 0, 255);
                if (!(radius_pass && position_pass))
                {
                    pass = false;
                }
                results.Add(radius_pass && position_pass);

                Cv2.Circle(ret_img, new Point((int)circle.x, (int)circle.y), (int)circle.r, draw_color, 3);
                Cv2.Circle(ret_img, new Point((int)circle.x, (int)circle.y), 4, draw_color, -1);
                Cv2.PutText(ret_img, circle.r.ToString("f2") + " / " + distance.ToString("f2"), new Point((int)circle.x + 50, (int)circle.y + 50), HersheyFonts.HersheySimplex, 3, draw_color, 3);
            }
            foreach (Point std_pt in standard_positions)
            {
                draw_cross(ret_img, std_pt, 10, new Scalar(128, 128, 0));
            }
            if (circles.Count != standard_positions.Count)
            {
                pass = false;
            }

            img.Dispose();
            h_img.Dispose();

            return pass;
        }

        public void create_lateral_template(Mat img, string template_path)
        {
            HObject template_img;
            HOperatorSet.GenEmptyObj(out template_img);
            cvMat2HObject(img, out template_img);

            HOperatorSet.CreateShapeModel(template_img, 3, -.4, 0.8, 0.008, "none", "use_polarity", 30,
                25, out lateral_template_model_id);
            HOperatorSet.WriteShapeModel(lateral_template_model_id, template_path);

            template_img.Dispose();
        }

        //public void draw_top_line(Mat img)
        public void get_top_line(Mat img, Point[] pts)
        {
            // Choose top line
            // Rect rect_line = Cv2.SelectROI(img);
            //Point2f top_left = new Point2f(rect_line.X, rect_line.Y);
            //Point2f bottom_right = new Point2f(rect_line.X + rect_line.Width, rect_line.Y + rect_line.Height);
            Point2f top_left = new Point2f(pts[0].X, pts[0].Y);
            Point2f bottom_right = new Point2f(pts[1].X, pts[1].Y);


            // Transform top line to template coordinate
            HObject h_img;
            HOperatorSet.GenEmptyObj(out h_img);
            cvMat2HObject(img, out h_img);

            HTuple row, column, angle, score;
            HOperatorSet.FindShapeModel(h_img, lateral_template_model_id, -0.4, 0.8, 0.9, 0, 0.5, "least_squares",
                2, 0.7, out row, out column, out angle, out score);

            HTuple template_top_line = new HTuple();
            template_top_line[0] = (int)top_left.Y;
            template_top_line[1] = (int)top_left.X;
            template_top_line[2] = (int)bottom_right.Y;
            template_top_line[3] = (int)bottom_right.X;

            HTuple mat_i;
            HOperatorSet.HomMat2dIdentity(out mat_i);
            HTuple mat_t;
            HOperatorSet.HomMat2dTranslate(mat_i, -row.TupleSelect(0), -column.TupleSelect(0), out mat_t);
            HTuple mat_r;
            HOperatorSet.HomMat2dRotate(mat_t, -angle.TupleSelect(0), 0, 0, out mat_r);

            HTuple line_start_y, line_start_x, line_end_y, line_end_x;
            HOperatorSet.AffineTransPoint2d(mat_r, template_top_line.TupleSelect(0), template_top_line.TupleSelect(1),
                out line_start_y, out line_start_x);
            HOperatorSet.AffineTransPoint2d(mat_r, template_top_line.TupleSelect(2), template_top_line.TupleSelect(3),
                out line_end_y, out line_end_x);

            // Record top line in template coordinate
            //metro_line[0] = new KeyPoint((float)line_start_x.D, (float)line_start_y.D, 1);
            //metro_line[1] = new KeyPoint((float)line_end_x.D, (float)line_end_y.D, 1);

            solution_params.metro_line[0] = new KeyPoint((float)line_start_x.D, (float)line_start_y.D, 1);
            solution_params.metro_line[1] = new KeyPoint((float)line_end_x.D, (float)line_end_y.D, 1);

            h_img.Dispose();
        }

        public bool detect_spines(Mat img, out Mat ret_img, out List<float> distances, out List<bool> results)
        {
            results = new List<bool>();
            HObject h_img;
            HOperatorSet.GenEmptyObj(out h_img);
            cvMat2HObject(img, out h_img);

            List<List<Point>> top_lines = new List<List<Point>>();
            List<Point> top_line_far_points = new List<Point>();
            distances = new List<float>();

            // template match
            HTuple row, column, angle, score;
            HOperatorSet.FindShapeModel(h_img, lateral_template_model_id, -0.4, 0.8, 0.3, 0, 0, "least_squares",
                4, 0.9, out row, out column, out angle, out score);

            List<Tuple<float, int>> seq = new List<Tuple<float, int>>();
            HTuple h_n_obj = (score.TupleLength()) - 1;
            for (HTuple h_obj_idx = 0; h_obj_idx.Continue(h_n_obj, 1); h_obj_idx += 1)
            {
                seq.Add(Tuple.Create((float)column.TupleSelect(h_obj_idx).D, h_obj_idx.I));
            }
            seq.Sort((a, b) => (int)(a.Item1 - b.Item1));

            {
                //HTuple h_n_obj = (score.TupleLength()) - 1;
                //HTuple h_step = 1;
                //HTuple h_obj_idx;
                for (int obj_idx = 0; obj_idx < seq.Count; obj_idx++)
                //for (h_obj_idx = 0; h_obj_idx.Continue(h_n_obj, h_step); h_obj_idx += h_step)
                {
                    // 1. compute object moment
                    HTuple h_obj_moment;
                    //HOperatorSet.VectorAngleToRigid(0, 0, 0, row.TupleSelect(h_obj_idx), column.TupleSelect(h_obj_idx), angle.TupleSelect(h_obj_idx),
                    //    out h_obj_moment);
                    HOperatorSet.VectorAngleToRigid(0, 0, 0, row.TupleSelect(seq[obj_idx].Item2), column.TupleSelect(seq[obj_idx].Item2), angle.TupleSelect(seq[obj_idx].Item2),
                        out h_obj_moment);

                    // 2. compute target metro line position
                    HTuple h_obj_line_start_x, h_obj_line_start_y, h_obj_line_end_x, h_obj_line_end_y;
                    //HOperatorSet.AffineTransPoint2d(h_obj_moment, (HTuple)metro_line[0].Pt.Y, (HTuple)metro_line[0].Pt.X,
                    //	out h_obj_line_start_y, out h_obj_line_start_x);
                    //HOperatorSet.AffineTransPoint2d(h_obj_moment, (HTuple)metro_line[1].Pt.Y, (HTuple)metro_line[1].Pt.X,
                    //	out h_obj_line_end_y, out h_obj_line_end_x);
                    HOperatorSet.AffineTransPoint2d(h_obj_moment, (HTuple)solution_params.metro_line[0].Pt.Y, (HTuple)solution_params.metro_line[0].Pt.X,
                        out h_obj_line_start_y, out h_obj_line_start_x);
                    HOperatorSet.AffineTransPoint2d(h_obj_moment, (HTuple)solution_params.metro_line[1].Pt.Y, (HTuple)solution_params.metro_line[1].Pt.X,
                        out h_obj_line_end_y, out h_obj_line_end_x);

                    HTuple h_metro_handle;
                    HOperatorSet.CreateMetrologyModel(out h_metro_handle);
                    HTuple h_top_line = new HTuple();
                    h_top_line.Append(h_obj_line_start_y);
                    h_top_line.Append(h_obj_line_start_x);
                    h_top_line.Append(h_obj_line_end_y);
                    h_top_line.Append(h_obj_line_end_x);

                    List<Point> line = new List<Point>();
                    line.Add(new Point(h_obj_line_start_x.D, h_obj_line_start_y.D));
                    line.Add(new Point(h_obj_line_end_x.D, h_obj_line_end_y.D));
                    top_lines.Add(line);

                    // 3. metrology finds line
                    HTuple h_line_idxs;
                    HOperatorSet.AddMetrologyObjectGeneric(h_metro_handle, "line", h_top_line, 10, 2, 2, 50,
                        new HTuple(), new HTuple(), out h_line_idxs);    // place metrology

                    HOperatorSet.ApplyMetrologyModel(h_img, h_metro_handle);
                    //HalconCpp::GetMetrologyObjectModelContour(&ho_ModelContour, h_metro_handle, "all", 1);
                    HObject h_measure_contour;
                    HOperatorSet.GenEmptyObj(out h_measure_contour);
                    HTuple h_measure_row, h_measure_column;
                    HOperatorSet.GetMetrologyObjectMeasures(out h_measure_contour, h_metro_handle, "all", "all",
                        out h_measure_row, out h_measure_column); // measure
                                                                  //HalconCpp::GenCrossContourXld(&ho_Cross, h_measure_row, h_measure_column, 5, 0);

                    HTuple h_line_params;
                    HOperatorSet.GetMetrologyObjectResult(h_metro_handle, "all", "all", "result_type", "all_param",
                        out h_line_params);    // fit line

                    HObject h_line_contour;
                    HOperatorSet.GenEmptyObj(out h_line_contour);
                    HOperatorSet.GetMetrologyObjectResultContour(out h_line_contour, h_metro_handle, "all", "all", 1.5);


                    // 4. find edges
                    HObject h_rect;
                    HOperatorSet.GenEmptyObj(out h_rect);
                    HOperatorSet.GenRectangle1(out h_rect, h_obj_line_start_y - 20, h_obj_line_start_x,
                        h_obj_line_end_y + 20, h_obj_line_end_x);   // rop roi

                    HObject h_img_top;
                    HOperatorSet.GenEmptyObj(out h_img_top);
                    HOperatorSet.ReduceDomain(h_img, h_rect, out h_img_top); // crop top roi

                    HObject h_edges;
                    HOperatorSet.GenEmptyObj(out h_edges);
                    HOperatorSet.EdgesSubPix(h_img_top, out h_edges, "deriche1", 4, 25, 30); // find edges

                    HTuple h_n_edges;
                    HOperatorSet.CountObj(h_edges, out h_n_edges);   // count edges

                    HTuple h_v_lx, h_v_ly;
                    h_v_lx = h_line_params.TupleSelect(3) - h_line_params.TupleSelect(1);
                    h_v_ly = h_line_params.TupleSelect(2) - h_line_params.TupleSelect(0);

                    HTuple h_max_dist = 0;
                    HTuple h_max_r = 0;
                    HTuple h_max_c = 0;
                    {
                        for (HTuple i_edge = 1; i_edge.Continue(h_n_edges, 1); i_edge += 1)
                        {
                            HObject h_edge;
                            HOperatorSet.GenEmptyObj(out h_edge);
                            HOperatorSet.SelectObj(h_edges, out h_edge, i_edge);
                            HTuple h_edge_row, h_edge_column;
                            HOperatorSet.GetContourXld(h_edge, out h_edge_row, out h_edge_column);
                            {
                                for (HTuple i_xld = 1; i_xld.Continue((h_edge_row.TupleLength()) - 1, 1); i_xld += 1)
                                {
                                    HTuple h_dist;
                                    HOperatorSet.DistancePl(h_edge_row.TupleSelect(i_xld), h_edge_column.TupleSelect(i_xld), h_line_params.TupleSelect(0),
                                        h_line_params.TupleSelect(1), h_line_params.TupleSelect(2), h_line_params.TupleSelect(3), out h_dist);

                                    HTuple h_p_x = h_edge_column.TupleSelect(i_xld) - h_obj_line_start_x;
                                    HTuple h_p_y = h_edge_row.TupleSelect(i_xld) - h_obj_line_start_y;
                                    if (((h_v_ly * h_p_x) - (h_v_lx * h_p_y)).D < 0)
                                    {
                                        if (h_dist.D > h_max_dist.D)
                                        {
                                            h_max_dist = h_dist;
                                            h_max_r = h_edge_row.TupleSelect(i_xld);
                                            h_max_c = h_edge_column.TupleSelect(i_xld);
                                        }
                                    }
                                }
                            }

                            h_edge.Dispose();
                        }
                    }

                    //std::cout << h_max_r.D() << ", " << h_max_c.D() << std::endl;
                    top_line_far_points.Add(new Point(h_max_c.D, h_max_r.D));
                    distances.Add((float)h_max_dist.D);

                    h_measure_contour.Dispose();
                    h_line_contour.Dispose();
                    h_img_top.Dispose();
                    h_edges.Dispose();
                }
            }


            bool pass = true;
            ret_img = new Mat(img.Rows, img.Cols, MatType.CV_8UC3);
            Cv2.CvtColor(img, ret_img, ColorConversionCodes.GRAY2BGR);
            for (int i_line = 0; i_line < top_lines.Count(); i_line++)
            {
                Cv2.Line(ret_img, new Point((int)(top_lines[i_line][0].X + .5), (int)(top_lines[i_line][0].Y + .5)),
                    new Point((int)(top_lines[i_line][1].X + .5), (int)(top_lines[i_line][1].Y + .5)), new Scalar(0, 255, 0), 3);
                Cv2.Circle(ret_img, new Point((int)(top_line_far_points[i_line].X + .5), (int)(top_line_far_points[i_line].Y + .5)),
                    3, new Scalar(0, 0, 255), -1);
                if (!(distances[i_line] >= solution_params.lateral_th[0] && distances[i_line] < solution_params.lateral_th[1]))
                {
                    results.Add(false);
                    pass = false;
                    Cv2.PutText(ret_img, distances[i_line].ToString(), new Point((int)(top_lines[i_line][1].X + 20), (int)(top_lines[i_line][1].Y + 20)),
                        HersheyFonts.HersheySimplex, 3, new Scalar(0, 0, 255));
                }
                else
                {
                    results.Add(true);
                    Cv2.PutText(ret_img, distances[i_line].ToString(), new Point((int)(top_lines[i_line][1].X + 20), (int)(top_lines[i_line][1].Y + 20)),
                        HersheyFonts.HersheySimplex, 3, new Scalar(0, 255, 0));
                }
            }

            img.Dispose();
            h_img.Dispose();

            return pass;
        }
    }
}
