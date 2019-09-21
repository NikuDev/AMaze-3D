using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
    interface IMazeAlgorithm
    {
        bool TryGetWallPositionsToDestroy(List<Vector3> allCellPositions, out List<Vector3> xWallPositionsToDestroy, out List<Vector3> yWallPositionsToDestroy);
    }
}
