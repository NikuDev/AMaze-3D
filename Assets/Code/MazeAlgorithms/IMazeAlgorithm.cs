using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
    /// <summary>
    /// All classes that can create a maze with an algorithm, should implement
    /// this interface so the MazeManager can use it
    /// </summary>
    interface IMazeAlgorithm
    {
        MazeAlgorithmResult GenerateMaze(List<Vector3> allCellPositions);
    }
}
