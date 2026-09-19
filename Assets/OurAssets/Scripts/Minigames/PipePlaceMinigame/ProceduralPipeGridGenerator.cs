using System.Collections.Generic;
using UnityEngine;

public static class ProceduralPipeGridGenerator
{
    static readonly string s_StraightPipePath = "PipeScriptableObjects/StraightPipe";
    static readonly string s_LPipePath = "PipeScriptableObjects/LPipe";
    static readonly string s_TPipePath = "PipeScriptableObjects/TPipe";
    static readonly string s_CrossPipePath = "PipeScriptableObjects/CrossPipe";

    public static PipeGridData Generate(Vector2Int gridSize, int numStartPipes, int numEndPipes, int extraPiecesMin = 0, int extraPiecesMax = 3)
    {
        HashSet<Vector2Int> usedCells = new HashSet<Vector2Int>();
        StartEndPipe[] startPipes = new StartEndPipe[numStartPipes];
        for (int i = 0; i < numStartPipes; ++i) startPipes[i] = PickTerminal(gridSize, usedCells);
        StartEndPipe[] endPipes = new StartEndPipe[numEndPipes];
        for (int i = 0; i < numEndPipes; ++i) endPipes[i] = PickTerminal(gridSize, usedCells);
        List<Vector2Int> terminalCells = new List<Vector2Int>(numStartPipes + numEndPipes);
        foreach (StartEndPipe startPipe in startPipes) terminalCells.Add(startPipe.CellPosition);
        foreach (StartEndPipe endPipe in endPipes) terminalCells.Add(endPipe.CellPosition);
        HashSet<Vector2Int> includedCells = BuildConnectingTree(gridSize, terminalCells, out Dictionary<Vector2Int, Vector2Int> parents);
        Dictionary<Vector2Int, HashSet<PipeSide>> openSidesPerCell = BuildOpenSides(includedCells, parents, startPipes, endPipes);
        return new PipeGridData()
        {
            GridSize = gridSize,
            StartPipes = startPipes,
            EndPipes = endPipes,
            Pipes = BuildRequiredPipeInventory(openSidesPerCell, extraPiecesMin, extraPiecesMax)
        };
    }

    #region Terminal Selection
    static StartEndPipe PickTerminal(Vector2Int gridSize, HashSet<Vector2Int> usedCells)
    {
        while (true)
        {
            PipeSide side = (PipeSide)Random.Range(0, 4);
            Vector2Int cell = side switch
            {
                PipeSide.Left => new Vector2Int(0, Random.Range(1, gridSize.y - 1)),
                PipeSide.Right => new Vector2Int(gridSize.x - 1, Random.Range(1, gridSize.y - 1)),
                PipeSide.Bottom => new Vector2Int(Random.Range(1, gridSize.x - 1), 0),
                PipeSide.Top => new Vector2Int(Random.Range(1, gridSize.x - 1), gridSize.y - 1),
                _ => throw new System.ArgumentException("Somehow you input a side that doesn't exist")
            };
            if (usedCells.Contains(cell)) continue;
            usedCells.Add(cell);
            return new StartEndPipe() { CellPosition = cell, EntranceExitSide = side };
        }
    }
    #endregion Terminal Selection

    #region Connecting Tree
    static IEnumerable<Vector2Int> GetGridNeighbors(Vector2Int cell, Vector2Int gridSize)
    {
        if (cell.x > 0) yield return new Vector2Int(cell.x - 1, cell.y);
        if (cell.y < gridSize.y - 1) yield return new Vector2Int(cell.x, cell.y + 1);
        if (cell.x < gridSize.x - 1) yield return new Vector2Int(cell.x + 1, cell.y);
        if (cell.y > 0) yield return new Vector2Int(cell.x, cell.y - 1);
    }

    static int ManhattanDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    static HashSet<Vector2Int> BuildConnectingTree(Vector2Int gridSize, List<Vector2Int> terminalCells, out Dictionary<Vector2Int, Vector2Int> parents)
    {
        HashSet<Vector2Int> includedCells = new HashSet<Vector2Int>() { terminalCells[0] };
        HashSet<Vector2Int> terminalCellSet = new HashSet<Vector2Int>(terminalCells);
        parents = new Dictionary<Vector2Int, Vector2Int>();
        Vector2Int? lastJunctionHint = null;
        for (int i = 1; i < terminalCells.Count; ++i)
        {
            if (includedCells.Contains(terminalCells[i])) continue;
            Vector2Int target = lastJunctionHint ?? FindNearestCell(terminalCells[i], includedCells);
            Vector2Int connectedAt = ConnectCellToTree(terminalCells[i], gridSize, includedCells, parents, target, lastJunctionHint);
            lastJunctionHint = WouldBeJunction(connectedAt, parents, includedCells, terminalCellSet) ? connectedAt : null;
        }
        return includedCells;
    }

