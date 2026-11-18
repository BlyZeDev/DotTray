namespace DotTray.Direct.Imaging;

using DotTray.Direct.Coloring;
using DotTray.Direct.Internal;
using DotTray.Direct.Primitives;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a mutable image that can be edited and turned into a <see cref="DirectImage"/>
/// </summary>
public sealed partial class DirectCanvas
{
    private const int ParallelThreshold = 4_194_304;
    private const int TileSize = 32;
    private const int MaxBlurRadius = 4096;

    private readonly Lock _lock;

    private byte[] curPixels;
    private int curWidth;
    private int curHeight;

    /// <summary>
    /// The dimensions of the canvas, in pixels
    /// </summary>
    public Dim Dimensions
    {
        get
        {
            lock (_lock)
            {
                return new Dim(curWidth, curHeight);
            }
        }
    }

    internal DirectCanvas(byte[] pixels, int width, int height)
    {
        _lock = new Lock();

        curPixels = pixels;
        curWidth = width;
        curHeight = height;
    }

    /// <summary>
    /// Creates an independent copy of this canvas
    /// </summary>
    /// <returns><see cref="DirectCanvas"/></returns>
    public DirectCanvas Clone()
    {
        lock (_lock)
        {
            return new DirectCanvas(Copy(curPixels), curWidth, curHeight);
        }
    }

    /// <summary>
    /// Gets the average color
    /// </summary>
    /// <remarks>
    /// The pixels are weighted by their opacity, so transparent parts do not darken the result. The alpha of the returned color is the average opacity
    /// </remarks>
    /// <param name="region">The part to look at, or <see langword="null"/> for the whole canvas</param>
    /// <returns><see cref="Rgba"/></returns>
    public Rgba GetAverageColor(Rect? region = null)
    {
        lock (_lock)
        {
            if (!Clip(curWidth, curHeight, region, out var area)) return Rgba.Transparent;

            long sumA = 0, sumR = 0, sumG = 0, sumB = 0;

            for (var y = 0; y < area.Height; y++)
            {
                foreach (var p in Row(curPixels, curWidth, area.X, area.Y + y, area.Width))
                {
                    sumA += p.A;
                    sumR += p.R;
                    sumG += p.G;
                    sumB += p.B;
                }
            }

            if (sumA == 0) return Rgba.Transparent;

            var count = (long)area.Width * area.Height;

            return new Rgba((byte)((sumR * 255 + sumA / 2) / sumA), (byte)((sumG * 255 + sumA / 2) / sumA), (byte)((sumB * 255 + sumA / 2) / sumA), (byte)((sumA + count / 2) / count));
        }
    }

    /// <summary>
    /// Gets or sets the color of a single pixel
    /// </summary>
    /// <param name="coordinate">The coordinates of the pixel to get or set</param>
    /// <returns><see cref="Rgba"/></returns>
    public Rgba this[Pos coordinate]
    {
        get => GetPixel(coordinate);
        set => SetPixel(coordinate, value);
    }

    /// <summary>
    /// Gets the color of a single pixel
    /// </summary>
    /// <param name="coordinates">The coordinates of the pixel to get</param>
    /// <returns><see cref="Rgba"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public Rgba GetPixel(Pos coordinates)
    {
        lock (_lock)
        {
            return AsPixels(curPixels)[IndexOf(coordinates.X, coordinates.Y)].ToRgba();
        }
    }

