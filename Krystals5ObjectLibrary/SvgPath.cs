using Moritz.Globals;

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Xml;

namespace Krystals5ObjectLibrary
{
    internal class SvgPath
    {
        public List<SvgNode> Nodes;
        public bool Closed = false;

        public SvgPath(XmlElement svgPathElem)
        {
            Debug.Assert(svgPathElem != null && svgPathElem.HasAttribute("d"));

            string d = svgPathElem.GetAttribute("d").Trim();
            Nodes = GetNodes(d);
            Closed = d.EndsWith("z") || d.EndsWith("Z");
        }

        // returns an object:
        //     object.nodes: is an array of Node objects
        //     object.closed: is a boolean that is true if the rawPathString ends with a "Z" or "z".
        private List<SvgNode> GetNodes(string rawPathString)
        {
            Debug.Assert(rawPathString.Contains(",,") == false);
            Debug.Assert(rawPathString.Contains("  ") == false);

            char[] separators = { ',', ' ' };
            string[] components = rawPathString.Split(separators); // split on both commas and whitespace
            int index = 0;
            SvgNode currentNode = new SvgNode(new PointF(0, 0), new PointF(0, 0), new PointF(0, 0));
            string currentOp = "";
            List<SvgNode> nodes = new List<SvgNode>();

            while(index < components.Length)
            {
                switch(components[index])
                {
                    case "M":
                        currentOp = "M";
                        index++;
                        break;
                    case "m":
                        currentOp = "m";
                        index++;
                        break;
                    case "C":
                        currentOp = "C";
                        index++;
                        break;
                    case "c":
                        currentOp = "c";
                        index++;
                        break;
                    case "L":
                        currentOp = "L";
                        index++;
                        break;
                    case "l":
                        currentOp = "l";
                        index++;
                        break;
                    case "H":
                        currentOp = "H";
                        index++;
                        break;
                    case "h":
                        currentOp = "h";
                        index++;
                        break;
                    case "V":
                        currentOp = "V";
                        index++;
                        break;
                    case "v":
                        currentOp = "v";
                        index++;
                        break;
                    case "Z":
                        currentOp = "Z";
                        // Do "Z" here, since the while loop will now break.
                        ClosePath(nodes, currentNode);
                        index++;
                        break;
                    case "z":
                        currentOp = "z";
                        // Do "z" here, since the while loop will now break.
                        ClosePath(nodes, currentNode);
                        index++;
                        break;
                    default:
                        currentNode = GetCurrentNode(currentNode, currentOp, components, ref index);
                        nodes.Add(currentNode);
                        break;
                }
            }

            return nodes;
        }

        private void ClosePath(List<SvgNode> localNodes, SvgNode currentNode)
        {
            bool IsNear(PointF thisPoint, PointF otherPoint)
            {
                bool near = false;
                float delta = 0.00002F,
                    xMax = thisPoint.X + delta,
                    xMin = thisPoint.X - delta,
                    yMax = thisPoint.Y + delta,
                    yMin = thisPoint.Y - delta,
                    x = otherPoint.X,
                    y = otherPoint.Y;

                if(x <= xMax && x >= xMin && y <= yMax && y >= yMin)
                {
                    near = true;
                }

                return near;
            }
            if(IsNear(currentNode.position, localNodes[0].position))
            {
                localNodes.RemoveAt(localNodes.Count - 1);
                localNodes[0].inControlPoint = currentNode.inControlPoint;
            }
        }

        private SvgNode GetCurrentNode(SvgNode currentSvgNode, string currentOp, string[] components, ref int index)
        {
            PointF pointF;
            List<PointF> pointFList;
            float f = 0;

            switch(currentOp)
            {
                case "M":
                    pointF = GetPointF(components, ref index);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                case "m":
                    pointF = GetPointF(components, ref index);
                    pointF = new PointF(currentSvgNode.position.X + pointF.X, currentSvgNode.position.Y + pointF.Y);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                case "C":
                    pointFList = GetThreePointFs(components, ref index);
                    currentSvgNode.outControlPoint = new PointF(pointFList[0].X, pointFList[0].Y);
                    pointF = new PointF(pointFList[2].X, pointFList[2].Y);
                    currentSvgNode = new SvgNode(new PointF(pointFList[1].X, pointFList[1].Y), pointFList[0], pointFList[0]);
                    break;
                case "c":
                    pointFList = GetThreePointFs(components, ref index);
                    currentSvgNode.outControlPoint = new PointF(currentSvgNode.position.X + pointFList[0].X, currentSvgNode.position.Y + pointFList[0].Y);
                    pointF = new PointF(currentSvgNode.position.X + pointFList[2].X, currentSvgNode.position.Y + pointFList[2].Y); 
                    currentSvgNode = new SvgNode(new PointF(currentSvgNode.position.X + pointFList[1].X, currentSvgNode.position.Y + pointFList[1].Y), pointF, pointF);
                    break;
                case "L":
                    pointF = GetPointF(components, ref index);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                case "l":
                    pointF = GetPointF(components, ref index);
                    pointF = new PointF(currentSvgNode.position.X + pointF.X, currentSvgNode.position.Y + pointF.Y);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                case "H":
                    f = float.Parse(components[index++]);
                    pointF = new PointF(f, currentSvgNode.position.Y);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                case "h":
                    f = float.Parse(components[index++]);
                    pointF = new PointF(currentSvgNode.position.X + f, currentSvgNode.position.Y);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                case "V":
                    f = float.Parse(components[index++]);
                    pointF = new PointF(currentSvgNode.position.X, f);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                case "v":
                    f = float.Parse(components[index++]);
                    pointF = new PointF(currentSvgNode.position.X, currentSvgNode.position.Y + f);
                    currentSvgNode = new SvgNode(pointF, pointF, pointF);
                    break;
                default:
                    Debug.Assert(false, "unknown operator");
                    break;
            }

            return currentSvgNode;
        }

        private List<PointF> GetThreePointFs(string[] components, ref int index)
        {
            float x1 = float.Parse(components[index++], M.En_USNumberFormat),
                y1 = float.Parse(components[index++], M.En_USNumberFormat),
                x2 = float.Parse(components[index++], M.En_USNumberFormat),
                y2 = float.Parse(components[index++], M.En_USNumberFormat),
                x3 = float.Parse(components[index++], M.En_USNumberFormat),
                y3 = float.Parse(components[index++], M.En_USNumberFormat);

            List<PointF> rval = new List<PointF>();
            rval.Add(new PointF(x1, y1));
            rval.Add(new PointF(x2, y2));
            rval.Add(new PointF(x3, y3));

            return rval;
        }

        private PointF GetPointF(string[] components, ref int index)
        {
            float x = float.Parse(components[index++], M.En_USNumberFormat),
                y = float.Parse(components[index++], M.En_USNumberFormat);

            return new PointF(x, y);
        }
    }
}