using UnityEngine;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public static class IsoGridMetrics
{
    public const float sinAngle = 0.60876143f;
    public const float cosAngle = 0.79335334f;

    public const int directionCount = 4;

    public static float2 basis1 = new float2(-cosAngle, sinAngle);
    public static float2 basis2 = new float2(cosAngle, sinAngle);

    public static Matrix2x2 basisMatrix = new Matrix2x2(-cosAngle, cosAngle, sinAngle, sinAngle);
    public static Matrix2x2 standardBasis = new Matrix2x2(1, 0, 0, 1);

    public static IsoGridCoord[] GridDirectionToCoord =
    {
        new IsoGridCoord(-1,0),
        new IsoGridCoord(0,-1),
        new IsoGridCoord(1,0),
        new IsoGridCoord(0,1),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IsoGridDirection CoordToDirection(this IsoGridCoord coord)
    {
        Debug.Assert(coord.x * coord.y == 0);
        int mag = math.abs(coord.x + coord.y);
        var normalized = new IsoGridCoord(coord.x / mag, coord.y / mag);
        for (int i = 0; i < GridDirectionToCoord.Length; i++)
        {
            if (GridDirectionToCoord[i] == normalized)
            {
                return (IsoGridDirection)i;
            }
        }
        return IsoGridDirection.SE;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToWorldPosition(this IsoGridCoord coord, IsoGrid grid)
    {
        float suby = grid.CellSize * (coord.y - (grid.Dimension.y / 2));
        float subx = grid.CellSize * (coord.x - (grid.Dimension.x / 2));

        float x = (-subx * cosAngle) + suby * cosAngle;
        float y = (subx * sinAngle) + suby * sinAngle;
        return new float3(x + grid.Offset.x, y + grid.Offset.y, 0);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IsoGridCoord ToIsoCoordinate(this Vector3 position, IsoGrid grid)
    {
        float edge = grid.CellSize;
        Matrix2x2 baseM = basisMatrix * edge;
        var tMatrix = baseM.Inverse();
        var v = tMatrix * new Vector2(position.x, position.y);
        var center = grid.Center;
        return new IsoGridCoord((int)(v.x + 0.5f * math.sign(v.x)) + center.x, (int)(v.y + 0.5f * math.sign(v.y)) + center.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int To2DArrayIndex(this IsoGridCoord coord, int2 dimension)
    {
        return coord.y * dimension.x + coord.x;
    }

    public static IsoGridDirection ApproximateDirection(IsoGridCoord from, IsoGridCoord to)
    {
        int deltaX = from.x - to.x;
        int deltaY = from.y - to.y;

        if(math.abs(deltaX)>math.abs(deltaY))
        {
            return deltaX >= 0 ? IsoGridDirection.NW : IsoGridDirection.SE;
        }
        else
        {
            return deltaY >= 0 ? IsoGridDirection.NE : IsoGridDirection.SW;
        }
    }
    public static IsoGridCoord OnRelativeTo(this IsoGridCoord relative, IsoGridCoord base_coord,IsoGridDirection base_dir)
    {
        int turn = (int)base_dir;
        IsoGridCoord coord = relative;
        for(int i = 0; i < turn; i++)
        {
            coord = coord.RotateCW();
        }
        return coord + base_coord;
    }


    private static IsoGridCoord RotateCW(this IsoGridCoord coord)
    {
        return new IsoGridCoord(-coord.y, coord.x);
    }

    public static IsoGridCoord RotateCCW(this IsoGridCoord coord, int turn_count)
    {
        IsoGridCoord result = coord;
        for (int i = 0; i < turn_count; i++)
        {
            result = result.RotateCCW();
        }
        return result;
    }

    private static IsoGridCoord RotateCCW(this IsoGridCoord coord)
    {
        return new IsoGridCoord(-coord.y, coord.x);
    }

    public static IsoGridDirection RotateRelativeTo(this IsoGridDirection relative, IsoGridDirection base_dir)
    {
        int index = ((int)base_dir + (int)relative) % directionCount;
        return (IsoGridDirection)index;
    }


    public static IsoGridDirection DirectionTo(this IsoGridCoord from, IsoGridCoord to, IsoGrid grid)
    {
        var surrounding = grid.SurroundingCoordsWithDummy(from);
        int index = 0;
        var min = int.MaxValue;
        for (int i = 0; i < surrounding.Length; i++)
        {
            if (surrounding[i].x < 0)
                continue;
            int dist = IsoGridCoord.Distance(surrounding[i], to);
            if (dist < min)
            {
                min = dist;
                index = i;
            }
        }
        return (IsoGridDirection)index;
    }


    public static IsoGridDirection Opposite(this IsoGridDirection direction)
    {
        return (IsoGridDirection)(((int)direction + 2)%4);
    }

}
