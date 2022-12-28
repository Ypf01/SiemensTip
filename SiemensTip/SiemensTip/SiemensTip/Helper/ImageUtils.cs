using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTip.Helper
{
    public class ImageUtils
    {
        public static void JpegComress(Mat img, out byte[] compressedImg)
        {
            ImageEncodingParam[] prms = new ImageEncodingParam[1];
            ImageEncodingParam param = new ImageEncodingParam(ImwriteFlags.JpegQuality, ConstHelper.JpegQuality);
            prms[0] = param;
            Cv2.ImEncode(ConstHelper.JpegExtension, img, out compressedImg, prms);
        }

        public static void CreateEncodedImageMap(Dictionary<string, Mat> matMap, out Dictionary<string, byte[]> compressedMap)
        {
            compressedMap = new Dictionary<string, byte[]>();
            foreach (var fovPair in matMap)
            {
                var lightType = fovPair.Key;
                var img = fovPair.Value;

                ImageUtils.JpegComress(img, out var compressed);
                compressedMap.Add(lightType, compressed);
            }
        }
    }
}
