using System;
using System.Drawing;

namespace Krystals5ObjectLibrary
{
    public class SvgNode : ICloneable
    {
        public PointF inControlPoint;
        public PointF outControlPoint;
        public PointF position;

        // The arguments are cloned internally,
        // so that Nodes always own their own Points.
        // (Moving a node never moves another Node!)
        public SvgNode(PointF inControlPoint, PointF outControlPoint, PointF position)
        {
            this.inControlPoint = new PointF(inControlPoint.X, inControlPoint.Y);
            this.outControlPoint = new PointF(outControlPoint.X, outControlPoint.Y);
            this.position = new PointF(position.X, position.Y);
        }

        public object Clone()
        {
            return new SvgNode(this.inControlPoint, this.outControlPoint, this.position);
        }

        // Moves this node by adding dx and dy to all its coordinates.
        public void Move(float dx, float dy)
        {
            this.inControlPoint.X += dx;
            this.outControlPoint.X += dx;
            this.position.X += dx;

            this.inControlPoint.Y += dy;
            this.outControlPoint.Y += dy;
            this.position.Y += dy;
        }
    }
}