    static readonly Vector2Int[] s_NeighborOffsets = { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };

    static bool WouldBeJunction(Vector2Int cell, Dictionary<Vector2Int, Vector2Int> parents, HashSet<Vector2Int> includedCells, HashSet<Vector2Int> terminalCellSet)
    {
        int degree = terminalCellSet.Contains(cell) ? 1 : 0;
        foreach (Vector2Int offset in s_NeighborOffsets)
        {
            Vector2Int neighbor = cell + offset;
            if (!includedCells.Contains(neighbor)) continue;
            bool bIsTreeEdge = (parents.TryGetValue(cell, out Vector2Int cellParent) && cellParent == neighbor)
                || (parents.TryGetValue(neighbor, out Vector2Int neighborParent) && neighborParent == cell);
            if (bIsTreeEdge) ++degree;
        }
        return degree >= 3;
    }

    static Vector2Int ConnectCellToTree(Vector2Int from, Vector2Int gridSize, HashSet<Vector2Int> includedCells, Dictionary<Vector2Int, Vector2Int> parents, Vector2Int target, Vector2Int? requiredConnectionCell)
    {
        HashSet<Vector2Int> visitedThisWalk = new HashSet<Vector2Int>() { from };
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(from);
        Dictionary<Vector2Int, Vector2Int> walkPredecessors = new Dictionary<Vector2Int, Vector2Int>();
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            Vector2Int? connectTo = null;
            foreach (Vector2Int neighbor in GetGridNeighbors(current, gridSize))
            {
                if (!includedCells.Contains(neighbor)) continue;
                if (requiredConnectionCell.HasValue && neighbor != requiredConnectionCell.Value) continue;
                connectTo = neighbor;
                break;
            }
            if (connectTo.HasValue)
            {
                MergeWalkIntoTree(current, connectTo.Value, walkPredecessors, includedCells, parents);
                return connectTo.Value;
            }
            Vector2Int? next = PickWeightedUnvisitedNeighbor(current, gridSize, visitedThisWalk, includedCells, target);
            if (next.HasValue)
            {
                visitedThisWalk.Add(next.Value);
                walkPredecessors[next.Value] = current;
                stack.Push(next.Value);
            }
            else stack.Pop();
        }
        if (requiredConnectionCell.HasValue) return ConnectCellToTree(from, gridSize, includedCells, parents, target, null);
        throw new System.InvalidOperationException("Could not connect a terminal cell to the rest of the pipe network");
    }

    static Vector2Int? PickWeightedUnvisitedNeighbor(Vector2Int current, Vector2Int gridSize, HashSet<Vector2Int> visitedThisWalk, HashSet<Vector2Int> includedCells, Vector2Int target)
    {
        const int closerWeight = 6;
        const int furtherWeight = 1;
        int currentDistance = ManhattanDistance(current, target);
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<int> weights = new List<int>();
        int totalWeight = 0;
        foreach (Vector2Int neighbor in GetGridNeighbors(current, gridSize))
        {
            if (visitedThisWalk.Contains(neighbor) || includedCells.Contains(neighbor)) continue;
            int weight = ManhattanDistance(neighbor, target) < currentDistance ? closerWeight : furtherWeight;
            candidates.Add(neighbor);
            weights.Add(weight);
            totalWeight += weight;
        }
        if (candidates.Count == 0) return null;
        int roll = Random.Range(0, totalWeight);
        for (int i = 0; i < candidates.Count; ++i)
        {
            roll -= weights[i];
            if (roll < 0) return candidates[i];
        }
        return candidates[^1];
    }

    static Vector2Int FindNearestCell(Vector2Int from, HashSet<Vector2Int> cells)
    {
        Vector2Int nearest = default;
        int nearestDistance = int.MaxValue;
        foreach (Vector2Int cell in cells)
        {
            int distance = ManhattanDistance(from, cell);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = cell;
            }
        }
        return nearest;
    }

    static void MergeWalkIntoTree(Vector2Int current, Vector2Int connectTo, Dictionary<Vector2Int, Vector2Int> walkPredecessors, HashSet<Vector2Int> includedCells, Dictionary<Vector2Int, Vector2Int> parents)
    {
        includedCells.Add(current);
        parents[current] = connectTo;
        Vector2Int node = current;
        while (walkPredecessors.TryGetValue(node, out Vector2Int predecessor))
        {
            includedCells.Add(predecessor);
            parents[predecessor] = node;
            node = predecessor;
        }
    }
    #endregion Connecting Tree

    #region Open Sides
    static PipeSide DirectionToSide(Vector2Int delta) => delta switch
    {
        { x: -1, y: 0 } => PipeSide.Left,
        { x: 0, y: 1 } => PipeSide.Top,
        { x: 1, y: 0 } => PipeSide.Right,
        { x: 0, y: -1 } => PipeSide.Bottom,
        _ => throw new System.ArgumentException("Delta was not a single grid step in one of the 4 directions")
    };

    static Dictionary<Vector2Int, HashSet<PipeSide>> BuildOpenSides(HashSet<Vector2Int> includedCells, Dictionary<Vector2Int, Vector2Int> parents, StartEndPipe[] startPipes, StartEndPipe[] endPipes)
    {
        Dictionary<Vector2Int, HashSet<PipeSide>> openSidesPerCell = new Dictionary<Vector2Int, HashSet<PipeSide>>();
        foreach (Vector2Int cell in includedCells) openSidesPerCell[cell] = new HashSet<PipeSide>();

        foreach (Vector2Int cell in includedCells)
        {
            if (!parents.TryGetValue(cell, out Vector2Int parentCell) || !includedCells.Contains(parentCell)) continue; // Tree root
            PipeSide sideToParent = DirectionToSide(parentCell - cell);
            openSidesPerCell[cell].Add(sideToParent);
            openSidesPerCell[parentCell].Add(PipeSideUtil.Opposite(sideToParent));
        }

        foreach (StartEndPipe startPipe in startPipes) openSidesPerCell[startPipe.CellPosition].Add(startPipe.EntranceExitSide);
        foreach (StartEndPipe endPipe in endPipes) openSidesPerCell[endPipe.CellPosition].Add(endPipe.EntranceExitSide);

        return openSidesPerCell;
    }
    #endregion Open Sides

    #region Pipe Inventory
    static bool IsOppositePair(HashSet<PipeSide> sides)
    {
        IEnumerator<PipeSide> enumerator = sides.GetEnumerator();
        enumerator.MoveNext();
        PipeSide first = enumerator.Current;
        enumerator.MoveNext();
        PipeSide second = enumerator.Current;
        return PipeSideUtil.Opposite(first) == second;
    }

    static string RequiredPieceResourcePath(HashSet<PipeSide> openSides) => openSides.Count switch
    {
        2 => IsOppositePair(openSides) ? s_StraightPipePath : s_LPipePath,
        3 => s_TPipePath,
        4 => s_CrossPipePath,
        _ => throw new System.InvalidOperationException($"A generated pipe cell ended up needing {openSides.Count} open side(s), which no piece can satisfy")
    };

    static PipeData[] BuildRequiredPipeInventory(Dictionary<Vector2Int, HashSet<PipeSide>> openSidesPerCell, int extraPiecesMin, int extraPiecesMax)
    {
        Dictionary<string, uint> requiredCounts = new Dictionary<string, uint>();
        foreach (HashSet<PipeSide> openSides in openSidesPerCell.Values)
        {
            string resourcePath = RequiredPieceResourcePath(openSides);
            requiredCounts.TryGetValue(resourcePath, out uint count);
            requiredCounts[resourcePath] = count + 1;
        }
        PipeData[] pipes = new PipeData[requiredCounts.Count];
        int i = 0;
        foreach (KeyValuePair<string, uint> requiredCount in requiredCounts)
        {
            uint quantity = requiredCount.Value + (uint)Random.Range(extraPiecesMin, extraPiecesMax + 1);
            pipes[i++] = new PipeData() { PipeType = Resources.Load<PipeSO>(requiredCount.Key), PipeQuantity = quantity };
        }
        return pipes;
    }
    #endregion Pipe Inventory
}
