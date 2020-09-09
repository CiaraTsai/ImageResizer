using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        
        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        #region Sync
        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public void ResizeImages(string sourcePath, string destPath, double scale)
        {
            var allFiles = FindImages(sourcePath);
            foreach (var filePath in allFiles)
            {
                Image imgPhoto = Image.FromFile(filePath);
                string imgName = Path.GetFileNameWithoutExtension(filePath);

                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale);
                int destionatonHeight = (int)(sourceHeight * scale);
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                Bitmap processedImage = processBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);
                //sw.Stop();
                //Console.WriteLine($"ThreadID[{Thread.CurrentThread.ManagedThreadId }]");
                string destFile = Path.Combine(destPath, imgName + ".jpg");
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                processedImage.Save(destFile, ImageFormat.Jpeg);
                //sw.Stop();
                //Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            }
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap processBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }
        #endregion

        #region Async
        public async Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            
            var allFiles = FindImages(sourcePath);

            Task[] taskArray = new Task[allFiles.Count];

            int index = 0;
            foreach (var filePath in allFiles)
            {
                taskArray[index] = Task.Run(async () =>
                {
                    Image imgPhoto = Image.FromFile(filePath);

                    string imgName = Path.GetFileNameWithoutExtension(filePath);
                    string destFile = Path.Combine(destPath, imgName + ".jpg");

                    int sourceWidth = imgPhoto.Width;
                    int sourceHeight = imgPhoto.Height;

                    int destionatonWidth = (int)(sourceWidth * scale);
                    int destionatonHeight = (int)(sourceHeight * scale);

                    var processedImage = await processBitmapAsync((Bitmap)imgPhoto,
                            sourceWidth, sourceHeight,
                            destionatonWidth, destionatonHeight);

                    await Task.Run(() => { processedImage.Save(destFile, ImageFormat.Jpeg); });
                });
                index++;
            }

            await Task.WhenAll(taskArray);
        }

        async Task<Bitmap> processBitmapAsync(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            var resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);

            await Task.Run(() => {
                g.DrawImage(img,
                    new Rectangle(0, 0, newWidth, newHeight),
                    new Rectangle(0, 0, srcWidth, srcHeight),
                    GraphicsUnit.Pixel);

            });

            return resizedbitmap;
        }
        #endregion
    }
}
