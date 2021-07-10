using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;
using MyFunctions;


namespace ConsoleDrawing
{
    class Program
    {
        static int consoleWidth;
        static int consoleHeight;
        const bool useParallel = true;
        const int threadCount = 4;
        const bool renderFullVideo = false;
        const int framesToRender = 500;

        static void Main(string[] args)
        {
            // Wait to start
            Console.Write("MAXIMIZE the terminal and press ENTER to start frame loading:");
            Console.ReadLine();

            // Set and get console information
            Console.CursorVisible = false;
            consoleWidth = Console.WindowWidth - 1;
            consoleHeight = Console.WindowHeight - 1;

            // Load video
            VideoFileReader videoFile = new VideoFileReader();
            try
            {
                videoFile.Open("VideoToRender.mp4");
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("File \"VideoToRender.mp4\" doesn't exist or couldn't be opened\nPress Enter to exit..");
                Console.ReadLine();
                return;
            }

            // Get video properties
            int frameCount = renderFullVideo ? (int)videoFile.FrameCount : framesToRender;
            string[] frames = new string[frameCount];
            double frameRate = videoFile.FrameRate.ToDouble();

            var frameLoadStopWatch = Stopwatch.StartNew();
            {
                // Load video frames to bitmaps
                Console.Clear();
                Console.WriteLine("Loading frames..");
                Bitmap[] bitmapArray = new Bitmap[frameCount];
                for (int i = 0; i < frameCount; i++)
                {
                    bitmapArray[i] = videoFile.ReadVideoFrame(i);
                }
                videoFile.Close();

                // Convert bitmaps to ASCII
                frameLoadStopWatch.Restart();
                if (useParallel)
                {
                    Parallel.For(0, frameCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, (i) =>
                    {
                        frames[i] = F.BitmapToASCII(bitmapArray[i], consoleWidth, consoleHeight);
                    });
                }
                else
                {
                    for (int i = 0; i < frameCount; i++)
                    {
                        frames[i] = F.BitmapToASCII(bitmapArray[i], consoleWidth, consoleHeight);
                    }
                }
                frameLoadStopWatch.Stop();
            }

            // Wait to render
            Console.Clear();
            Console.WriteLine($"Frame loading complete! Load time: {frameLoadStopWatch.Elapsed.TotalMilliseconds / 1000.0} seconds");
            Console.WriteLine("Press ENTER to start rendering:");
            Console.ReadLine();

            // Console setup
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            // Render
            F.RenderFrames(frames, frameRate, true);
        }
    }
}
