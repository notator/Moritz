
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    public class StrandArgs
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

    public class Trajectory
    {
        public readonly int Level = int.MinValue;
        public readonly string DensityInputKrystalName;
        public List<StrandArgs> StrandsInput = new List<StrandArgs>();

        public Trajectory(XmlElement trajectoryPathElement, int nEffectiveTrajectoryNodes, DensityInputKrystal densityInputKrystal)
        {
            if(nEffectiveTrajectoryNodes > 1)
            {
                Debug.Assert(densityInputKrystal.Level > 0, "The density input cannot be a constant."); // The trajectory must contain at least two nodes...");
            }

            DensityInputKrystalName = densityInputKrystal.Name;
            var leveledDensityValues = densityInputKrystal.LeveledValues;
            Level = (int)densityInputKrystal.Level + 1;

            var svgPath = new SvgPath(trajectoryPathElement);
            var nodes = svgPath.Nodes; // default
            if(nEffectiveTrajectoryNodes == 1)
            {
                nodes = new List<SvgNode>() { svgPath.Nodes[0] };
            }
                
            int nodesLevel = GetNodesLevel(densityInputKrystal, nodes.Count);
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

        private int GetNodesLevel(Krystal densityInputKrystal, int trajectoryNodesCount)
        {
            int nodesLevel = 1;

            if(densityInputKrystal.Level > 0)
            {
                Debug.Assert(densityInputKrystal.ShapeArray.Length > 0);

                int[] shapeArray = densityInputKrystal.ShapeArray;

                for(int level = 0; level < shapeArray.Length; level++)
                {
                    if(trajectoryNodesCount == shapeArray[level])
                    {
                        nodesLevel = level + 1;
                        break;
                    }
                }
            }

            Debug.Assert(nodesLevel > 0, "The (input) nodes count must exist somewhere in the (output) shapeArray.\n\n" +
                "In other words: The density input krystal must have a shape that includes\n" +
                "the number of nodes in the trajectory path (in the SVG input).");

            return nodesLevel;
        }
    }
}