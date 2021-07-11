using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Video.FFMPEG;
using MyFunctions;


namespace ConsoleDrawing
{
    class Program
    {
        static bool useParallel = true;

        [STAThread]
        static void Main(string[] args)
        {
            // Select video
            string videoName = "";
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose Video File",
                InitialDirectory = Directory.GetCurrentDirectory(),
            };
            using (dialog)
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    videoName = dialog.FileName;
                }
            }
            dialog.Dispose();
            if(videoName == "")
            {
                return;
            }

            // Load video
            VideoFileReader videoFile = new VideoFileReader();
            try
            {
                videoFile.Open(videoName);
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine($"File \"{videoName}\" doesn't exist or couldn't be opened\nPress Enter to exit..");
                Console.ReadLine();
                return;
            }
            catch (Accord.Video.VideoException)
            {
                Console.WriteLine($"File \"{videoName}\" doesn't exist or couldn't be opened\nPress Enter to exit..");
                Console.ReadLine();
                return;
            }

            // Frame count information input
            bool renderFullVideo = false;
            int framesToRender = 0;
            do {
                Console.Clear();
                Console.Write("Render full video?(Y or N)");
                ConsoleKeyInfo inKey = Console.ReadKey();
                if(inKey.KeyChar == 'y' || inKey.KeyChar == 'Y')
                {
                    renderFullVideo = true;
                    break;
                }
                else if (inKey.KeyChar == 'n' || inKey.KeyChar == 'N')
                {
                    int tempFrameCount = 0;
                    do
                    {
                        Console.Clear();
                        Console.Write("Number of frames to render: ");
                        try
                        {
                            tempFrameCount = Convert.ToInt32(Console.ReadLine());
                        }
                        catch (FormatException)
                        {
                            tempFrameCount = 0;
                        }
                        catch (OverflowException)
                        {
                            tempFrameCount = 0;
                        }
                    } while (tempFrameCount < 1);
                    if(tempFrameCount >= videoFile.FrameCount)
                    {
                        renderFullVideo = true;
                    }
                    else
                    {
                        framesToRender = tempFrameCount;
                    }
                    break;
                }
            } while (true);

            // Wait for input to start frame loading and building
            Console.Clear();
            Console.Write("Resize the terminal to desired size(can't be resized later) and press ENTER to start frame loading:");
            Console.ReadLine();
            Console.CursorVisible = false;

            // Get video properties
            int frameCount = renderFullVideo ? (int)videoFile.FrameCount : framesToRender;
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
                    frames[i] = F.BitmapToASCII(tempBitmap, frameWidth, frameHeight);
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
                var bitmapLock = new Object();
                var printLock = new Object();
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

                    frames[tempIndex] = F.BitmapToASCII(tempBitmap, frameWidth, frameHeight);
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
            Console.WriteLine("Press ENTER to start rendering:");
            Console.ReadLine();

            // Render with replays
            while (true)
            {
                F.RenderFrames(frames, frameRate, true);
                Console.Write("Press ENTER to replay!");
                Console.ReadLine();
                Console.Clear();
            }
        }
    }
}
