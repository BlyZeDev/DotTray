namespace DotTray.Direct.Imaging;

using DotTray.Direct.Coloring;
using DotTray.Direct.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a mutable image that can be edited and turned into a <see cref="DirectImage"/>
/// </summary>
public sealed class DirectCanvas
{
    private const int ParallelThreshold = 4_194_304;
    private const int TileSize = 32;
    private const int MaxBlurRadius = 4096;

    private static readonly byte[] Unpremultiplied = BuildUnpremultiplyTable();

    private readonly Lock _lock;

    private byte[] pixels;
    private int width;
    private int height;

    /// <summary>
    /// The dimensions of the canvas, in pixels
    /// </summary>
    public Dim Dimensions
    {
        get
        {
            lock (_lock)
            {
                return new Dim(width, height);
            }
        }
    }

    internal DirectCanvas(byte[] pixels, int width, int height)
    {
        _lock = new Lock();

        this.pixels = pixels;
        this.width = width;
        this.height = height;
    }

    /// <summary>
    /// Creates an independent copy of this canvas
    /// </summary>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Clone()
    {
        lock (_lock)
        {
            return new DirectCanvas(Copy(pixels), width, height);
        }
    }

    /// <summary>
    /// Gets the color of a single pixel
    /// </summary>
    /// <param name="coordinates">The coordinates</param>
    /// <returns><see cref="Rgba"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Rgba GetPixel(Pos coordinates)
    {
        lock (_lock)
        {
            return Unpack(AsPixels(pixels)[IndexOf(coordinates.X, coordinates.Y)]);
        }
    }

    /// <summary>
    /// Replaces the color of a single pixel
    /// </summary>
    /// <param name="coordinates">The coordinates</param>
    /// <param name="color">The new color</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas SetPixel(Pos coordinates, Rgba color)
    {
        lock (_lock)
        {
            AsPixels(pixels)[IndexOf(coordinates.X, coordinates.Y)] = Pack(color);
            return this;
        }
    }

    /// <summary>
    /// Replaces every pixel with <paramref name="color"/>, without blending
    /// </summary>
    /// <param name="color">The color to fill with</param>
    /// <param name="region">The part to fill, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Fill(Rgba color, Rect? region = null)
    {
        var value = Pack(color);

        return Apply(region, row => row.Fill(value));
    }

    /// <summary>
    /// Makes every pixel fully transparent
    /// </summary>
    /// <param name="region">The part to clear, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Clear(Rect? region = null) => Fill(Rgba.Transparent, region);

