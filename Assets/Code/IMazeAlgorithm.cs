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
        MazeAlgorithmResult GenerateMaze(List<Vector3> allCellPositions);
    }
}
