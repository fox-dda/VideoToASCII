using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;

namespace VideoToAscii
{
    class Program
    {
        static bool useParallel = true;

        static void Main(string[] args)
        {
            // Select video
            string videoName = Path.Combine(Directory.GetCurrentDirectory(), "video1.mp4");
            
            // Load video
            VideoFileReader videoFile = new VideoFileReader();
            try
            {
                videoFile.Open(videoName);
            }
            catch (Exception)
            {
                Console.WriteLine($"File \"{videoName}\" doesn't exist or couldn't be opened\nPress Enter to exit..");
                Console.ReadLine();
                return;
            }      

            // Wait for input to start frame loading and building
            Console.Clear();
           //Console.Write("Resize the terminal to desired size(can't be resized later) and press ENTER to start frame loading:");
            //Console.ReadLine();
            Console.CursorVisible = false;

            // Get video properties
            int frameCount = (int)videoFile.FrameCount;
            double frameRate = videoFile.FrameRate.ToDouble();
            double videoAspectRatio = videoFile.Width / (double)videoFile.Height;

            // Frame properties
            string[] frames = new string[frameCount];
            int frameHeight = Console.WindowHeight - 1;
            int frameWidth = (int)(Math.Round(frameHeight * videoAspectRatio) * 2);
            if(frameWidth > Console.WindowWidth - 1)
            {
                frameWidth = Console.WindowWidth - 1;
            }

            // Convert frames to ASCII
            Console.Clear();
            Console.WriteLine("Building frames..");
            var frameLoadSW = Stopwatch.StartNew();

            if (!useParallel)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    Bitmap tempBitmap = videoFile.ReadVideoFrame(i);
                    frames[i] = AsciiService.BitmapToASCII(tempBitmap, frameWidth, frameHeight);
                    tempBitmap.Dispose();
                    if (i % 10 == 0)
                    {
                        Console.SetCursorPosition(0, 1);
                        Console.Write($"Frame build progress {i}/{frameCount}");
                    }
                }
            }
            else
            {
                var bitmapLock = new object();
                var printLock = new object();
                int frameBuildCounter = 0;
                Parallel.For(0, frameCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (i) => 
                {
                    Bitmap tempBitmap;
                    int tempIndex;
                    lock (bitmapLock)
                    {
                        tempIndex = frameBuildCounter++;
                        tempBitmap = videoFile.ReadVideoFrame(tempIndex);
                    }

                    frames[tempIndex] = AsciiService.BitmapToASCII(tempBitmap, frameWidth, frameHeight);
                    tempBitmap.Dispose();
                    
                    if (tempIndex % 10 == 0)
                    {
                        lock (printLock)
                        {
                            Console.SetCursorPosition(0, 1);
                            Console.Write($"Frame build progress {tempIndex}/{frameCount}");
                        }
                    }
                });
            }
            
            frameLoadSW.Stop();
            videoFile.Close();

            // Wait for input to start rendering
            Console.Clear();
            Console.WriteLine($"Frame building done! Load time: {frameLoadSW.Elapsed.TotalMilliseconds / 1000.0} seconds");
            Thread.Sleep(500);
            //Console.WriteLine("Press ENTER to start rendering:");
            //Console.ReadLine();

            // Render with replays
            while (true)
            {
                AsciiService.RenderFrames(frames, frameRate, true);
                Console.Write("Press ENTER to replay!");
                Console.ReadLine();
                Console.Clear();
            }
        }
    }
}