    /// <summary>
    /// Replaces every pixel with the result of <paramref name="transform"/>
    /// </summary>
    /// <remarks>
    /// The callback works with straight (non-premultiplied) colors and is always called sequentially on the calling thread.
    /// For common edits prefer the dedicated methods, they are several times faster
    /// </remarks>
    /// <param name="transform">The function that maps a pixel color to its new color</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Map(Func<Rgba, Rgba> transform, Rect? region = null)
    {
        ArgumentNullException.ThrowIfNull(transform);

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = Pack(transform(Unpack(row[i])));
            }
        }, false);
    }

    /// <summary>
    /// Replaces every pixel close to <paramref name="from"/> with <paramref name="to"/>
    /// </summary>
    /// <param name="from">The color to replace</param>
    /// <param name="to">The replacement color, its alpha is multiplied with the alpha of the replaced pixel</param>
    /// <param name="tolerance">How far each channel may differ from <paramref name="from"/>, or <see langword="null"/> to use <see cref="Rgba.Transparent"/> for an exact match</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas ReplaceColor(Rgba from, Rgba to, Rgba? tolerance = null, Rect? region = null) => Apply(region, row =>
    {
        var actualTolerance = tolerance ?? Rgba.Transparent;

        for (var i = 0; i < row.Length; i++)
        {
            var color = Unpack(row[i]);

            if (color.A == 0
                || Math.Abs(color.R - from.R) > actualTolerance.R
                || Math.Abs(color.G - from.G) > actualTolerance.G
                || Math.Abs(color.B - from.B) > actualTolerance.B
                || Math.Abs(color.A - from.A) > actualTolerance.A) continue;

            row[i] = Pack(to with { A = (byte)Mul255(color.A, to.A) });
        }
    });

    /// <summary>
    /// Replaces the color of every pixel with <paramref name="color"/> but keeps its transparency
    /// </summary>
    /// <param name="color">The color to tint with, its alpha is multiplied with the alpha of each pixel</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Recolor(Rgba color, Rect? region = null)
    {
        var tint = Pack(color);

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = Scale(tint, row[i] >>> 24);
            }
        });
    }

    /// <summary>
    /// Paints <paramref name="color"/> over the image like a filter, which is the same as <see cref="ImageOptions.Tint"/> but permanent
    /// </summary>
    /// <remarks>
    /// The alpha of <paramref name="color"/> is the strength, so a red color with alpha 100 gives a light red wash.
    /// Transparent parts stay transparent, the alpha of the pixels never changes
    /// </remarks>
    /// <param name="color">The color to paint over the image</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Tint(Rgba color, Rect? region = null)
    {
        if (color.A == 0) return this;

        var layer = Pack(color with { A = 255 });
        var strength = (uint)color.A;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];
                var a = p >>> 24;

                if (a != 0) row[i] = Scale(p, 255 - strength) + Scale(layer, Mul255(a, strength));
            }
        });
    }

    /// <summary>
    /// Converts the colors to shades of gray
    /// </summary>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Grayscale(Rect? region = null) => Apply(region, row =>
    {
        for (var i = 0; i < row.Length; i++)
        {
            var p = row[i];
            var luma = (54 * ((p >>> 16) & 0xFF) + 183 * ((p >>> 8) & 0xFF) + 19 * (p & 0xFF)) >>> 8;

            row[i] = (p & 0xFF000000u) | (luma * 0x010101u);
        }
    });

    /// <summary>
    /// Inverts the colors but keeps the transparency
    /// </summary>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Invert(Rect? region = null) => Apply(region, row =>
    {
        for (var i = 0; i < row.Length; i++)
        {
            var p = row[i];
            var a = p >>> 24;

            row[i] = (a << 24) | ((a * 0x010101u) - (p & 0x00FFFFFFu));
        }
    });

    /// <summary>
    /// Multiplies the opacity of every pixel with <paramref name="factor"/>
    /// </summary>
    /// <param name="factor">The factor from <c>0f</c> (invisible) to <c>1f</c> (unchanged)</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Opacity(float factor, Rect? region = null)
    {
        var bFactor = ToByteFactor(factor);
        if (bFactor == 255) return this;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++) row[i] = Scale(row[i], bFactor);
        });
    }

    /// <summary>
    /// Multiplies the brightness of every pixel with <paramref name="factor"/>
    /// </summary>
    /// <param name="factor">The factor, <c>1f</c> is unchanged, lower is darker, higher is brighter</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas Brightness(float factor, Rect? region = null)
    {
        if (!float.IsFinite(factor) || factor < 0f) throw new ArgumentOutOfRangeException(nameof(factor));
        if (factor == 1f) return this;

        var fixedPoint = (uint)MathF.Round(MathF.Min(factor, 255f) * 256f);

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];
                var a = p >>> 24;

                var r = Math.Min(a, ((((p >>> 16) & 0xFF) * fixedPoint) + 128) >>> 8);
                var g = Math.Min(a, ((((p >>> 8) & 0xFF) * fixedPoint) + 128) >>> 8);
                var b = Math.Min(a, (((p & 0xFF) * fixedPoint) + 128) >>> 8);

                row[i] = (a << 24) | (r << 16) | (g << 8) | b;
            }
        });
    }

    /// <summary>
    /// Changes the difference between dark and bright colors
    /// </summary>
    /// <param name="factor">The factor, <c>1f</c> is unchanged, lower is flatter, higher has more contrast</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas Contrast(float factor, Rect? region = null)
    {
        var fixedPoint = (int)ToFixedPoint(factor);
        if (fixedPoint == 256) return this;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];
                var a = (int)(p >>> 24);
                var middle = (a * 128 + 127) / 255;

                var r = Math.Clamp(middle + ((((int)((p >>> 16) & 0xFF) - middle) * fixedPoint + 128) >> 8), 0, a);
                var g = Math.Clamp(middle + ((((int)((p >>> 8) & 0xFF) - middle) * fixedPoint + 128) >> 8), 0, a);
                var b = Math.Clamp(middle + ((((int)(p & 0xFF) - middle) * fixedPoint + 128) >> 8), 0, a);

                row[i] = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
            }
        });
    }

    /// <summary>
    /// Changes how colorful the image is
    /// </summary>
    /// <param name="factor">The factor, <c>1f</c> is unchanged, <c>0f</c> is grayscale, higher is more intense</param>
    /// <param name="region">The part to change, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas Saturation(float factor, Rect? region = null)
    {
        var fixedPoint = (int)ToFixedPoint(factor);
        if (fixedPoint == 256) return this;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];
                var a = (int)(p >>> 24);
                var r = (int)((p >>> 16) & 0xFF);
                var g = (int)((p >>> 8) & 0xFF);
                var b = (int)(p & 0xFF);
                var gray = (54 * r + 183 * g + 19 * b) >> 8;

                r = Math.Clamp(gray + (((r - gray) * fixedPoint + 128) >> 8), 0, a);
                g = Math.Clamp(gray + (((g - gray) * fixedPoint + 128) >> 8), 0, a);
                b = Math.Clamp(gray + (((b - gray) * fixedPoint + 128) >> 8), 0, a);

                row[i] = ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
            }
        });
    }

    /// <summary>
    /// Blurs the canvas
    /// </summary>
    /// <param name="radius">How many pixels each pixel is spread to every side, from <c>0</c> (unchanged) to <c>4096</c></param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas Blur(int radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(radius);
        if (radius == 0) return this;

        radius = Math.Min(radius, MaxBlurRadius);

        lock (_lock)
        {
            var current = AsPixels(pixels);
            var buffer = new uint[current.Length];

            for (var pass = 0; pass < 3; pass++)
            {
                for (var y = 0; y < height; y++)
                {
                    BoxBlurLine(current, buffer, y * width, 1, width, radius);
                }

                for (var x = 0; x < width; x++)
                {
                    BoxBlurLine(buffer, current, x, width, height, radius);
                }
            }

            return this;
        }
    }

    /// <summary>
    /// Mirrors the canvas from left to right
    /// </summary>
    /// <param name="region">The part to mirror inside itself, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas FlipHorizontal(Rect? region = null) => Apply(region, row => row.Reverse());

    /// <summary>
    /// Mirrors the canvas from top to bottom
    /// </summary>
    /// <param name="region">The part to mirror inside itself, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas FlipVertical(Rect? region = null)
    {
        lock (_lock)
        {
            if (!Clip(width, height, region, out var area)) return this;

            var buffer = new uint[area.Width];

            for (int top = area.Top, bottom = area.Bottom - 1; top < bottom; top++, bottom--)
            {
                var upper = Row(pixels, width, area.X, top, area.Width);
                var lower = Row(pixels, width, area.X, bottom, area.Width);

                upper.CopyTo(buffer);
                lower.CopyTo(upper);
                buffer.AsSpan().CopyTo(lower);
            }

            return this;
        }
    }

    /// <summary>
    /// Rotates the canvas by 90 degrees clockwise, which swaps the width and height
    /// </summary>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Rotate90() => Transpose(true);

    /// <summary>
    /// Rotates the canvas by 180 degrees
    /// </summary>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Rotate180()
    {
        lock (_lock)
        {
            AsPixels(pixels).Reverse();
            return this;
        }
    }

    /// <summary>
    /// Rotates the canvas by 270 degrees clockwise (90 degrees counterclockwise), which swaps the width and height
    /// </summary>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Rotate270() => Transpose(false);

    /// <summary>
    /// Cuts the canvas down to <paramref name="area"/>
    /// </summary>
    /// <param name="area">The part to keep, parts outside the canvas are ignored</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public DirectCanvas Crop(Rect area)
    {
        lock (_lock)
        {
            if (!Clip(width, height, area, out var clipped)) throw new ArgumentException("The area does not overlap the canvas", nameof(area));

            pixels = CopyRegion(clipped);
            width = clipped.Width;
            height = clipped.Height;

            return this;
        }
    }

    /// <summary>
    /// Adds space around the canvas
    /// </summary>
    /// <param name="left">The space to add on the left, in pixels</param>
    /// <param name="top">The space to add on the top, in pixels</param>
    /// <param name="right">The space to add on the right, in pixels</param>
    /// <param name="bottom">The space to add on the bottom, in pixels</param>
    /// <param name="fillColor">The color of the new space, or <see langword="null"/> for fully transparent</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas Pad(int left, int top, int right, int bottom, Rgba? fillColor = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(left);
        ArgumentOutOfRangeException.ThrowIfNegative(top);
        ArgumentOutOfRangeException.ThrowIfNegative(right);
        ArgumentOutOfRangeException.ThrowIfNegative(bottom);

        lock (_lock)
        {
            var newWidth = (long)width + left + right;
            var newHeight = (long)height + top + bottom;
            ArgumentOutOfRangeException.ThrowIfGreaterThan(newWidth * newHeight * 4, Array.MaxLength, "padding");

            var padded = new byte[newWidth * newHeight * 4];
            if (fillColor.HasValue) AsPixels(padded).Fill(Pack(fillColor.Value));

            for (var y = 0; y < height; y++)
            {
                Row(pixels, width, 0, y, width).CopyTo(Row(padded, (int)newWidth, left, top + y, width));
            }

            pixels = padded;
            width = (int)newWidth;
            height = (int)newHeight;

            return this;
        }
    }

    /// <summary>
    /// Adds the same space on every side of the canvas
    /// </summary>
    /// <param name="padding">The space to add on every side, in pixels</param>
    /// <param name="fillColor">The color of the new space, or <see langword="null"/> for fully transparent</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas Pad(int padding, Rgba? fillColor = null) => Pad(padding, padding, padding, padding, fillColor);

    /// <summary>
    /// Makes the corners of the canvas transparent and round with smooth edges, with a separate radius for every corner
    /// </summary>
    /// <remarks>
    /// If the radii of two neighboring corners are larger than the side between them, all radii are scaled down by the same factor until they fit.<br/>
    /// A radius of <c>0</c> leaves the corner as it is
    /// </remarks>
    /// <param name="topLeft">The radius of the top left corner, in pixels</param>
    /// <param name="topRight">The radius of the top right corner, in pixels</param>
    /// <param name="bottomRight">The radius of the bottom right corner, in pixels</param>
    /// <param name="bottomLeft">The radius of the bottom left corner, in pixels</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas RoundCorners(float topLeft, float topRight, float bottomRight, float bottomLeft)
    {
        lock (_lock)
        {
            var max = (float)Math.Max(width, height);

            topLeft = Math.Clamp(float.IsNaN(topLeft) ? 0f : topLeft, 0f, max);
            topRight = Math.Clamp(float.IsNaN(topRight) ? 0f : topRight, 0f, max);
            bottomRight = Math.Clamp(float.IsNaN(bottomRight) ? 0f : bottomRight, 0f, max);
            bottomLeft = Math.Clamp(float.IsNaN(bottomLeft) ? 0f : bottomLeft, 0f, max);

            var factor = Math.Min(
                Math.Min(Fit(width, topLeft + topRight), Fit(width, bottomLeft + bottomRight)),
                Math.Min(Fit(height, topLeft + bottomLeft), Fit(height, topRight + bottomRight)));

            var data = AsPixels(pixels);

            RoundCorner(data, 0, 0, 1, 1, topLeft * factor);
            RoundCorner(data, width - 1, 0, -1, 1, topRight * factor);
            RoundCorner(data, width - 1, height - 1, -1, -1, bottomRight * factor);
            RoundCorner(data, 0, height - 1, 1, -1, bottomLeft * factor);

            return this;
        }

        static float Fit(int length, float sum) => sum > length ? length / sum : 1f;
    }

    /// <summary>
    /// Makes the corners of the canvas transparent and round with smooth edges
    /// </summary>
    /// <remarks>
    /// A radius of half the width or height or more makes the shortest side fully round, which gives a circle for a square canvas
    /// </remarks>
    /// <param name="radius">The radius of the corners, in pixels</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas RoundCorners(float radius) => RoundCorners(radius, radius, radius, radius);

    /// <summary>
    /// Draws <paramref name="source"/> onto this canvas, blending it over the existing pixels
    /// </summary>
    /// <param name="source">The image to draw</param>
    /// <param name="position">The position of the upper-left corner of the drawn part, parts outside the canvas are ignored</param>
    /// <param name="opacity">The opacity from <c>0f</c> (invisible) to <c>1f</c> (opaque)</param>
    /// <param name="sourceRegion">The part of <paramref name="source"/> to draw, or <see langword="null"/> for all of it</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Draw(DirectImage source, Pos position, float opacity = 1f, Rect? sourceRegion = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!Clip(source.Dimensions.Width, source.Dimensions.Height, sourceRegion, out var area)) return this;

        return Compose(source.Pixels, source.Dimensions.Width, area, position, opacity);
    }

    /// <summary>
    /// Draws <paramref name="source"/> onto this canvas, blending it over the existing pixels
    /// </summary>
    /// <remarks>
    /// The drawn part is copied first, so <paramref name="source"/> may be this canvas
    /// </remarks>
    /// <param name="source">The canvas to draw</param>
    /// <param name="position">The position of the upper-left corner of the drawn part, parts outside the canvas are ignored</param>
    /// <param name="opacity">The opacity from <c>0f</c> (invisible) to <c>1f</c> (opaque)</param>
    /// <param name="sourceRegion">The part of <paramref name="source"/> to draw, or <see langword="null"/> for all of it</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Draw(DirectCanvas source, Pos position, float opacity = 1f, Rect? sourceRegion = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        byte[] snapshot;
        Rect area;

        lock (source._lock)
        {
            if (!Clip(source.width, source.height, sourceRegion, out area)) return this;

            snapshot = source.CopyRegion(area);
        }

        return Compose(snapshot, area.Width, new Rect(0, 0, area.Width, area.Height), position, opacity);
    }

    /// <summary>
    /// Creates a <see cref="DirectImage"/> with a copy of the current pixels
    /// </summary>
    /// <remarks>
    /// Later edits of this canvas do not change the returned image
    /// </remarks>
    /// <returns><see cref="DirectImage"/></returns>
    public DirectImage ToImage()
    {
        lock (_lock)
        {
            return new DirectImage(Copy(pixels), width, height);
        }
    }

    private DirectCanvas Apply(Rect? region, RowAction action, bool parallel = true)
    {
        lock (_lock)
        {
            if (!Clip(width, height, region, out var area)) return this;

            var data = pixels;
            var stride = width;

            void Process(int i) => action(Row(data, stride, area.X, area.Y + i, area.Width));

            if (parallel) Run(area.Height, area.Width * area.Height, Process);
            else
            {
                for (var i = 0; i < area.Height; i++)
                {
                    Process(i);
                }
            }

            return this;
        }
    }

    private void RoundCorner(Span<uint> data, int originX, int originY, int directionX, int directionY, float radius)
    {
        var size = (int)MathF.Ceiling(radius);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = Math.Max(0f, radius - (x + 0.5f));
                var dy = Math.Max(0f, radius - (y + 0.5f));

                var coverage = Math.Clamp(radius - MathF.Sqrt(dx * dx + dy * dy) + 0.5f, 0f, 1f);
                if (coverage >= 1f) continue;

                var index = (originY + y * directionY) * width + originX + x * directionX;
                data[index] = Scale(data[index], (uint)MathF.Round(coverage * 255f));
            }
        }
    }

    private DirectCanvas Compose(byte[] source, int sourceStride, Rect sourceArea, Pos position, float opacity)
    {
        var factor = ToByteFactor(opacity);
        if (factor == 0) return this;

        lock (_lock)
        {
            var left = Math.Max(0, position.X);
            var top = Math.Max(0, position.Y);
            var right = Math.Min(width, position.X + sourceArea.Width);
            var bottom = Math.Min(height, position.Y + sourceArea.Height);

            if (right <= left || bottom <= top) return this;

            var data = pixels;
            var stride = width;
            var count = right - left;
            var sourceX = sourceArea.X + (left - position.X);
            var sourceY = sourceArea.Y + (top - position.Y);

            Run(bottom - top,
                count * (bottom - top),
                i => BlendRow(Row(data, stride, left, top + i, count), Row(source, sourceStride, sourceX, sourceY + i, count), factor));

            return this;
        }
    }

    private DirectCanvas Transpose(bool clockwise)
    {
        lock (_lock)
        {
            var rotated = new byte[pixels.Length];
            var src = AsPixels(pixels);
            var dst = AsPixels(rotated);

            for (var tileY = 0; tileY < height; tileY += TileSize)
            {
                for (var tileX = 0; tileX < width; tileX += TileSize)
                {
                    for (var y = tileY; y < Math.Min(height, tileY + TileSize); y++)
                    {
                        for (var x = tileX; x < Math.Min(width, tileX + TileSize); x++)
                        {
                            dst[clockwise ? x * height + (height - 1 - y) : (width - 1 - x) * height + y] = src[y * width + x];
                        }
                    }
                }
            }

            pixels = rotated;
            (width, height) = (height, width);

            return this;
        }
    }

    private byte[] CopyRegion(Rect area)
    {
        var copy = GC.AllocateUninitializedArray<byte>(area.Width * area.Height * 4);

        for (var y = 0; y < area.Height; y++)
        {
            Row(pixels, width, area.X, area.Y + y, area.Width).CopyTo(Row(copy, area.Width, 0, y, area.Width));
        }

        return copy;
    }

    private int IndexOf(int x, int y)
    {
        if ((uint)x >= (uint)width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)height) throw new ArgumentOutOfRangeException(nameof(y));

        return y * width + x;
    }

    /// <summary>
    /// Creates a new canvas filled with <paramref name="fillColor"/>
    /// </summary>
    /// <param name="width">The width in pixels</param>
    /// <param name="height">The height in pixels</param>
    /// <param name="fillColor">The color to fill the canvas with, or <see langword="null"/> for fully transparent</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static DirectCanvas CreateBlank(int width, int height, Rgba? fillColor = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var size = (long)width * height * 4;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, Array.MaxLength, nameof(width));

        var canvas = new DirectCanvas(new byte[size], width, height);
        if (fillColor.HasValue) canvas.Fill(fillColor.Value);

        return canvas;
    }

    /// <summary>
    /// Creates a new canvas with a copy of the pixels of <paramref name="image"/>
    /// </summary>
    /// <param name="image">The image to copy</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    public static DirectCanvas FromImage(DirectImage image) => new DirectCanvas(Copy(image.Pixels), image.Dimensions.Width, image.Dimensions.Height);

    /// <summary>
    /// Loads a canvas from <paramref name="path"/>
    /// </summary>
    /// <remarks>
    /// Supports every format Windows can decode, such as PNG, JPEG, BMP, GIF (first frame), TIFF and ICO
    /// </remarks>
    /// <param name="path">The path to the image file</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public static DirectCanvas FromFile(string path)
    {
        var pixels = DirectImage.LoadPixels(path, out var width, out var height);
        return new DirectCanvas(pixels, width, height);
    }

    /// <summary>
    /// Tries to load a canvas from <paramref name="path"/>
    /// </summary>
    /// <param name="path">The path to the image file</param>
    /// <param name="canvas">The canvas or <see langword="null"/> if it could not be created</param>
    /// <returns><see cref="bool"/></returns>
    public static bool TryFromFile(string path, [NotNullWhen(true)] out DirectCanvas? canvas)
    {
        try
        {
            canvas = FromFile(path);
            return true;
        }
        catch (Exception)
        {
            canvas = null;
            return false;
        }
    }

    private static void BoxBlurLine(ReadOnlySpan<uint> source, Span<uint> destination, int start, int step, int count, int radius)
    {
        var inverse = (1u << 24) / (uint)(2 * radius + 1);
        uint sumA = 0, sumR = 0, sumG = 0, sumB = 0;

        for (var i = 0; i <= Math.Min(radius, count - 1); i++)
        {
            var p = source[start + i * step];

            sumA += p >>> 24;
            sumR += (p >>> 16) & 0xFF;
            sumG += (p >>> 8) & 0xFF;
            sumB += p & 0xFF;
        }

        for (var i = 0; i < count; i++)
        {
            destination[start + i * step] = (Average(sumA, inverse) << 24) | (Average(sumR, inverse) << 16) | (Average(sumG, inverse) << 8) | Average(sumB, inverse);

            var entering = i + radius + 1;
            if (entering < count)
            {
                var p = source[start + entering * step];

                sumA += p >>> 24;
                sumR += (p >>> 16) & 0xFF;
                sumG += (p >>> 8) & 0xFF;
                sumB += p & 0xFF;
            }

            var leaving = i - radius;
            if (leaving >= 0)
            {
                var p = source[start + leaving * step];

                sumA -= p >>> 24;
                sumR -= (p >>> 16) & 0xFF;
                sumG -= (p >>> 8) & 0xFF;
                sumB -= p & 0xFF;
            }
        }

        static uint Average(uint sum, uint inverse) => (sum * inverse + (1u << 23)) >>> 24;
    }

    private static bool Clip(int width, int height, Rect? region, out Rect area)
    {
        var actualRegion = region ?? new Rect(0, 0, width, height);

        var left = Math.Max(0, actualRegion.Left);
        var top = Math.Max(0, actualRegion.Top);
        var right = Math.Min(width, actualRegion.Right);
        var bottom = Math.Min(height, actualRegion.Bottom);

        area = new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        return right > left && bottom > top;
    }

    private static void Run(int count, int pixelCount, Action<int> body)
    {
        var parts = Math.Min(count, Environment.ProcessorCount);

        if (parts <= 1 || pixelCount < ParallelThreshold)
        {
            for (var i = 0; i < count; i++)
            {
                body(i);
            }
            return;
        }

        Parallel.For(0, parts, part =>
        {
            var start = (int)((long)count * part / parts);
            var end = (int)((long)count * (part + 1) / parts);

            for (var i = start; i < end; i++)
            {
                body(i);
            }
        });
    }

    private static Span<uint> AsPixels(byte[] data) => MemoryMarshal.Cast<byte, uint>(data.AsSpan());

    private static Span<uint> Row(byte[] data, int stride, int x, int y, int count) => AsPixels(data).Slice(y * stride + x, count);

    private static byte[] Copy(byte[] source)
    {
        var copy = GC.AllocateUninitializedArray<byte>(source.Length);
        source.CopyTo(copy, 0);

        return copy;
    }

    private static uint ToByteFactor(float factor) => (uint)MathF.Round(Math.Clamp(float.IsNaN(factor) ? 0f : factor, 0f, 1f) * 255f);

    private static void BlendRow(Span<uint> destination, ReadOnlySpan<uint> source, uint factor)
    {
        for (var i = 0; i < destination.Length; i++)
        {
            var s = factor == 255 ? source[i] : Scale(source[i], factor);
            var a = s >>> 24;

            if (a == 255) destination[i] = s;
            else if (a != 0) destination[i] = Scale(destination[i], 255 - a) + s;
        }
    }

    private static uint Mul255(uint x, uint y)
    {
        var t = x * y + 128;
        return (t + (t >>> 8)) >>> 8;
    }

    private static uint ToFixedPoint(float factor)
    {
        if (!float.IsFinite(factor) || factor < 0f) throw new ArgumentOutOfRangeException(nameof(factor));

        return (uint)MathF.Round(MathF.Min(factor, 255f) * 256f);
    }

    private static uint Scale(uint pixel, uint factor)
    {
        var rb = (pixel & 0x00FF00FFu) * factor + 0x00800080u;
        rb = ((rb + ((rb >>> 8) & 0x00FF00FFu)) >>> 8) & 0x00FF00FFu;

        var ag = ((pixel >>> 8) & 0x00FF00FFu) * factor + 0x00800080u;
        ag = (ag + ((ag >>> 8) & 0x00FF00FFu)) & 0xFF00FF00u;

        return rb | ag;
    }

    private static Rgba Unpack(uint pixel)
    {
        var a = pixel >>> 24;
        if (a == 0) return Rgba.Transparent;
        if (a == 255) return new Rgba((byte)(pixel >>> 16), (byte)(pixel >>> 8), (byte)pixel, 255);

        var offset = (int)(a << 8);
        return new Rgba(Unpremultiplied[offset | (int)((pixel >>> 16) & 0xFF)], Unpremultiplied[offset | (int)((pixel >>> 8) & 0xFF)], Unpremultiplied[offset | (int)(pixel & 0xFF)], (byte)a);
    }

    private static uint Pack(Rgba color)
    {
        var a = (uint)color.A;
        if (a == 255) return 0xFF000000u | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
        if (a == 0) return 0;

        return (a << 24) | (Mul255(color.R, a) << 16) | (Mul255(color.G, a) << 8) | Mul255(color.B, a);
    }

    private static byte[] BuildUnpremultiplyTable()
    {
        var table = new byte[256 * 256];

        for (var a = 1; a < 256; a++)
        {
            for (var v = 0; v <= a; v++)
            {
                table[(a << 8) | v] = (byte)Math.Min(255, (v * 255 + a / 2) / a);
            }
        }

        return table;
    }

    private delegate void RowAction(Span<uint> row);
}