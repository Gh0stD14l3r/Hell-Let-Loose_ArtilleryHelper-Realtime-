using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using IronOcr;
using IronOcr.Events;
using IronOcr.Exceptions;

namespace HLLArtillery
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public static string capturePath = @"capture.jpg";
        public static ImageCodecInfo myImageCodecInfo;
        public static Encoder myEncoder;
        public static EncoderParameter myEncoderParameter;
        public static EncoderParameters myEncoderParameters;

        static void Main(string[] args)
        {
            new Thread(() =>
            {
                while (true)
                {
                    Bitmap image = GetScreenshot();

                    if (File.Exists(capturePath))
                    {
                        File.Delete(capturePath);
                    }

                    myImageCodecInfo = GetEncoderInfo("image/jpeg");
                    myEncoder = Encoder.Quality;
                    myEncoderParameters = new EncoderParameters(1);

                    myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    image.Save(capturePath, myImageCodecInfo, myEncoderParameters);

                    IronTesseract OCRTesseract = new IronTesseract() { };
                    
                    var Result = OCRTesseract.Read(capturePath);
                    string path = "output.txt";
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        foreach (var line in Result.Lines)
                        {
                            if (line.ToString().Contains("ELEVATION") && line.ToString().Contains("MIL"))
                            {
                                Console.Clear();

                                string getMil = line.ToString().Substring(line.ToString().Length - 6);
                                getMil = CleanString(getMil.Substring(0, 3));

                                double dist = CalculateMIL2Dist(Convert.ToInt32(getMil));
                                Console.WriteLine($"{getMil} MIL - Distance: {dist}");
                            }
                        }
                    }
                    Thread.Sleep(500);
                }
                
            }).Start();

        }

        private static Bitmap GetScreenshot()
        {
            string procName = "HLL-Win64-Shipping";

            Process[] procList = Process.GetProcessesByName(procName);
            Process ProcHwnd = procList[0];
            
            IntPtr ptr = ProcHwnd.MainWindowHandle;

            Rect WindowBounds = new Rect();
            GetWindowRect(ptr, ref WindowBounds);

            int wArea = 250;
            int hArea = 250;
            int xArea = (WindowBounds.Right) - wArea;
            int yArea = (WindowBounds.Bottom) - hArea;

            Bitmap bm = new Bitmap(wArea, hArea);

            Graphics a = Graphics.FromImage(bm);
            a.CopyFromScreen(xArea, yArea, 0, 0, bm.Size);
            
            return bm;
        }

        private static double CalculateMIL2Dist (int MIL)
        {
            var m = -0.237035714285714;
            var b = 1001.46547619048;
            var xmin = 100;
            var xmax = 1600;

            for (int i = xmin; i < xmax; i++)
            {
                if (Math.Round(m * i + b) == MIL)
                {
                    return i;
                }
            }

            return 0;

            /*
            var m = -0.237035714285714;
            var b = 1001.46547619048;
            var xmin = 100;
            var xmax = 1600;
            var x = 100;
            var y = 978;

            function calculate()
            {
                var x = document.getElementById('distance').value;
                if (x >= 100 && x <= 1600)
                {
                    document.getElementById('elevation').value = Math.round(m * x + b);
                    document.getElementById('error').innerHTML = '';
                }
                else
                {
                    document.getElementById('elevation').value = String('ERROR');
                    document.getElementById('error').innerHTML = 'Enter a distance between 100 and 1600 meters!';
                };
            };
            // End --!>*/

        }

        private static string CleanString(string ToClean)
        {
            ToClean = ToClean.Replace("S", "5");
            ToClean = ToClean.Replace("O", "0");
            ToClean = ToClean.Replace("B", "8");
            ToClean = ToClean.Replace("A", "4");
            ToClean = ToClean.Replace("I", "1");

            return ToClean;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
