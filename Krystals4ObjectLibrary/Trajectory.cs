using Moritz.Globals;

using System.Collections.Generic;
using System.Collections;
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
            var svgPath = new SvgPath(trajectoryPathElement);
            var nodes = svgPath.Nodes;
            var nodeIndex = 0;
            DensityInputKrystalName = trajectoryPathElement.GetAttribute("densityInputKrystal");
            string densityInputKrystalPath = M.LocalMoritzKrystalsFolder + @"\" + DensityInputKrystalName;
            var densityInputKrystal = new DensityInputKrystal(densityInputKrystalPath);
            var leveledDensityValues = densityInputKrystal.LeveledValues;

            foreach(var leveledValue in leveledDensityValues)
            {
                Debug.Assert(nodeIndex < nodes.Count);

                Level = (Level > leveledValue.level) ? Level : leveledValue.level;
                var strandArgs = new StrandArgs(leveledValue.level, leveledValue.value, nodes[nodeIndex++].position);
                StrandsInput.Add(strandArgs);
            }

            Debug.Assert(nodeIndex == nodes.Count);
        }
    }
}