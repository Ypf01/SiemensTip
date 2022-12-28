using SiemensTip.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SiemensTip.src
{
    public interface IWork
    {
        /// <summary>
        /// 临时算法结果
        /// </summary>
        int TempResult { get; set; }
        /// <summary>
        /// 相机开始索引
        /// </summary>
        int StartIndex { get; set; }
        /// <summary>
        /// 工位相机数
        /// </summary>
        int CameraNumber { get; set; }
        /// <summary>
        /// 相机取图数组
        /// </summary>
        Task[] PhotoGraph { get; set; }
        /// <summary>
        /// 算法任务集合
        /// </summary>
        List<Task> Inspect { get; set; }
        /// <summary>
        /// 当前工位产品
        /// </summary>
        Product TProduct { get; set; }
        /// <summary>
        /// 工位任务
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task InspectTask();
    }
}
