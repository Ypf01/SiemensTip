using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenCvSharp;
using HalconDotNet;
using BMS_tip_wrapper;
using System.Collections;
using Newtonsoft.Json;

namespace BMS_tip_wrapper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Mat img, test_img, result_img;

            string lateral_template_img_path = "../../test/lateral_template.png";   // 侧边模板图路径
            string lateral_test_img_path = "../../test/lateral_test.png";   // 侧边测试图路径
            string bottom_template_img_path = "../../test/bottom_template.png"; // 底部模板图路径
            string bottom_test_img_path = "../../test/bottom_test.png"; // 底部测试图路径
            string lateral_template_model_path = "../../test/lateral_template.hmodel";  // 侧边模板模型路径
            string bottom_template_model_path = "../../test/bottom_template.hmodel";    // 底部模板模型路径
            string params_path = "../../test/tip_params.yml";   // 参数路径

            // Solution object / Solution对象
            Solution solution = new Solution();
            
            // Init solution / 初始化solution
            bool create_template;           
            create_template = File.Exists(lateral_template_model_path) && File.Exists(params_path) && File.Exists(bottom_template_model_path) ? false : true;
            if (!create_template)
            {
                // Load lateral model / 加载侧边模板
                solution.load_template(lateral_template_model_path, "top");

                // Load bottom model / 加载底部模板
                solution.load_template(bottom_template_model_path, "circle");

                // Load solution params / 加载参数
                string params_str = File.ReadAllText(params_path);
                solution.solution_params = JsonConvert.DeserializeObject<Params>(params_str);
            }
            else
            {
                // Setting thresholds / 设置阈值
                solution.solution_params.radius_th = new float[2] { 46.5F, 48 }; // radius threshold / 圆半径阈值
                solution.solution_params.lateral_th = new float[2] { 0, 3.5F };   // lateral threshold / 批锋阈值
                solution.solution_params.axial_distance_th = new float[2] { 0, 5.0F };    // axial distance threshold / 离心度阈值

                // Create lateral template / 生成侧边模板
                img = Cv2.ImRead(lateral_template_img_path);
                Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);
                solution.create_lateral_template(img, lateral_template_model_path);

                // Draw lateral metro line / 侧边测量线
                //solution.draw_top_line(img);

                // Create bottom template / 生成底部模板
                img = Cv2.ImRead(bottom_template_img_path);
                Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);
                solution.create_bottom_template(img, bottom_template_model_path);

                // Save params / 保存参数
                string params_str = JsonConvert.SerializeObject(solution.solution_params);
                File.WriteAllText(params_path, params_str);

                img.Dispose();
            }

            // -------------------------- 1. Lateral Test / 侧边测试 -------------------------- 
            // Test lateral algo. / 测试侧边检测算法
            test_img = Cv2.ImRead(lateral_test_img_path);
            Cv2.CvtColor(test_img, test_img, ColorConversionCodes.BGR2GRAY);
            List<float> distances;  // 批锋长度
            List<bool> lateral_results;
            if (solution.detect_spines(test_img.Clone(), out result_img, out distances, out lateral_results))
            {
                System.Console.WriteLine("Pass: Lateral");
            }
            else
            {
                System.Console.WriteLine("Fail: Lateral");
            }
            // Viz / 结果图
            Cv2.ImWrite("spines_viz.png", result_img);


            // -------------------------- 2. Bottom Test / 底部测试 -------------------------- 
            // Test bottom algo. / 测试底部检测算法
            test_img = Cv2.ImRead(bottom_test_img_path);
            Cv2.CvtColor(test_img, test_img, ColorConversionCodes.BGR2GRAY);

            List<Solution.Circle> circles;  // 半径大小
            List<bool> bottom_results;
            List<Point> standard_positions = new List<Point> { 
                new Point(3340, 1285),
                new Point(1660, 1300),
                new Point(2515, 1280),
                new Point(760, 1290)
            };  // 标准位置
            if (solution.measure_circles(test_img.Clone(), standard_positions, out result_img, out circles, out bottom_results))
            {
                System.Console.WriteLine("Pass: Bottom");
            }
            else
            {
                System.Console.WriteLine("Fail: Bottom");
            }
            // Viz / 结果图
            Cv2.ImWrite("viz_circles.png", result_img);

            test_img.Dispose();
            result_img.Dispose();
        }
    }
}