    /// <summary>
    /// Sets the color of a single pixel
    /// </summary>
    /// <param name="coordinates">The coordinates of the pixel to set</param>
    /// <param name="color">The new color</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas SetPixel(Pos coordinates, Rgba color)
    {
        lock (_lock)
        {
            AsPixels(curPixels)[IndexOf(coordinates.X, coordinates.Y)] = Pbgra32.FromRgba(color);
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
        var value = Pbgra32.FromRgba(color);

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
        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = Pbgra32.FromRgba(transform(row[i].ToRgba()));
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
            var color = row[i].ToRgba();

            if (color.A == 0
                || Math.Abs(color.R - from.R) > actualTolerance.R
                || Math.Abs(color.G - from.G) > actualTolerance.G
                || Math.Abs(color.B - from.B) > actualTolerance.B
                || Math.Abs(color.A - from.A) > actualTolerance.A) continue;

            row[i] = Pbgra32.FromRgba(to with { A = (byte)Pbgra32.Mul255(color.A, to.A) });
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
        var tint = Pbgra32.FromRgba(color);

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                row[i] = tint.Scale(row[i].A);
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

        var layer = Pbgra32.FromRgba(color with { A = 255 });
        var strength = (uint)color.A;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];

                if (!p.IsTransparent) row[i] = p.Scale(255 - strength) + layer.Scale(Pbgra32.Mul255(p.A, strength));
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
            var luma = p.Luma;

            row[i] = Pbgra32.FromChannels(p.A, luma, luma, luma);
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
            uint a = p.A;

            row[i] = new Pbgra32((a << 24) | ((a * 0x010101u) - (p.Packed & 0x00FFFFFFu)));
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
            for (var i = 0; i < row.Length; i++) row[i] = row[i].Scale(bFactor);
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
        var fPoint = ToFixedPoint(factor);
        if (fPoint == FixedOne) return this;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];
                uint a = p.A;

