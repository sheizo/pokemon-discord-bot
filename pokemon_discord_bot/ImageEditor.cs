#pragma warning disable CA1416 // Suppress platform compatibility warning

using pokemon_discord_bot.Data;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace pokemon_discord_bot
{
    public class ImageEditor
    {
        private const int EMBED_IMAGE_WIDTH = 500;
        private const int EMBED_IMAGE_HEIGHT = 250;
        private const int EMBED_IMAGE_PADDING = 10;
        private const int EMBED_IMAGE_FONT_SIZE = 14;

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

        public static async Task<Stream> GenerateEmbedImageAsync(string spriteUrl, Pokemon pokemon, float imageScaleFactor = 1.0f, float pokemonScaleFactor = 1.0f)
        {
            int resizedSprite = (int)(100 * pokemonScaleFactor);

            int pokemonBoxWidth = (EMBED_IMAGE_WIDTH / 2);
            int pokemonBoxHeight = EMBED_IMAGE_HEIGHT;
            int radius = 5;

            float yPos = EMBED_IMAGE_PADDING;

            //Create font for text
            FontFamily fontFamily = SystemFonts.Get("Cascadia Mono");
            Font font = fontFamily.CreateFont(EMBED_IMAGE_FONT_SIZE, FontStyle.Regular);
            Font boldFont = fontFamily.CreateFont(EMBED_IMAGE_FONT_SIZE * 1.5f, FontStyle.Bold);

            // Create blank image with background color
            using Image image = new Image<Rgba32>(EMBED_IMAGE_WIDTH, EMBED_IMAGE_HEIGHT, Color.ParseHex("#2F3136"));
            
            //Create Pokemon box
            var pokemonBox = RoundedRectangle(new PointF(pokemonBoxWidth, 0), pokemonBoxWidth - 1, pokemonBoxHeight - 1, radius);

            image.Mutate(img =>
            {
                //Pokemon box Background
                img.Fill(Color.Transparent, pokemonBox);
                //Pokemon box "frame" 
                img.Draw(Color.Yellow, 1f, pokemonBox);
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
                int spriteX = pokemonBoxWidth + (pokemonBoxWidth - resizedSprite) / 2;
                int spriteY = (pokemonBoxHeight - resizedSprite) / 2;

                image.Mutate(ctx => ctx.DrawImage(spriteImage, new Point(spriteX, spriteY), 1f));
            }

            //Store pokemon individual IV for later when drawing stat color -> IvColorGradient(pokemonStats[i])
            short[] pokemonStats =
            {
                pokemon.PokemonStats.IvHp,
                pokemon.PokemonStats.IvAtk,
                pokemon.PokemonStats.IvDef,
                pokemon.PokemonStats.IvSpAtk,
                pokemon.PokemonStats.IvSpDef,
                pokemon.PokemonStats.IvSpeed
            };

            //Store stat + description for drawing
            string[] statsDescription = 
            {
            $"{pokemonStats[0]} HP",
            $"{pokemonStats[1]} ATK",
            $"{pokemonStats[2]} DEF",
            $"{pokemonStats[3]} SPATK",
            $"{pokemonStats[4]} SPDEF",
            $"{pokemonStats[5]} SPEED"
            };

            // Draw pokemon stats 
            image.Mutate(ctx => ctx.DrawText("Stats:", boldFont, Color.White, new PointF(EMBED_IMAGE_PADDING + 4 * imageScaleFactor, yPos)));
            yPos += EMBED_IMAGE_FONT_SIZE * 2.0f;
            for (int i = 0; i < statsDescription.Length; i++)
            {
                image.Mutate(ctx => ctx.DrawText(statsDescription[i], font, IvColorGradient(pokemonStats[i]), new PointF(EMBED_IMAGE_PADDING + 4 * imageScaleFactor, yPos)));
                if (i + 1 < statsDescription.Length) yPos += EMBED_IMAGE_FONT_SIZE * 1.2f;
                else yPos += EMBED_IMAGE_FONT_SIZE * 2.5f;
            }

            image.Mutate(ctx => ctx.DrawText($"SIZE: {pokemon.PokemonStats.Size}", font, Color.Yellow, new PointF(EMBED_IMAGE_PADDING + 4 * imageScaleFactor, yPos)));
            yPos += EMBED_IMAGE_FONT_SIZE * 1.2f;

            // Save to stream (PNG format)
            var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);
            stream.Position = 0;
            return stream;
        }

        private static Color IvColorGradient(short ivValue)
        {
            int minIv = 0;
            int maxIv = 31;

            ivValue = (short) Math.Clamp(ivValue, minIv, maxIv);

            float normalizedIv = (float) ivValue / maxIv;

            byte r = (byte)(255 * (1 - normalizedIv));
            byte g = (byte)(255 * normalizedIv);
            byte b = 0;

            return Color.FromRgb(r, g, b);
        }

        private static IPath RoundedRectangle(PointF origin, int width, int height, int r)
        {
            IPath roundedRectangle = new PathBuilder()
            .SetOrigin(origin)// optional
            .AddLine(r, 0, width - r, 0) // Top line
            .AddArc(width - r, r, r, r, 0, 270, 90) //Top Right arc
            .AddLine(width, r, width, height - r) // Right Line
            .AddArc(width - r, height - r, r, r, 0, 0, 90) // Bottom right arc
            .AddLine(width - r, height, r, height) // Bot line
            .AddArc(r, height - r, r, r, 0, 90, 90) //Bot left
            .AddLine(0, height - r, 0, r) // Left line
            .AddArc(r, r, r, r, 0, 180, 90) //Bot right
            .CloseFigure()
            .Build();

            return roundedRectangle;
        }
    }
}
