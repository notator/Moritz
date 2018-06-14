using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Symbols
{
    public class HLine
    {
        /// <summary>
        /// Coordinates of a horizontal line segment.
        /// </summary>
        /// <param name="leftX">The left x-coordinate</param>
        /// <param name="rightX">The right x-coordinate</param>
        /// <param name="y">The y-coordinate</param>
        public HLine(float leftX, float rightX, float y)
        {
            Debug.Assert(leftX < rightX);
            Left = leftX;
            Right = rightX;
            Y = y;
        }

        public Dictionary<float, HLine> Split(float x)
        {
            Debug.Assert(Left < x && Right > x);

            Dictionary<float, HLine> linesDict = new Dictionary<float, HLine>();
            HLine line1 = new HLine(Left, x, Y);
            HLine line2 = new HLine(x, Right, Y);
            linesDict.Add(line1.Left, line1);
            linesDict.Add(line2.Left, line2);
            return linesDict;
        }

        public Dictionary<float, HLine> Split(List<float> Xs)
        {
            #region conditions
            // Xs can be empty, in which case this function returns the original line in the dict.
            foreach(float x in Xs)
            {
                Debug.Assert(x > Left && x < Right);
            }
            if(Xs.Count > 1)
            {
                for(int i = 1; i < Xs.Count; ++i)
                {
                    Debug.Assert(Xs[i - 1] < Xs[i]);
                }
            }
            #endregion conditions;

            Dictionary<float, HLine> lines = new Dictionary<float, HLine>();
			List<float> allXs = new List<float>
			{
				Left
			};
			allXs.AddRange(Xs);
            allXs.Add(Right);
            for(int i = 1; i < allXs.Count; ++i)
            {
                HLine line = new HLine(allXs[i - 1], allXs[i], Y);
                lines.Add(line.Left, line);
            }
            return lines;
        }


        public float Left;
        public float Right;
        public float Y;
    }
    internal abstract class HorizontalEdge
    {
        /// <summary>
        /// A sequence of HLines (horizontal line segments) ordered from left to right horizontally.
        /// Each HLine has its own y-coordinate. Its right x-coordinate is the same as the
        /// left x-coordinate of the following HLine. 
        /// </summary>
        protected HorizontalEdge()
        {
        }

        public abstract float YatX(float X);
 
        /// <summary>
        /// returns the y-coordinate of this BottomEdge at X.
        /// If X greater than or equal to the leftX of an HLine, and less than the rightX of the same HLine,
        /// then the returned value is HLine.Y.
        /// Else If X is equal to the rightX of an Hline and there is another Hline to the right,
        /// {
        ///     if isTopEdge is true, then the returned value is the higher (=smaller) of the two Ys,
        ///     else the returned value is the lower (=greater) of the two Ys
        /// }
        /// </summary>
        protected float YatX(float X, bool isTopEdge)
        {
            float y = float.MaxValue;
            for(int i = 0; i < this.Lines.Count; ++i)
            {
                HLine hLine = this.Lines[i];
                if(hLine.Left <= X && hLine.Right > X)
                {
                    y = hLine.Y;
                    break;
                }
                else if(hLine.Right == X)
                {
                    if(i < this.Lines.Count - 1)
                    {
                        HLine nextHLine = this.Lines[i + 1];
                        if(isTopEdge)
                            y = nextHLine.Y < hLine.Y ? nextHLine.Y : hLine.Y;
                        else
                            y = nextHLine.Y > hLine.Y ? nextHLine.Y : hLine.Y;
                        break;
                    }
                    else
                    {
                        y = hLine.Y;
                        break;
                    }
                }
            }
            Debug.Assert(y != float.MaxValue);
            return y;
        }
        
        /// <summary>
        /// Adds either the top or the bottom of the metrics object to this horizontal edge,
        /// depending on whether this is a top or bottom edge.
        /// </summary>
        /// <param name="metrics"></param>
        public abstract void Add(Metrics metrics);
        /// <summary>
        /// Adds the hLine to the edge line.
        /// </summary>
        /// <param name="hLine"></param>
        public abstract void Add(HLine hLine);

        /// <summary>
        /// Adds all the staff's metrics, except for the top or bottom staffline.
        /// </summary>
        /// <param name="staff"></param>
        protected void AddStaffMetrics(Staff staff)
        {
            foreach(Voice voice in staff.Voices)
            {
                foreach(NoteObject noteObject in voice.NoteObjects)
                {
					Clef clef = noteObject as Clef;

					if(noteObject is OutputChordSymbol chordSymbol)
					{
						chordSymbol.ChordMetrics.AddToEdge(this);
					}
					else if(clef != null && clef.ClefType != "n")
					{
						Add(clef.Metrics);
					}
					else
					{
						Add(noteObject.Metrics);
					}

					EndBarline endBarline = noteObject as EndBarline;
					if(noteObject is Barline barline && endBarline == null)
					{
						BarlineMetrics barlineMetrics = barline.Metrics as BarlineMetrics;
						if(barlineMetrics.BarnumberMetrics != null)
						{
							Add(barlineMetrics.BarnumberMetrics);
						}
						if(barlineMetrics.StaffNameMetrics != null)
						{
							Add(barlineMetrics.StaffNameMetrics);
						}
					}
				}
            }
        }


        /// <summary>
        /// Each line has its own y-coordinate.
        /// The right x-coordinate is the same as the left x-coordinate of the following line.
        /// </summary>
        public List<HLine> Lines = new List<HLine>();
    }

    internal class TopEdge : HorizontalEdge
    {
        /// <summary>
        /// The top edge of a staff.
        /// </summary>
        public TopEdge(Staff staff, float leftMargin, float rightMargin)
            : base()
        {
            StafflineMetrics topStaffLineMetrics = new StafflineMetrics(leftMargin, rightMargin, staff.Metrics.OriginY);
            Add(topStaffLineMetrics);
            AddStaffMetrics(staff);
        }

        /// <summary>
        /// Creates a TopEdge by inverting the bottomEdge
        /// </summary>
        public TopEdge(BottomEdge bottomEdge)
            : base()
        {
            foreach(HLine line in bottomEdge.Lines)
            {
                HLine fLine = new HLine(line.Left, line.Right, -line.Y);
                Lines.Add(fLine);
            }
        }

        /// <summary>
        /// returns the y-coordinate of this TopEdge at X.
        /// If X greater than the leftX of an HLine, and less than the rightX of the same HLine,
        /// then the returned value is HLine.Y.
        /// If X is equal to the rightX of an Hline and there is another Hline to the right,
        /// then the returned value is the **higher** (=smaller) Y-value of the two HLines
        /// If X is equal to the rightX of an Hline and there is no other HLine to the right,
        /// then the returned value is HLine.Y
        /// </summary>
        public override float YatX(float X)
        {
            return YatX(X, true);
        }

        /// <summary>
        /// If the top edge of the metrics object lies above any line in this top edge,
        /// then this top edge is adjusted accordingly.
        /// </summary>
        public override void Add(Metrics metrics)
        {
            HLine hLine = new HLine(metrics.Left, metrics.Right, metrics.Top);
            AddLineToUpperEdge(hLine);
        }

        /// <summary>
        /// If the line lies above any line in this top edge,
        /// then this top edge is adjusted accordingly.
        /// </summary>
        public override void Add(HLine hLine)
        {
            AddLineToUpperEdge(hLine);
        }

        public void AddLineToUpperEdge(HLine newLine)
        {
            Debug.Assert(newLine.Y != float.MaxValue);
            if(Lines.Count == 0)
            {
                Lines.Add(newLine);
            }
            else
            {
                float leftY = this.YatX(newLine.Left);
                float rightY = this.YatX(newLine.Right);
                Dictionary<float, HLine> splitEdge = SplitEdge(newLine, leftY, rightY);
                List<float> splitXsOnNewLine = SplitXsOnNewLine(newLine);
                Dictionary<float, HLine> splitNewLine = newLine.Split(splitXsOnNewLine); // if 

                List<HLine> newLines = new List<HLine>();
                float currentX = splitEdge[0].Left;
                foreach(float x in splitEdge.Keys)
                {
                    if(splitNewLine.ContainsKey(x))
                    {
                        if(newLine.Y < splitEdge[x].Y)
                        {
                            newLines.Add(splitNewLine[x]);
                            currentX = splitNewLine[x].Right;
                        }
                        else
                        {
                            newLines.Add(splitEdge[x]);
                            currentX = splitEdge[x].Right;
                        }
                    }
                    else if(splitEdge[x].Left == currentX)
                    {
                        newLines.Add(splitEdge[x]);
                        currentX = splitEdge[x].Right;
                    }
                }

                for(int i = 1; i < newLines.Count; ++i)
                {
                    Debug.Assert(newLines[i - 1].Right == newLines[i].Left);
                }

                Lines = null;
                Lines = newLines;
            }
        }

        /// <summary>
        /// The HLines from this edge, with the HLines at the beginning and end of the newLine split into two. 
        /// The dictionary's key is the left edge of each HLine.
        /// </summary>
        private Dictionary<float, HLine> SplitEdge(HLine newLine, float leftY, float rightY)
        {
            Debug.Assert(Lines.Count > 0);

            Dictionary<float, HLine> splitEdge = new Dictionary<float, HLine>();
            foreach(HLine hline in Lines)
            {
                if(hline.Left < newLine.Left && hline.Right > newLine.Left && leftY > newLine.Y)
                {
                    Dictionary<float, HLine> splitLines1 = hline.Split(newLine.Left);
                    Dictionary<float, HLine> splitLines2 = new Dictionary<float, HLine>();
                    foreach(float x in splitLines1.Keys)
                    {
                        HLine line = splitLines1[x];
                        if(line.Left < newLine.Right && line.Right > newLine.Right && line.Y > newLine.Y)
                        {
                            splitLines2 = line.Split(newLine.Right);
                        }
                    }
                    foreach(float x in splitLines1.Keys)
                    {
                        if(!splitLines2.ContainsKey(x))
                            splitEdge.Add(splitLines1[x].Left, splitLines1[x]);
                    }

                    foreach(float x in splitLines2.Keys)
                    {
                        splitEdge.Add(splitLines2[x].Left, splitLines2[x]);
                    }

                }
                else if(hline.Left < newLine.Right && hline.Right > newLine.Right && rightY > newLine.Y)
                {
                    Dictionary<float, HLine> splitLines = hline.Split(newLine.Right);
                    foreach(float x in splitLines.Keys)
                    {
                        splitEdge.Add(splitLines[x].Left, splitLines[x]);
                    }
                }
                else
                {
                    splitEdge.Add(hline.Left, hline);
                }
            }

            return splitEdge;
        }

        /// <summary>
        /// The x-coordinates on the newLine which intersect the verticals between the top Lines
        /// (The edge considered as a continuous, joined up line).
        /// This does NOT include the end points of the newLine.
        /// </summary>
        private List<float> SplitXsOnNewLine(HLine newLine)
        {
            List<float> splitXs = new List<float>();
            for(int i = 1; i < Lines.Count; ++i)
            {
                HLine leftEdgeLine = Lines[i - 1];
                HLine rightEdgeLine = Lines[i];
                Debug.Assert(leftEdgeLine.Right == rightEdgeLine.Left);

                if(leftEdgeLine.Right > newLine.Left && leftEdgeLine.Right < newLine.Right)
                {
                    if((leftEdgeLine.Y >= newLine.Y && rightEdgeLine.Y <= newLine.Y)
                        || (leftEdgeLine.Y <= newLine.Y && rightEdgeLine.Y >= newLine.Y))
                    {
                        splitXs.Add(leftEdgeLine.Right);
                    }
                }
            }

            return splitXs;
        }


        public List<HLine> FlippedLines
        {
            get
            {
                List<HLine> fLines = new List<HLine>();
                foreach(HLine line in Lines)
                {
                    HLine fLine = new HLine(line.Left, line.Right, -line.Y);
                    fLines.Add(fLine);
                }
                return fLines;
            }
        }

        /// <summary>
        /// Returns the (minimum) vertical distance to the bottom edge above.
        /// This top edge and the bottom edge above belong to two diferent objects, for example two staves or two systems.
        /// The returned value is positive if this top edge is completely below the bottomEdge argument.
        /// </summary>
        /// <param name="currentMinDist"></param>
        /// <param name="rightEdge"></param>
        /// <returns></returns>
        public float DistanceToEdgeAbove(BottomEdge bottomEdge)
        {
            float minDist = float.MaxValue;
            HashSet<float> allLeftXs = AllLeftXsInBothEdges(bottomEdge);
            foreach(float leftX in allLeftXs)
            {
                float bottomEdgeYatLeftX = bottomEdge.YatX(leftX);
                float topEdgeYatLeftX = this.YatX(leftX);
                float dist = topEdgeYatLeftX - bottomEdgeYatLeftX;
                minDist = minDist < dist ? minDist : dist;
            }
            return minDist;
        }

        private HashSet<float> AllLeftXsInBothEdges(BottomEdge bottomEdge)
        {
            HashSet<float> allXs = new HashSet<float>();
            foreach(HLine ht in Lines)
            {
                allXs.Add(ht.Left);
            }

            foreach(HLine ht in bottomEdge.Lines)
            {
                if(!allXs.Contains(ht.Left))
                {
                    allXs.Add(ht.Left);
                }
            }

            return allXs;
        }
    }
    internal class BottomEdge : HorizontalEdge
    {
        /// <summary>
        /// The bottom edge of a staff.
        /// </summary>
        public BottomEdge(Staff staff, float left, float right, float gap)
            : base()
        {
            StafflineMetrics bottomStafflineMetrics = new StafflineMetrics(left, right,
                staff.Metrics.OriginY + (gap * (staff.NumberOfStafflines - 1)));
            Add(bottomStafflineMetrics);
            AddStaffMetrics(staff);
        }

        /// <summary>
        /// returns the y-coordinate of this BottomEdge at X.
        /// If X greater than the leftX of an HLine, and less than the rightX of the same HLine,
        /// then the returned value is HLine.Y.
        /// If X is equal to the rightX of an Hline and there is another Hline to the right,
        /// then the returned value is the **lower** (=greater) Y-value of the two HLines
        /// If X is equal to the rightX of an Hline and there is no other HLine to the right,
        /// then the returned value is HLine.Y
        /// </summary>
        public override float YatX(float X)
        {
            return YatX(X, false);
        }

        /// <summary>
        /// If the bottom edge of the metrics object lies below any line in this bottom edge,
        /// then this bottom edge is adjusted accordingly.
        /// </summary>
        public override void Add(Metrics metrics)
        {
            HLine line = new HLine(metrics.Left, metrics.Right, metrics.Bottom);
            Add(line);
        }
        /// <summary>
        /// If the hLine lies below any line in this bottom edge,
        /// then this bottom edge is adjusted accordingly.
        /// </summary>
        public override void Add(HLine hLine)
        {
            if(Lines.Count == 0)
            {
                Lines.Add(hLine);
            }
            else
            {
                TopEdge flippedEdge = FlipVertically();

                HLine flippedLine = new HLine(hLine.Left, hLine.Right, -hLine.Y);
                flippedEdge.AddLineToUpperEdge(flippedLine);

                Lines = flippedEdge.FlippedLines;
            }
        }

        private TopEdge FlipVertically()
        {
            Debug.Assert(Lines.Count > 0);
            return new TopEdge(this);
        }
    }

}
