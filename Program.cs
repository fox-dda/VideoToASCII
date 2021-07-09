using System;
using System.Diagnostics;
using MyFunctions;
using Accord.Video.FFMPEG;

namespace ConsoleDrawing
{
    class Program
    {
        static int frameCount;
        static int frameCounter;
        static int framerate;

        static void Main(string[] args)
        {
            // Wait to start
            Console.Write("Change terminal FONT to 11, MAXIMIZE the terminal window and press ENTER to start frame loading:");
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
            }

            // Load frames
            string[] frames = new string[frameCount];
            var frameLoadStopWatch = Stopwatch.StartNew();

            Console.Clear();
            Console.WriteLine("Loading frames..");
            

            frameLoadStopWatch.Stop();

            // Wait to render
            Console.Clear();
            Console.Write($"Frame loading complete! Load time: {frameLoadStopWatch.Elapsed.Seconds} seconds\nPress ENTER to start rendering!");
            Console.ReadLine();

            // Console setup
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.CursorVisible = false;

            // Render
            F.RenderFrames(frames, framerate, true);
        }
    }
}
