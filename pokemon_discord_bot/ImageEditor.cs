#pragma warning disable CA1416 // Suppress platform compatibility warning

using System.Drawing;
using System.Drawing.Imaging;

namespace pokemon_discord_bot
{
    public class ImageEditor
    {

        public static async Task<byte[]> CombineImagesAsync(string[] imageUrls, float scaleFactor = 1.0f)
        {
            if (imageUrls == null || imageUrls.Length == 0)
                return Array.Empty<byte>();

            if (imageUrls.Length > 10)
                throw new ArgumentException("Maximum of 10 images allowed.");

            if (scaleFactor <= 0)
                throw new ArgumentException("Scale factor must be greater than 0.");

            try
            {
                // Download the images asynchronously
                using var httpClient = new HttpClient();
                Bitmap[] bitmaps = new Bitmap[imageUrls.Length];

                for (int i = 0; i < imageUrls.Length; i++)
                {
                    byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrls[i]);
                    await using var inputStream = new MemoryStream(imageBytes);
                    bitmaps[i] = new Bitmap(inputStream);
                }

                // Resize each bitmap if scaleFactor != 1
                Bitmap[] resizedBitmaps = new Bitmap[bitmaps.Length];
                for (int i = 0; i < bitmaps.Length; i++)
                {
                    if (Math.Abs(scaleFactor - 1.0f) < 0.001) // Close to 1, no resize
                    {
                        resizedBitmaps[i] = bitmaps[i];
                    }
                    else
                    {
                        int newWidth = (int)(bitmaps[i].Width * scaleFactor);
                        int newHeight = (int)(bitmaps[i].Height * scaleFactor);
                        resizedBitmaps[i] = new Bitmap(newWidth, newHeight);
                        using (Graphics g = Graphics.FromImage(resizedBitmaps[i]))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                            g.DrawImage(bitmaps[i], 0, 0, newWidth, newHeight);
                        }
                        bitmaps[i].Dispose(); // Dispose original after resizing
                    }
                }

                // Calculate dimensions for the combined image
                int totalWidth = 0;
                int maxHeight = 0;
                const int gap = 10; // Pixel gap between images (fixed, not scaled)

                for (int i = 0; i < resizedBitmaps.Length; i++)
                {
                    totalWidth += resizedBitmaps[i].Width;
                    if (resizedBitmaps[i].Height > maxHeight)
                        maxHeight = resizedBitmaps[i].Height;
                }
                totalWidth += gap * (resizedBitmaps.Length - 1); // Add gaps between images (none if only 1)

                // Create a new Bitmap for the combined image
                using Bitmap combinedBitmap = new Bitmap(totalWidth, maxHeight, PixelFormat.Format32bppArgb); // For transparency support

                // Draw each resized image side by side with gaps
                using (Graphics graphics = Graphics.FromImage(combinedBitmap))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

                    // Clear with transparent background
                    graphics.Clear(Color.Transparent);

                    int currentX = 0;
                    for (int i = 0; i < resizedBitmaps.Length; i++)
                    {
                        graphics.DrawImage(resizedBitmaps[i], currentX, 0, resizedBitmaps[i].Width, resizedBitmaps[i].Height);
                        currentX += resizedBitmaps[i].Width + gap;
                        resizedBitmaps[i].Dispose(); // Dispose after drawing
                    }
                }

                // Convert combined Bitmap to byte array in memory (PNG for transparency)
                await using var outputStream = new MemoryStream();
                combinedBitmap.Save(outputStream, ImageFormat.Png);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error combining images: {ex.Message}", ex);
            }
        }

    }
}
