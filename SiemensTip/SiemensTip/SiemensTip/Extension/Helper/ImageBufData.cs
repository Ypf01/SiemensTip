using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTip.Extension.helpers
{
    public class ImageBufData
    {
        public byte[] Data;
        public int Width;
        public int Height;

        public Mat ConvertImageType()
        {
            var data = this;
            if (data.Width == 0 || data.Height == 0)
                return new Mat();
            //var t = DateTime.Now;
            Mat img = new Mat(data.Height, data.Width, MatType.CV_8UC1, data.Data);
            //Mat imgS = new Mat();
            //Cv2.CvtColor(img, imgS, ColorConversionCodes.BayerRG2RGB);
            //img.Dispose();
            return img;
        }
    }
}
