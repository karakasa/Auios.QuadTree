// Copyright 2020 Connor Andrew Ngo
// Licensed under the MIT License

namespace Auios.QuadTree
{
    public interface IQuadTreeObjectBounds<in T>
    {
        double GetTop(T obj);
        double GetBottom(T obj);
        double GetLeft(T obj);
        double GetRight(T obj);
    }
}
