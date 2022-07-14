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
        public StrandArgs(uint level, uint density, PointF trajectoryPoint)
        {
            this.Level = level;
            this.Density = density;
            this.TrajectoryPoint = trajectoryPoint;
        }

        public uint Level;
        public uint Density;
        public PointF TrajectoryPoint;
    }

    internal class Trajectory
    {
        public List<StrandArgs> StrandsInput = new List<StrandArgs>();

        public Trajectory(XmlElement trajectoryPathElement)
        {
            var svgPath = new SvgPath(trajectoryPathElement);
            var nodes = svgPath.Nodes;
            var nodeIndex = 0;
            string densityInputKrystalName = trajectoryPathElement.GetAttribute("densityInputKrystal");
            string densityInputKrystalPath = M.LocalMoritzKrystalsFolder + @"\" + densityInputKrystalName;
            var densityInputKrystal = new DensityInputKrystal(densityInputKrystalPath);
            var leveledDensityValues = densityInputKrystal.LeveledValues;
            

            foreach(var leveledValue in leveledDensityValues)
            {
                var strandArgs = new StrandArgs((uint)leveledValue.level, (uint)leveledValue.value, nodes[nodeIndex++].position);
                StrandsInput.Add(strandArgs);
            }

            Debug.Assert(nodeIndex == nodes.Count);
        }
    }
}