                row[i] = Pbgra32.FromChannels(a, Brighten(p.R, a), Brighten(p.G, a), Brighten(p.B, a));
            }
        });

        uint Brighten(byte channel, uint alpha) => Math.Min(alpha, (uint)((channel * fPoint + FixedHalf) >> FixedShift));
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
        var fPoint = ToFixedPoint(factor);
        if (fPoint == FixedOne) return this;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];
                var a = (int)p.A;
                var middle = (a * 128 + 127) / 255;

                row[i] = Pbgra32.FromChannels((uint)a, Stretch(p.R, middle, fPoint, a), Stretch(p.G, middle, fPoint, a), Stretch(p.B, middle, fPoint, a));
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
        var fPoint = ToFixedPoint(factor);
        if (fPoint == FixedOne) return this;

        return Apply(region, row =>
        {
            for (var i = 0; i < row.Length; i++)
            {
                var p = row[i];
                var a = (int)p.A;
                var gray = (int)p.Luma;

                row[i] = Pbgra32.FromChannels((uint)a, Stretch(p.R, gray, fPoint, a), Stretch(p.G, gray, fPoint, a), Stretch(p.B, gray, fPoint, a));
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

        lock (_lock)
        {
            BoxBlur(AsRaw(AsPixels(curPixels)), curWidth, curHeight, Math.Min(radius, MaxBlurRadius), false);
            return this;
        }
    }

    /// <summary>
    /// Sharpens the canvas
    /// </summary>
    /// <param name="radius">The size of the details to enhance in pixels, from <c>1</c> to <c>4096</c></param>
    /// <param name="amount">How strongly the details are added back, <c>0f</c> is unchanged, <c>1f</c> is a normal sharpening and higher is stronger</param>
    /// <returns><see cref="DirectCanvas"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DirectCanvas Sharpen(int radius = 1, float amount = 1f)
    {
        var fPoint = ToFixedPoint(amount);
        ArgumentOutOfRangeException.ThrowIfNegative(radius);

        if (fPoint == 0 || radius == 0) return this;

        lock (_lock)
        {
            var data = curPixels;
            var stride = curWidth;

            var blurred = AsPixels(data).ToArray();
            BoxBlur(AsRaw(blurred), curWidth, curHeight, Math.Min(radius, MaxBlurRadius), true);

            Run(curHeight, curWidth * curHeight, y =>
            {
                var row = Row(data, stride, 0, y, stride);

                for (var x = 0; x < row.Length; x++)
                {
                    var blurredPixel = blurred[y * stride + x];
                    if (row[x].IsTransparent || blurredPixel.IsTransparent) continue;

                    var original = row[x].ToRgba();
                    var soft = blurredPixel.ToRgba();

                    var strength = (fPoint * Math.Min((int)blurredPixel.A, 64)) >> 6;

                    row[x] = Pbgra32.FromRgba(new Rgba(Enhance(original.R, soft.R, strength), Enhance(original.G, soft.G, strength), Enhance(original.B, soft.B, strength), original.A));
                }
            });

            return this;
        }

        static byte Enhance(int original, int soft, long amount) => (byte)Math.Clamp(original + (int)((((original - soft) * amount) + FixedHalf) >> FixedShift), 0, 255);
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
            if (!Clip(curWidth, curHeight, region, out var area)) return this;

            var buffer = new Pbgra32[area.Width];

            for (int top = area.Top, bottom = area.Bottom - 1; top < bottom; top++, bottom--)
            {
                var upper = Row(curPixels, curWidth, area.X, top, area.Width);
                var lower = Row(curPixels, curWidth, area.X, bottom, area.Width);

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
            AsPixels(curPixels).Reverse();
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
            if (!Clip(curWidth, curHeight, area, out var clipped)) throw new ArgumentException("The area does not overlap the canvas", nameof(area));

            curPixels = CopyRegion(clipped);
            curWidth = clipped.Width;
            curHeight = clipped.Height;

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
            var newWidth = (long)curWidth + left + right;
            var newHeight = (long)curHeight + top + bottom;
            ArgumentOutOfRangeException.ThrowIfGreaterThan(newWidth * newHeight * 4, Array.MaxLength, "padding");

            var padded = new byte[newWidth * newHeight * 4];
            if (fillColor.HasValue) AsPixels(padded).Fill(Pbgra32.FromRgba(fillColor.Value));

            for (var y = 0; y < curHeight; y++)
            {
                Row(curPixels, curWidth, 0, y, curWidth).CopyTo(Row(padded, (int)newWidth, left, top + y, curWidth));
            }

            curPixels = padded;
            curWidth = (int)newWidth;
            curHeight = (int)newHeight;

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
    /// To make the image completely round just use <see cref="float.MaxValue"/>
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
            var max = (float)Math.Max(curWidth, curHeight);

            topLeft = Math.Clamp(float.IsNaN(topLeft) ? 0f : topLeft, 0f, max);
            topRight = Math.Clamp(float.IsNaN(topRight) ? 0f : topRight, 0f, max);
            bottomRight = Math.Clamp(float.IsNaN(bottomRight) ? 0f : bottomRight, 0f, max);
            bottomLeft = Math.Clamp(float.IsNaN(bottomLeft) ? 0f : bottomLeft, 0f, max);

            var factor = Math.Min(
                Math.Min(Fit(curWidth, topLeft + topRight), Fit(curWidth, bottomLeft + bottomRight)),
                Math.Min(Fit(curHeight, topLeft + bottomLeft), Fit(curHeight, topRight + bottomRight)));

            var data = AsPixels(curPixels);

            RoundCorner(data, 0, 0, 1, 1, topLeft * factor);
            RoundCorner(data, curWidth - 1, 0, -1, 1, topRight * factor);
            RoundCorner(data, curWidth - 1, curHeight - 1, -1, -1, bottomRight * factor);
            RoundCorner(data, 0, curHeight - 1, 1, -1, bottomLeft * factor);

            return this;
        }

        static float Fit(int length, float sum) => sum > length ? length / sum : 1f;
    }

    /// <summary>
    /// Makes the corners of the canvas transparent and round with smooth edges
    /// </summary>
    /// <remarks>
    /// To make the image completely round just use <see cref="float.MaxValue"/>
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
        return Clip(source.Dimensions.Width, source.Dimensions.Height, sourceRegion, out var area)
            ? Compose(source.Pixels, source.Dimensions.Width, area, position, opacity)
            : this;
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
        byte[] snapshot;
        Rect area;

        lock (source._lock)
        {
            if (!Clip(source.curWidth, source.curHeight, sourceRegion, out area)) return this;

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
            return new DirectImage(Copy(curPixels), curWidth, curHeight);
        }
    }

    /// <summary>
    /// Saves the current canvas to a file
    /// </summary>
    /// <remarks>
    /// The format is chosen by the extension of <paramref name="path"/>.<br/>
    /// Supported formats are <c>.png</c>, <c>.jpg</c>, <c>.jpeg</c> and <c>.bmp</c>.<br/>
    /// For formats other than <c>.png</c>, <paramref name="background"/> is used to fill the transparent parts
    /// </remarks>
    /// <param name="path">The path of the file</param>
    /// <param name="background">The color used to replace transparent pixels in formats without transparency, <see langword="null"/> for <see cref="Rgba.White"/></param>
    public void SaveToFile(string path, Rgba? background = null)
    {
        lock (_lock)
        {
            Wic.Save(path, curPixels, curWidth, curHeight, background);
        }
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
}