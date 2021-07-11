using System;
using System.Diagnostics;
using System.Drawing;


namespace MyFunctions
{
    public static class F
    {
        public static string BitmapToASCII(Bitmap loadedImage, int consoleWidth, int consoleHeight)
        {
            // Calculations
            int pixelWidthIncrement = loadedImage.Width / consoleWidth;
            int pixelHeightIncrement = loadedImage.Height / consoleHeight;

            // Pixels to grayscale
            int[,] grayScaledPixels = new int[consoleHeight, consoleWidth];
            for (int y = 0; y < consoleHeight; y++)
            {
                for (int x = 0; x < consoleWidth; x++)
                {
                    Color currentPixel = loadedImage.GetPixel(x * pixelWidthIncrement, y * pixelHeightIncrement);
                    grayScaledPixels[y, x] = (int)(currentPixel.R * 0.299 + currentPixel.G * 0.587 + currentPixel.B * 0.114);
                }
            }

            // Grayscaled values to ASCII
            char[] asciiPixelTable = { '@', '%', '#', 'x', '+', '=', ':', '-', '.', ' ' };
            string frame = "";
            for (int y = 0; y < consoleHeight; y++)
            {
                for (int x = 0; x < consoleWidth; x++)
                {
                    int saturationLevel = (int)((grayScaledPixels[y, x] / 255.0) * (asciiPixelTable.Length - 1));
                    frame += asciiPixelTable[saturationLevel].ToString();
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
