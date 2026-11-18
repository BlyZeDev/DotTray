namespace DotTray.Direct.Imaging;

using DotTray.Direct.Internal;
using DotTray.Direct.Primitives;
using System.Runtime.InteropServices;

public sealed partial class DirectCanvas
{
    private const int FixedShift = 16;
    private const long FixedHalf = 1L << (FixedShift - 1);
    private const long FixedOne = 1L << FixedShift;

    private DirectCanvas Apply(Rect? region, RowAction action, bool parallel = true)
    {
        lock (_lock)
        {
            if (!Clip(curWidth, curHeight, region, out var area)) return this;

            var data = curPixels;
            var stride = curWidth;

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

    private void RoundCorner(Span<Pbgra32> data, int originX, int originY, int directionX, int directionY, float radius)
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

                var index = (originY + y * directionY) * curWidth + originX + x * directionX;
                data[index] = data[index].Scale((uint)MathF.Round(coverage * 255f));
            }
        }
    }

    private DirectCanvas Compose(byte[] source, int sourceStride, Rect sourceArea, Pos position, float opacity)
    {
        var bFactor = ToByteFactor(opacity);
        if (bFactor == 0) return this;

        lock (_lock)
        {
            var left = Math.Max(0, position.X);
            var top = Math.Max(0, position.Y);
            var right = Math.Min(curWidth, position.X + sourceArea.Width);
            var bottom = Math.Min(curHeight, position.Y + sourceArea.Height);

            if (right <= left || bottom <= top) return this;

            var data = curPixels;
            var stride = curWidth;
            var count = right - left;
            var sourceX = sourceArea.X + (left - position.X);
            var sourceY = sourceArea.Y + (top - position.Y);

            Run(bottom - top,
                count * (bottom - top),
                i => BlendRow(Row(data, stride, left, top + i, count), Row(source, sourceStride, sourceX, sourceY + i, count), bFactor));

            return this;
        }
    }

    private DirectCanvas Transpose(bool clockwise)
    {
        lock (_lock)
        {
            var rotated = new byte[curPixels.Length];
            var src = AsPixels(curPixels);
            var dst = AsPixels(rotated);

            for (var tileY = 0; tileY < curHeight; tileY += TileSize)
            {
                for (var tileX = 0; tileX < curWidth; tileX += TileSize)
                {
                    for (var y = tileY; y < Math.Min(curHeight, tileY + TileSize); y++)
                    {
                        for (var x = tileX; x < Math.Min(curWidth, tileX + TileSize); x++)
                        {
                            dst[clockwise ? x * curHeight + (curHeight - 1 - y) : (curWidth - 1 - x) * curHeight + y] = src[y * curWidth + x];
                        }
                    }
                }
            }

            curPixels = rotated;
            (curWidth, curHeight) = (curHeight, curWidth);

            return this;
        }
    }

    private byte[] CopyRegion(Rect area)
    {
        var copy = GC.AllocateUninitializedArray<byte>(area.Width * area.Height * 4);

        for (var y = 0; y < area.Height; y++)
        {
            Row(curPixels, curWidth, area.X, area.Y + y, area.Width).CopyTo(Row(copy, area.Width, 0, y, area.Width));
        }

        return copy;
    }

    private int IndexOf(int x, int y)
    {
        if ((uint)x >= (uint)curWidth) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)curHeight) throw new ArgumentOutOfRangeException(nameof(y));

        return y * curWidth + x;
    }

    private static void BoxBlur(Span<uint> data, int width, int height, int radius, bool clampEdges)
    {
        var buffer = new uint[data.Length];

        for (var pass = 0; pass < 3; pass++)
        {
            for (var y = 0; y < height; y++)
            {
                BoxBlurLine(data, buffer, y * width, 1, width, radius, clampEdges);
            }

            for (var x = 0; x < width; x++)
            {
                BoxBlurLine(buffer, data, x, width, height, radius, clampEdges);
            }
        }
    }

    private static void BoxBlurLine(ReadOnlySpan<uint> source, Span<uint> destination, int start, int step, int count, int radius, bool clampEdges)
    {
        var inverse = (1u << 24) / (uint)(2 * radius + 1);
        uint sumA = 0, sumR = 0, sumG = 0, sumB = 0;

        for (var i = -radius; i <= radius; i++)
        {
            var index = i;

            if (index < 0 || index >= count)
            {
                if (!clampEdges) continue;

                index = Math.Clamp(index, 0, count - 1);
            }

            Add(source[start + index * step], ref sumA, ref sumR, ref sumG, ref sumB);
        }

        for (var i = 0; i < count; i++)
        {
            destination[start + i * step] = (Average(sumA, inverse) << 24) | (Average(sumR, inverse) << 16) | (Average(sumG, inverse) << 8) | Average(sumB, inverse);

            var entering = i + radius + 1;
            var leaving = i - radius;

            if (clampEdges)
            {
                Add(source[start + Math.Min(entering, count - 1) * step], ref sumA, ref sumR, ref sumG, ref sumB);
                Remove(source[start + Math.Max(leaving, 0) * step], ref sumA, ref sumR, ref sumG, ref sumB);
                continue;
            }

            if (entering < count) Add(source[start + entering * step], ref sumA, ref sumR, ref sumG, ref sumB);
            if (leaving >= 0) Remove(source[start + leaving * step], ref sumA, ref sumR, ref sumG, ref sumB);
        }

        static void Add(uint p, ref uint a, ref uint r, ref uint g, ref uint b)
        {
            a += p >>> 24;
            r += (p >>> 16) & 0xFF;
            g += (p >>> 8) & 0xFF;
            b += p & 0xFF;
        }

        static void Remove(uint p, ref uint a, ref uint r, ref uint g, ref uint b)
        {
            a -= p >>> 24;
            r -= (p >>> 16) & 0xFF;
            g -= (p >>> 8) & 0xFF;
            b -= p & 0xFF;
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

    private static Span<Pbgra32> AsPixels(byte[] data) => MemoryMarshal.Cast<byte, Pbgra32>(data.AsSpan());

    private static Span<uint> AsRaw(Span<Pbgra32> data) => MemoryMarshal.Cast<Pbgra32, uint>(data);

    private static Span<Pbgra32> Row(byte[] data, int stride, int x, int y, int count) => AsPixels(data).Slice(y * stride + x, count);

    private static byte[] Copy(byte[] source)
    {
        var copy = GC.AllocateUninitializedArray<byte>(source.Length);
        source.CopyTo(copy, 0);

        return copy;
    }

    private static uint ToByteFactor(float factor) => (uint)MathF.Round(Math.Clamp(float.IsNaN(factor) ? 0f : factor, 0f, 1f) * 255f);

    private static void BlendRow(Span<Pbgra32> destination, ReadOnlySpan<Pbgra32> source, uint factor)
    {
        for (var i = 0; i < destination.Length; i++)
        {
            var s = factor == 255 ? source[i] : source[i].Scale(factor);

            if (!s.IsTransparent) destination[i] = s.Over(destination[i]);
        }
    }

    private static long ToFixedPoint(float factor)
    {
        if (!float.IsFinite(factor) || factor < 0f) throw new ArgumentOutOfRangeException(nameof(factor));

        return (long)Math.Round(Math.Min((double)factor, 255.0) * FixedOne);
    }

    private static uint Stretch(int value, int center, long factor, int alpha) => (uint)Math.Clamp(center + (int)(((value - center) * factor + FixedHalf) >> FixedShift), 0, alpha);

    private delegate void RowAction(Span<Pbgra32> row);
}