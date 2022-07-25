using Moritz.Globals;

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    internal class StrandArgs
    {
        public StrandArgs(int level, int density, PointF trajectoryPoint)
        {
            this.Level = level;
            this.Density = density;
            this.TrajectoryPoint = trajectoryPoint;
        }

        public int Level;
        public int Density;
        public PointF TrajectoryPoint;
    }

    internal class Trajectory
    {
        public readonly int Level = int.MinValue;
        public readonly string DensityInputKrystalName;
        public List<StrandArgs> StrandsInput = new List<StrandArgs>();

        public Trajectory(XmlElement trajectoryPathElement)
        {
            DensityInputKrystalName = trajectoryPathElement.GetAttribute("densityInputKrystal");
            string densityInputKrystalPath = M.LocalMoritzKrystalsFolder + @"\" + DensityInputKrystalName;
            var densityInputKrystal = new DensityInputKrystal(densityInputKrystalPath);
            var leveledDensityValues = densityInputKrystal.LeveledValues;
            Level = (int)densityInputKrystal.Level + 1;

            var svgPath = new SvgPath(trajectoryPathElement);
            var nodes = svgPath.Nodes;
            int nodesLevel = GetNodesLevel(densityInputKrystal.ShapeArray, nodes.Count);
            var nodeIndex = -1;

            foreach(var leveledValue in leveledDensityValues)
            {
                if(leveledValue.level <= nodesLevel)
                {
                    nodeIndex++;
                }
                Debug.Assert(nodeIndex >= 0 && nodeIndex < nodes.Count);
                var strandArgs = new StrandArgs(leveledValue.level, leveledValue.value, nodes[nodeIndex].position);
                StrandsInput.Add(strandArgs);
            }

            Debug.Assert(nodeIndex == (nodes.Count - 1));
        }

        private int GetNodesLevel(int[] shapeArray, int trajectoryNodesCount)
        {
            int nodesLevel = -1;
            for(int level = 0; level < shapeArray.Length; level++)
            {
                if(trajectoryNodesCount == shapeArray[level])
                {
                    nodesLevel = level + 1;
                    break;
                }
            }

            Debug.Assert(nodesLevel > 0, "The (input) nodes count must exist somewhere in the (output) shapeArray");

            return nodesLevel;
        }
    }
}