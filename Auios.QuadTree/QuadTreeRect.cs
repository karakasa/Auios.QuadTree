// Copyright 2020 Connor Andrew Ngo
// Licensed under the MIT License

namespace Auios.QuadTree;

/// <summary>Stores the position and size of the quadrant.</summary>
/// <remarks>Initializes a new instance of the <see cref="T:Auios.QuadTree.QuadTreeRect"></see> struct with the specified position and size.</remarks>
/// <param name="x">The x-coordinate of the upper-left corner of the quadrant.</param>
/// <param name="y">The y-coordinate of the upper-left corner of the quadrant.</param>
/// <param name="width">The width of the quadrant.</param>
/// <param name="height">The height of the quadrant.</param>
public sealed class QuadTreeRect(double x, double y, double width, double height)
{
    /// <summary>If an overlap check from the QuadTree returns true on this quadrant this flag will be set to true. Set to false on QuadTree.Clear().</summary>
    public bool isOverlapped = false;

    public readonly double X = x;

    public readonly double Y = y;

    public readonly double Width = width;

    public readonly double Height = height;

    public double Top => Y;

    public double Bottom => Y + Height;

    public double Left => X;

    public double Right => X + Width;

    public double HalfWidth => Width * 0.5;

    public double HalfHeight => Height * 0.5;

    public double CenterX => X + HalfWidth;

    public double CenterY => Y + HalfHeight;

    public bool Contains(double x, double y) 
        => (x >= Left && x <= Right && y <= Top && y >= Bottom);
}
