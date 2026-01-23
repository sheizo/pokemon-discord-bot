#pragma warning disable CA1416 // Suppress platform compatibility warning

using pokemon_discord_bot.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace pokemon_discord_bot
{
    public class ImageEditor
    {
        private const int EMBED_IMAGE_WIDTH = 350;
        private const int EMBED_IMAGE_HEIGHT = 350;
        private const int POKEMON_FRAME_WIDTH = 300;
        private const int POKEMON_FRAME_HEIGHT = 300;

        public static async Task<byte[]> CombineImagesAsync(List<string> imageUrls, float scaleFactor = 1.0f)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                return Array.Empty<byte>();

            if (imageUrls.Count > 10)
                throw new ArgumentException("Maximum of 10 images allowed.");

            int totalWidth = 0;
            int maxHeight = 0;
            const int gap = 10; // Pixel gap between images (fixed, not scaled)

            Image<Rgba32>[] bitmaps = new Image<Rgba32>[imageUrls.Count];

            try
            {
                for (int i = 0; i < imageUrls.Count; i++)
                {
                    Image<Rgba32> bitmap = await SingleImageBitmapAsync(imageUrls[i], scaleFactor);
                    totalWidth += bitmap.Width;

                    if (bitmap.Height > maxHeight)
                    {
                        maxHeight = bitmap.Height;
                    }

                    bitmaps[i] = bitmap;
                }

                totalWidth += gap * (imageUrls.Count - 1);

                using Image<Rgba32> combinedBitmap = new Image<Rgba32>(totalWidth, maxHeight);

                int currentX = 0;

                combinedBitmap.Mutate(ctx =>
                {
                    for (int i = 0; i < bitmaps.Length; i++)
                    {
                        ctx.DrawImage(
                            bitmaps[i],
                            new Point(currentX, 0),
                            1f);

                        currentX += bitmaps[i].Width + gap;
                    }
                });

                await using var outputStream = new MemoryStream();
                var imageEncoder = combinedBitmap.Configuration.ImageFormatsManager.GetEncoder(PngFormat.Instance);

                combinedBitmap.Save(outputStream, imageEncoder);
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing images: {ex.Message}", ex);
            }
        }

        private static async Task<Image<Rgba32>> SingleImageBitmapAsync(string imageUrl, float scaleFactor = 1.0f)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            if (scaleFactor <= 0)
                throw new ArgumentException("Scale factor must be greater than 0.");

            try
            {
                // Download the images asynchronously
                using var httpClient = new HttpClient();

                byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                await using var inputStream = new MemoryStream(imageBytes);

                Image<Rgba32> bitmap = Image.Load<Rgba32>(inputStream);

                int newWidth = (int)(bitmap.Width * scaleFactor);
                int newHeight = (int)(bitmap.Height * scaleFactor);

                Image<Rgba32> resizedBitmap = bitmap.Clone(ctx =>
                {
                    ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(newWidth, newHeight),
                        Sampler = KnownResamplers.NearestNeighbor
                    });
                });

                bitmap.Dispose(); // Dispose original after resizing
                return resizedBitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error combining images: {ex.Message}", ex);
            }
        }

        public static async Task<byte[]> GetUrlImageBytesAsync(string imageUrl, float scaleFactor = 1.0f)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return Array.Empty<byte>();

            Image<Rgba32> bitmap = await SingleImageBitmapAsync(imageUrl, scaleFactor);

            if (bitmap == null) return Array.Empty<byte>();

            await using var outputStream = new MemoryStream();
            var imageEncoder = bitmap.Configuration.ImageFormatsManager.GetEncoder(PngFormat.Instance);

            bitmap.Save(outputStream, imageEncoder);
            return outputStream.ToArray();
        }

        public static async Task<Stream> GeneratePokemonWithFrame(string spriteUrl, string? framePath, Pokemon pokemon, float pokemonScaleFactor = 1.0f)
        {
            int resizedSprite = (int)(100 * pokemonScaleFactor);

            // Create blank image with transparent background
            using Image image = new Image<Rgba32>(EMBED_IMAGE_WIDTH, EMBED_IMAGE_HEIGHT, Color.Transparent);

            if (!File.Exists(framePath))
                framePath = "assets/frames/default_frame.png";

            using Image frameImage = Image.Load(framePath);

            image.Mutate(img =>
            {
                //Pokemon box "frame"
                img.DrawImage(frameImage, new Point((EMBED_IMAGE_WIDTH - POKEMON_FRAME_WIDTH) / 2, (EMBED_IMAGE_HEIGHT - POKEMON_FRAME_HEIGHT) / 2), 1f);
            });

            // Download, mutate and draw sprite
            if (!string.IsNullOrEmpty(spriteUrl))
            {
                using var httpClient = new HttpClient();

                var spriteBytes = await httpClient.GetByteArrayAsync(spriteUrl);
                using var spriteImage = Image.Load(spriteBytes);

                // Resize sprite
                spriteImage.Mutate(ctx => 
                {
                    ctx.Resize(new ResizeOptions
                    {
                        Size = new Size(resizedSprite, resizedSprite),
                        Sampler = KnownResamplers.NearestNeighbor
                    });
                });

                //Center sprite in pokemon box
                int spriteX = (EMBED_IMAGE_WIDTH - resizedSprite) / 2;
                int spriteY = (EMBED_IMAGE_HEIGHT - resizedSprite) / 2;

                image.Mutate(ctx => ctx.DrawImage(spriteImage, new Point(spriteX, spriteY), 1f));
            }
            
            // Save to stream (PNG format)
            var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
