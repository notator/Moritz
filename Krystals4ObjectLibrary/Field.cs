using Moritz.Globals;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Xml;

namespace Krystals4ObjectLibrary
{
    public class Field
    {
        public readonly string FieldType; // "linear", "circular", etc.
        public readonly double Domain;
        public readonly List<PointF> Foci = new List<PointF>();
        public readonly List<string> Values = new List<string>();

        public Field(XmlElement svgPathElem)
        {
            var svgPath = new SvgPath(svgPathElem);
            var valuesArray = svgPathElem.GetAttribute("values").Split(' ');
            var nodes = svgPath.Nodes;

            Debug.Assert(valuesArray.Length == nodes.Count);

            Domain = nodes.Count;

            for(var i = 0; i < Domain; i++)
            {
                Foci.Add(new PointF(nodes[i].position.X, nodes[i].position.Y));
                Values.Add(valuesArray[i]);
            }

            FieldType = svgPathElem.GetAttribute("fieldType");
        }
    }
}