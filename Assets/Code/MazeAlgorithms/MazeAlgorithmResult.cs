using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
    /// <summary>
    /// DTO for holding the results needed to create the maze
    /// </summary>
    class MazeAlgorithmResult
    {
        /// <summary>
        /// The walls along the x-axis to destroy to make the maze
        /// </summary>
        public List<Vector3> XWallPositionsToDestroy { get; set; }

        /// <summary>
        /// The walls along the y-axis to destroy to make the maze
        /// </summary>
        public List<Vector3> YWallPositionsToDestroy { get; set; }

        /// <summary>
        /// Where the maze starts (e.g. position to put a character)
        /// </summary>
        public Vector3 StartPosition { get; set; }

        /// <summary>
        /// Where the maze ends (e.g. position to put the exit)
        /// </summary>
        public Vector3 EndPosition { get; set; }

        /// <summary>
        /// Returns true if the x- or y-wall collections have GameObjects (to destroy)
        /// </summary>
        public bool HasWallsToDestroy
        {
            get
            {
                return this.XWallPositionsToDestroy.Any() || this.YWallPositionsToDestroy.Any();

            }
        }
    }
}
