using MvCamCtrl.NET;
using SiemensTip.Extension.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiemensTip.Extension
{
    public class HikCamera
    {
        public MyCamera myCamera;
        private MyCamera.MV_CC_DEVICE_INFO_LIST deviceList;//设备列表
        private MyCamera.MV_CC_DEVICE_INFO deviceInfo;//设备对象
        public string UserDefinedName;//
        public string SerialNumber;//接收相机序列号
        private string idName = "";

        /// <summary>
        /// 相机开启OK
        /// </summary>
        public bool StartGrab { get; set; }
        public bool Connected { get => myCamera != null && myCamera.MV_CC_IsDeviceConnected_NET(); }
        public HikCamera()
        {
            deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isSerialNumber"></param>
        /// <returns></returns>
        public bool ConnectCamera(string id, bool isSerialNumber = false)
        {
            bool flag = false;
            bool getTheFirst = false;
            idName = id;
            int temp;//接收命令执行结果
            myCamera = new MyCamera();
            try
            {
                temp = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref deviceList);//更新设备列表
                for (int i = 0; i < deviceList.nDeviceNum && !getTheFirst; i++)
                {
                    string m_SerialNumber = "";//接收设备返回的序列号
                    string m_DefinedName = "";
                    deviceInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));//获取设备
                    if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceInfo.SpecialInfo.stGigEInfo, 0);
                        MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        m_SerialNumber = gigeInfo.chSerialNumber;//获取序列号
                        m_DefinedName = gigeInfo.chUserDefinedName;
                    }
                    else if (deviceInfo.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceInfo.SpecialInfo.stUsb3VInfo, 0);
                        MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        m_SerialNumber = usbInfo.chSerialNumber;
                        m_DefinedName = usbInfo.chUserDefinedName;
                    }
                    if ((isSerialNumber && m_SerialNumber.Equals(id)) || (!isSerialNumber && m_DefinedName.Equals(id)))
                    {
                        flag = true;
                        this.UserDefinedName = m_DefinedName;
                        this.SerialNumber = m_SerialNumber;
                        temp = myCamera.MV_CC_CreateDevice_NET(ref deviceInfo);

                        if (MyCamera.MV_OK != temp)
                        {
                            //创建相机失败
                            Log.ErrorLog($"创建相机 {id} 失败");
                            flag = false;
                        }
                        temp = myCamera.MV_CC_OpenDevice_NET();//
                        if (MyCamera.MV_OK != temp)
                        {
                            //打开相机失败
                            Log.ErrorLog($"打开相机 {id} 失败");
                            flag = false;
                        }
                        flag &= true;
                        getTheFirst = flag;


                        // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                        if (flag)
                            if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                            {
                                int nPacketSize = myCamera.MV_CC_GetOptimalPacketSize_NET();
                                if (nPacketSize > 0)
                                {
                                    temp = myCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                                    if (temp != MyCamera.MV_OK)
                                    {
                                        Log.ErrorLog("Set Packet Error " + idName);
                                        flag = false;
                                    }
                                }
                                else
                                    Log.ErrorLog("Get Packet Error " + idName);
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ErrorLog(e.Message);
                flag = false;
            }
            return flag;
        }

        public bool ConnectCameraWithIP(string devIp)
        {
            myCamera = new MyCamera();
            Int32 devIndex = -1;
            // ch:打印设备信息 en:Print device info
            for (Int32 i = 0; i < deviceList.nDeviceNum; i++)
            {
                deviceInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (MyCamera.MV_GIGE_DEVICE == deviceInfo.nTLayerType)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(deviceInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                    uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                    uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                    uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);
                    Console.WriteLine("[device " + i.ToString() + "]:");
                    Console.WriteLine("DevIP:" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                    Console.WriteLine("UserDefineName:" + stGigEDeviceInfo.chUserDefinedName + "\n");
                    string devip = nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4;
                    if (devIndex == -1 && devip == devIp)
                    {
                        devIndex = i;
                    }
                }
            }
            var flag = devIndex != -1;
            if (flag)
            {
                deviceInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(deviceList.pDeviceInfo[devIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));

                // ch:创建设备 | en:Create device
                int nRet = myCamera.MV_CC_CreateDevice_NET(ref deviceInfo);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Create device failed:{0:x8}", nRet);
                    flag = false;

                }
                // ch:打开设备 | en:Open device
                nRet = myCamera.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Open device failed:{0:x8}", nRet);
                    flag = false;

                }
            }
            if (flag)
                if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int nPacketSize = myCamera.MV_CC_GetOptimalPacketSize_NET();
                    if (nPacketSize > 0)
                    {
                        var temp = myCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                        if (temp != MyCamera.MV_OK)
                            flag = false;
                    }
                    else
                        flag = false;
                }
            return flag;
        }


        public bool CloseCamera()//关闭相机
        {
            bool flag = StopCamera();
            if (myCamera == null)
                return true;
            int temp = myCamera.MV_CC_CloseDevice_NET();
            if (MyCamera.MV_OK != temp)
                flag = false;
            temp = myCamera.MV_CC_DestroyDevice_NET();
            if (MyCamera.MV_OK != temp)
                flag = false;
            return flag;
        }

        //发送成功返回0，失败返回-1
        private bool SoftTrigger()
        {
            int temp = myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != temp)
                return false;
            return true;
        }

        //2.设置枚举型参数
        public bool SetTriggerMode(uint TriggerMode = 1, int source = 1)//设置触发事件，成功返回0失败返回-1
        {
            bool flag = true;
            //1:On 触发模式
            //0:Off 非触发模式
            int returnValue = myCamera.MV_CC_SetEnumValue_NET("TriggerMode", TriggerMode);
            if (MyCamera.MV_OK != returnValue)
                flag = false;
            #region 打开水印
            //int nRet = myCamera.MV_CC_SetEnumValue_NET("FrameSpecInfoSelector", 5);  // 5 -framecounter
            //Console.WriteLine("myCamera.MV_CC_SetEnumValue_NET(FrameSpecInfoSelector, 5)=" + nRet);
            //nRet = myCamera.MV_CC_SetBoolValue_NET("FrameSpecInfo", true);
            //Console.WriteLine("myCamera.MV_CC_SetBoolValue_NET(FrameSpecInfo,true)=" + nRet);
            //nRet = myCamera.MV_CC_SetEnumValue_NET("FrameSpecInfoSelector", 6);  //6-triggerindex
            //Console.WriteLine(" myCamera.MV_CC_SetEnumValue_NET(FrameSpecInfoSelector, 6)=" + nRet);
            //nRet = myCamera.MV_CC_SetBoolValue_NET("FrameSpecInfo", true);
            //Console.WriteLine(" myCamera.MV_CC_SetBoolValue_NET(FrameSpecInfo, true)=" + nRet);
            #endregion

            returnValue = myCamera.MV_CC_SetEnumValue_NET("TriggerSource", source == 1 ? (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0 : (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            if (MyCamera.MV_OK != returnValue)
                flag = false;

            return flag;
        }

        //3.设置浮点型型参数
        public bool SetExposureTime(uint ExposureTime)//设置曝光时间（us），成功返回0失败返回-1
        {
            int returnValue = myCamera.MV_CC_SetFloatValue_NET("ExposureTime", ExposureTime);
            if (MyCamera.MV_OK != returnValue)
                return false;
            return true;
        }

        public bool LoadFeature(string file)
        {
            int ret = myCamera.MV_CC_FeatureLoad_NET(file);
            if (MyCamera.MV_OK != ret)
                return false;
            return true;
        }

        public bool LoadUserSet(uint set = 1)
        {
            int ret = myCamera.MV_CC_SetEnumValue_NET("UserSetSelector", set);
            bool flag = ret == MyCamera.MV_OK;
            if (!flag)
            {
                Log.ErrorLog("Failed To Set UserSet " + idName);
                return false;
            }
            ret = myCamera.MV_CC_SetCommandValue_NET("UserSetLoad");
            flag = ret == MyCamera.MV_OK;
            if (!flag)
            {
                Log.ErrorLog("Failed To Load UserSet" + idName);
                return false;
            }
            return flag;
        }

        //4.判断是否为黑白图像
        private Boolean IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        public bool StartCamera()
        {
            var temp = myCamera.MV_CC_StartGrabbing_NET();
            if (temp != MyCamera.MV_OK)
            {
                Log.ErrorLog("Failed Starting Camera" + idName);
                return false;
            }
            MyCamera.MVCC_INTVALUE_EX stParam = new MyCamera.MVCC_INTVALUE_EX();
            myCamera.MV_CC_GetIntValueEx_NET("PayloadSize", ref stParam);
            NPayloadSize = stParam.nCurValue;
            StartGrab = temp == MyCamera.MV_OK;
            return StartGrab;
        }

        public bool StopCamera()//停止相机采集，返回0为成功，-1为失败
        {
            if (myCamera == null)
                return true;
            int temp = myCamera.MV_CC_StopGrabbing_NET();
            if (MyCamera.MV_OK != temp)
                Log.ErrorLog("Failed Stopping Camera " + idName);
            return MyCamera.MV_OK != temp;
        }

        public bool Capture()
        {
            bool flag;
            var count = 0;

            lock (IMAGECAPTURE)
                imageCaptured = false;
            lock (IMAGEBUFFER)
                imageBuffered = false;
            do
            {
                count++;
                flag = SoftTrigger();//trigger capture 
                if (!flag)
                {
                    Thread.Sleep(5);
                    Console.WriteLine("触发重试");
                }
            }
            while (!flag && count < 5);

            return flag;
        }

        private MyCamera.cbOutputExdelegate ImageCallback;
        private readonly object IMAGEBUFFER = new object();
        private readonly object IMAGECAPTURE = new object();
        private bool imageCaptured = false;
        private bool imageBuffered = false;
        private ImageBufData _bufferData;

        public bool WaitOnImageCapture()
        {
            var flag = true;
            lock (IMAGECAPTURE)
            {
                if (!imageCaptured)
                    flag = Monitor.Wait(IMAGECAPTURE, 2000);
                imageCaptured = false;
            }
            return flag;
        }

        public ImageBufData WaitOnImageDataGet()
        {
            //LOG
            lock (IMAGEBUFFER)
            {
                //log
                var flag = true;
                if (!imageBuffered)
                    flag = Monitor.Wait(IMAGEBUFFER, 2000);
                if (flag)
                {
                    var data = _bufferData;
                    imageBuffered = false;
                    _bufferData = null;
                    return data;
                }
                else
                    return null;
            }
        }

        public bool SetImageCallback()
        {
            ImageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
            var ret = myCamera.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
            if (ret == MyCamera.MV_OK)
                Log.AppLog("Set Image Callback " + idName);
            else
                Log.ErrorLog("Failed Setting Camera Image Callback " + idName);
            return ret == MyCamera.MV_OK;
        }

        private long NPayloadSize { get; set; }
        private void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            //Console.WriteLine(idName + " Enter ImageCallbackFunc!!!");
            lock (IMAGEBUFFER)
            {
                Console.WriteLine(idName + "Enter ImageCallbackFunc Copy Data!!!");
                byte[] buf = new byte[NPayloadSize];

                Marshal.Copy(pData, buf, 0, Convert.ToInt32(pFrameInfo.nFrameLen));

                int width = pFrameInfo.nWidth;
                int height = pFrameInfo.nHeight;

                _bufferData = new ImageBufData() { Data = buf, Width = width, Height = height };

                imageBuffered = true;
                Monitor.Pulse(IMAGEBUFFER);
            }
        }
    }
}
