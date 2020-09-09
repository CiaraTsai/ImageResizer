using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string sourcePath = Path.Combine(Environment.CurrentDirectory, "images");
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "output"); ;

            ImageProcess imageProcess = new ImageProcess();

            imageProcess.Clean(destinationPath);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            imageProcess.ResizeImages(sourcePath, destinationPath, 2.0);
            sw.Stop();
            double oldts = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Sync Times: {oldts} ms");

            imageProcess.Clean(destinationPath);

            sw.Restart();
            await imageProcess.ResizeImagesAsync(sourcePath, destinationPath, 2.0);
            sw.Stop();
            
            double newts = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Async Times: {newts} ms");
            Console.WriteLine($"Improvement Ratio: {Math.Round(((oldts - newts) / oldts) * 100, 0)} %");
            
           
            Console.ReadKey();
        }
    }
}
