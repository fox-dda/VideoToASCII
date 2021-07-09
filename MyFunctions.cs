using System;
using System.Diagnostics;
using System.Drawing;


namespace MyFunctions
{
    public static class F
    {
        public static string BitmapToASCII(Bitmap image, int terminalFontSize)
        {
            // Calculate divisioner
            int divisioner = image.Width / (1280 / terminalFontSize);

            // Image to grayscaled values
            int[,] grayScaledPixels = new int[image.Height / divisioner + divisioner, image.Width / divisioner + divisioner];
            int iMax = image.Height - (divisioner / 2);
            int jMax = image.Width - (divisioner / 2);
            for (int i = 0; i < iMax; i += divisioner)
            {
                for (int j = 0; j < jMax; j += divisioner)
                {
                    System.Drawing.Color pixel = image.GetPixel(j + (divisioner / 2), i + (divisioner / 2));
                    grayScaledPixels[i / divisioner, j / divisioner] = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                }
            }

            // Grayscaled values to ASCII
            char[] asciiPixelTable = { '@', '%', '#', 'x', '+', '=', ':', '-', '.', ' ' };
            string frame = "";
            for (int i = 0; i < image.Height / divisioner; i++)
            { 
                for (int j = 0; j < image.Width / divisioner; j++)
                {
                    int saturationLevel = (int)((grayScaledPixels[i, j] / 255.0) * (asciiPixelTable.Length - 1));
                    frame += asciiPixelTable[saturationLevel] + "" + asciiPixelTable[saturationLevel];
                }
                frame += '\n';
            }
            return frame;
        }

        public static void RenderFrames(string[] frames, double fps, bool debug)
        {
            double timeToSleep = ((1.0 / fps) * 1000.0);

            Console.Clear();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < frames.Length; i++)
            {
                var tempSw = Stopwatch.StartNew();
                Console.SetCursorPosition(0, 0);
                Console.Write(frames[i]);
                while (tempSw.Elapsed.TotalMilliseconds >= timeToSleep)
                {
                    break;
                }
            }
            sw.Stop();

            // Debug
            if (debug)
            {
                Console.WriteLine("Average time per frame: " + (sw.Elapsed.TotalMilliseconds / frames.Length));
                Console.WriteLine("Desired time per frame: " + timeToSleep);
            }
        }
    }
}
