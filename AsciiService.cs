using System;
using System.Diagnostics;
using System.Drawing;

namespace VideoToAscii
{
    public static class AsciiService
    {
        private static char[] _shortAsciiPixelTable = { ' ', '@', '%', '#', 'x', '+', '=', ':', '-', '.', ' ' };
        private static char[] _longAsciiPixelTable = { '$','@','B','%','8','&','W','M','#','*','o','a','h','k','b','d','p',
                'q','w','m','Z','O','0','Q','L','C','J','U','Y','X','z','c','v','u','n','x','r','j','f','t',
                '/','\\','|','(',')','1','[',']','?','-','_','+','~','<','>','i','!','l','I',';',':',',',' ', ' ', ' ' };

        public static string BitmapToASCII(Bitmap loadedImage, int consoleWidth, int consoleHeight)
        {
            // Calculations
            int pixelWidthIncrement = loadedImage.Width / consoleWidth;
            int pixelHeightIncrement = loadedImage.Height / consoleHeight;

            // Image processing to ASCII

            string frame = string.Empty;
            for (int y = 0; y < consoleHeight; y++)
            {
                for (int x = 0; x < consoleWidth; x++)
                {
                    // Pixels to grayscale
                    Color currentPixel = loadedImage.GetPixel(x * pixelWidthIncrement, y * pixelHeightIncrement);
                    int grayScaledPixel = (int)(currentPixel.R * 0.299 + currentPixel.G * 0.587 + currentPixel.B * 0.114);

                    // Grayscaled values to ASCII
                    int saturationLevel = (int)((grayScaledPixel / 255.0) * (_shortAsciiPixelTable.Length - 1));
                    frame += _shortAsciiPixelTable[saturationLevel].ToString();
                }
                frame += '\n';
            }
            return frame;
        }

        public static void RenderFrames(string[] frames, double fps, bool debug)
        {
            // Calculate time to sleep
            double timeToSleep = ((1.0 / fps) * 1000.0);

            // Console setup
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.Clear();

            // Start the rendering
            var frameTimeSW = Stopwatch.StartNew();
            var totalTimeSW = Stopwatch.StartNew();
            for (int i = 0; i < frames.Length; i++)
            {
                frameTimeSW.Restart();
                Console.SetCursorPosition(0, 0);
                Console.Write(frames[i]);
                while (frameTimeSW.Elapsed.TotalMilliseconds < timeToSleep);
            }
            totalTimeSW.Stop();
            frameTimeSW.Stop();

            // Debug
            if (debug)
            {
                Console.WriteLine("Average time per frame: " + (totalTimeSW.Elapsed.TotalMilliseconds / frames.Length));
                Console.WriteLine("Desired time per frame: " + timeToSleep);
            }
        }
    }
}
