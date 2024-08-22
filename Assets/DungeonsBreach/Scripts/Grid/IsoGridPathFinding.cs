using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Runtime.CompilerServices;


[System.Serializable]
public struct PathFindingMask
{
    public byte value;

    public const byte landBlocking = 0b00001111;
    public const byte airBlocking = 0b00000111;
    
    public PathFindingMask(byte value)
    {
        this.value = value;
    }

    public static PathFindingMask operator | (PathFindingMask lhs, PathFindingMask rhs)
    {
        return new PathFindingMask((byte)(lhs.value | rhs.value));
    }

    public static PathFindingMask operator ^(PathFindingMask lhs, PathFindingMask rhs)
    {
        return new PathFindingMask((byte)(lhs.value ^ rhs.value));
    }
}


public enum PathFindingTile : byte
{
    Land = 0,
    Obstacle = 0b1,
    Ally = 0b10,
    Enemy = 0b100,
    Hole = 0b1000,
    Water = 0b10000,
    Trap = 0b100000,
}

public class AstarNode
{
    public IsoGridCoord coord;

    public float G { get; set; }
    public float H { get; set; }
    public float F => G + H;

    public AstarNode Parent {  get; set; }

    public AstarNode(IsoGridCoord coord)
    {
        this.coord = coord;
    }

    public override bool Equals(object obj)
    {
        return obj is AstarNode other && other.coord == coord;
    }

    public override int GetHashCode()
    {
        return coord.GetHashCode();
    }
}


public static class IsoGridPathFinding
{

    public static bool FindPathAstar(IsoGridCoord start, IsoGridCoord target, IsoGrid grid, PathFindingMask agent_mask, out List<IsoGridCoord> path)
    {
        AstarNode startNode = new AstarNode(start);
        AstarNode targetNode = new AstarNode(target);
        startNode.G = ManhattanDistance(startNode, startNode);
        startNode.H = ManhattanDistance(startNode, targetNode);

        var mask = grid.PathFindingMask;
        List<AstarNode> openNodes = new List<AstarNode>() { startNode };
        HashSet<AstarNode> closedNodes = new HashSet<AstarNode>();

        bool pathFound = false;
        AstarNode currentNode = startNode;

        while (openNodes.Count > 0 && !pathFound)
        {
            openNodes.Sort((a, b) => b.F.CompareTo(a.F));
            currentNode = openNodes[openNodes.Count - 1];
            openNodes.RemoveAt(openNodes.Count - 1);
            closedNodes.Add(currentNode);

            //check neightbours
            for (int i = 0; i < IsoGridMetrics.directionCount; i++)
            {
                var neighbourCoord = currentNode.coord + IsoGridMetrics.GridDirectionToCoord[i];
                if (neighbourCoord.x < grid.Dimension.x && neighbourCoord.y < grid.Dimension.y
                    && neighbourCoord.x >= 0 && neighbourCoord.y >= 0)
                {
                    var neighbourNode = new AstarNode(neighbourCoord);

                    //found the path
                    if(neighbourNode.coord == targetNode.coord)
                    {
                        pathFound = true;
                        targetNode.Parent = currentNode;
                        break;
                    }


                    if (!closedNodes.Contains(neighbourNode))
                    {
                        neighbourNode.G = currentNode.G + ManhattanDistance(neighbourNode, currentNode);
                        neighbourNode.H = ManhattanDistance(neighbourNode, targetNode);
                        neighbourNode.Parent = currentNode;

                        var neighbourOpenIndex = openNodes.IndexOf(neighbourNode);
                        //new open node, add to the list
                        if (neighbourOpenIndex < 0)
                        {
                            var neighbourMaskIndex = IsoGridMetrics.To2DArrayIndex(neighbourCoord, grid.Dimension);
                            //blocked node
                            if ((mask[neighbourMaskIndex].value & agent_mask.value) != 0)
                            {
                                closedNodes.Add(neighbourNode);
                            }
                            else
                            {
                                openNodes.Add(neighbourNode);
                            }
                        }

                        //already in the list, if F is smaller, update the node
                        else if(neighbourNode.F< openNodes[neighbourOpenIndex].F)
                        {
                            openNodes[neighbourOpenIndex] = neighbourNode;
                        }
                    }
                }
            }
        }

        path = new List<IsoGridCoord>();

        if(pathFound)
        {
            var backTrackNode = targetNode;
            while(backTrackNode.Parent != startNode)
            {
                path.Add(backTrackNode.coord);
                backTrackNode = backTrackNode.Parent;
            }
            path.Add(backTrackNode.coord);
            path.Add(start);
        }

        return pathFound;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ManhattanDistance(AstarNode node1, AstarNode node2)
    {
        return math.abs(node1.coord.x - node2.coord.x) + math.abs(node1.coord.y - node2.coord.y);
    }
}
