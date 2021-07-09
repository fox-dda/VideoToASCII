using System;
using System.Diagnostics;
using MyFunctions;
using Accord.Video.FFMPEG;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleDrawing
{
    class Program
    {
        static int consoleSize = 12;
        static Mutex videoMutex = new Mutex();
        static void Main(string[] args)
        {
            // Wait to start
            Console.Write($"Change terminal FONT to {consoleSize-1}, MAXIMIZE the terminal window and press ENTER to start frame loading:");
            Console.ReadLine();

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

            // Load frames
            Console.Clear();
            Console.WriteLine("Loading frames..");

            double frameRate = videoFile.FrameRate.ToDouble();
            int frameCount = (int)500;//videoFile.FrameCount;
            string[] frames = new string[frameCount];
            var frameLoadStopWatch = Stopwatch.StartNew();

            int frameBuildCounter = 0;
            Parallel.For(0, frameCount, (i) =>
            {
                videoMutex.WaitOne();
                frames[i] = F.BitmapToASCII(videoFile.ReadVideoFrame(i), consoleSize);
                frameBuildCounter++;
                videoMutex.ReleaseMutex();
                if (frameBuildCounter % 10 == 0)
                {
                    Console.SetCursorPosition(0, 1);
                    Console.Write("ASCII frame build progress: " + frameBuildCounter + "/" + frameCount);
                }
            });

            /*
            for(int i=0; i<frameCount; i++)
            {
                frames[i] = F.BitmapToASCII(videoFile.ReadVideoFrame(i), consoleSize);
                if(i % 10 == 0)
                {
                    Console.SetCursorPosition(0, 1);
                    Console.Write("ASCII frame build progress: " + i + "/" + frameCount);
                }
            }
            */

            videoFile.Close();

            frameLoadStopWatch.Stop();

            // Wait to render
            Console.Clear();
            Console.Write($"Frame loading complete! Load time: {frameLoadStopWatch.Elapsed.TotalMilliseconds / 1000.0} seconds\nPress ENTER to start rendering!");
            Console.ReadLine();

            // Console setup
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.CursorVisible = false;

            // Render
            F.RenderFrames(frames, frameRate, true);
        }
    }
}
