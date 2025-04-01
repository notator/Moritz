using Moritz.Globals;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Moritz.Symbols
{
    public abstract class Metrics
    {
        protected Metrics(CSSObjectClass cssObjectClass)
        {
            _cssObjectClass = cssObjectClass;
            if(!_usedCSSObjectClasses.Contains(cssObjectClass))
            {
                _usedCSSObjectClasses.Add(cssObjectClass);
            }
        }

        protected Metrics(CSSObjectClass cssObjectClass1, CSSObjectClass cssObjectClass2)
        {
            if(!_usedCSSObjectClasses.Contains(cssObjectClass1))
            {
                _usedCSSObjectClasses.Add(cssObjectClass1);
            }
            if(!_usedCSSObjectClasses.Contains(cssObjectClass2))
            {
                _usedCSSObjectClasses.Add(cssObjectClass2);
            }
        }

        public static void ClearUsedCSSClasses()
        {
            _usedCSSObjectClasses.Clear();
            _usedCSSColorClasses.Clear();
        }

        public virtual void Move(float dx, float dy)
        {
            _left += dx;
            _top += dy;
            _right += dx;
            _bottom += dy;
            _originX += dx;
            _originY += dy;
        }

        public abstract void WriteSVG(SvgWriter w);

        /// <summary>
        /// Use this function to check all atomic overlaps (single character or line boxMetrics).
        /// </summary>
        /// <param name="metrics"></param>
        /// <returns></returns>
        public bool Overlaps(Metrics metrics)
        {
            bool verticalOverlap = true;
            bool horizontalOverlap = true;
            if((metrics.Top > Bottom) || (metrics.Bottom < Top))
                verticalOverlap = false;
            if((metrics.Left > Right) || (metrics.Right < Left))
                horizontalOverlap = false;

            return verticalOverlap && horizontalOverlap;
        }

        /// <summary>
        /// If the previousMetrics overlaps this metrics vertically, and this.Left is les than 
        /// than previousMetrics.Right, previousMetrics.Right - this.Left is returned. 
        /// If there is no vertical overlap, this or the previousMetrics is a RestMetrics, and
        /// the metrics overlap, half the width of the rest is returned.
        /// Otherwise float.MinValue is returned.
        /// </summary>
        public float OverlapWidth(Metrics previousMetrics)
        {
            bool verticalOverlap = true;
            if(!(this is Barline_LineMetrics))
            {
                if((previousMetrics.Top > Bottom) || (previousMetrics.Bottom < Top))
                    verticalOverlap = false;
            }

            float overlap = float.MinValue;
            if(verticalOverlap && previousMetrics.Right > Left)
            {
                overlap = previousMetrics.Right - this.Left;
                if(this is AccidentalMetrics aMetrics && (aMetrics.CharacterString == "b" || aMetrics.CharacterString == "n"))
                {
                    overlap -= ((this.Right - this.Left) * 0.15F);
                    overlap = overlap > 0F ? overlap : 0F;
                }
            }

            if(!verticalOverlap && this is RestMetrics && this.OriginX <= previousMetrics.Right)
                overlap = previousMetrics.Right - this.OriginX;

            if(!verticalOverlap && previousMetrics is RestMetrics && previousMetrics.OriginX >= Left)
                overlap = previousMetrics.OriginX - Left;


            return overlap;
        }

        /// <summary>
        /// Returns the positive horizontal distance by which this metrics overlaps the argument metrics.
        /// The result can be 0, if previousMetrics.Right = this.Metrics.Left.
        /// If there is no overlap, float.MinValue is returned.
        /// </summary>
        public float OverlapWidth(AnchorageSymbol previousAS)
        {
            float overlap = float.MinValue;

            if(previousAS is ChordSymbol chord)
            {
                overlap = chord.ChordMetrics.OverlapWidth(this);
            }
            else
            {
                overlap = this.OverlapWidth(previousAS.Metrics);
            }
            return overlap;
        }

        /// <summary>
        /// If the padded argument overlaps this metrics horizontally, 
        /// and this.Top is smaller than (=above) paddedArgument.Bottom,
        /// then paddedArgument.Bottom - this.Top (a positive value) is returned. 
        /// Otherwise 0F is returned.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public float OverlapHeight(Metrics arg, float padding)
        {
            float newArgRight = arg.Right + padding;
            float newArgLeft = arg.Left - padding;
            float newArgBottom = arg.Bottom + padding;

            bool horizontalOverlap = true;
            if((newArgRight < Left) || (newArgLeft > Right))
                horizontalOverlap = false;

            if(horizontalOverlap && newArgBottom > Top)
            {
                float overlap = newArgBottom - Top;
                return (overlap);
            }
            else
                return 0F;
        }

        public void SetTop(float top)
        {
            _top = top;
        }

        public void SetBottom(float bottom)
        {
            _bottom = bottom;
        }

        protected void MoveAboveTopBoundary(float topBoundary, float padding)
        {
            M.Assert(padding >= 0.0F);
            float newBottom = topBoundary - padding;
            Move(0F, newBottom - Bottom);
        }
        protected void MoveBelowBottomBoundary(float bottomBoundary, float padding)
        {
            M.Assert(padding >= 0.0F);
            float newTop = bottomBoundary + padding;
            Move(0F, newTop - Top);
        }

        /// <summary>
        /// The actual position of the top edge of the object in the score.
        /// </summary>
        public float Top { get { return _top; } }
        protected float _top = 0F;
        /// <summary>
        /// The actual position of the right edge of the object in the score.
        /// </summary>
        public float Right { get { return _right; } }
        protected float _right = 0F;
        /// <summary>
        /// The actual position of the bottom edge of the object in the score.
        /// </summary>
        public virtual float Bottom { get { return _bottom; } }
        protected float _bottom = 0F;
        /// <summary>
        /// The actual position of the left edge of the object in the score.
        /// </summary>
        public float Left { get { return _left; } }
        protected float _left = 0F;

        /// <summary>
        /// The actual position of the object's x-origin in the score.
        /// This is the value written into the SvgScore.
        /// </summary>
        public float OriginX { get { return _originX; } }
        protected float _originX = 0F;
        /// <summary>
        /// The actual position of the object's y-origin in the score
        /// This is the value written into the SvgScore.
        /// </summary>
        public float OriginY { get { return _originY; } }
        protected float _originY = 0F;


        public CSSObjectClass CSSObjectClass { get => _cssObjectClass; }
        private readonly CSSObjectClass _cssObjectClass;
        public CSSColorClass CSSColorClass
        {
            get => _cssColorClass;
            protected set
            {
                _cssColorClass = value;
                if(!_usedCSSColorClasses.Contains(value))
                {
                    _usedCSSColorClasses.Add(value);
                }
            }
        }
        private CSSColorClass _cssColorClass = CSSColorClass.none;

        public static IReadOnlyList<CSSObjectClass> UsedCSSObjectClasses { get => _usedCSSObjectClasses as IReadOnlyList<CSSObjectClass>; }
        private static List<CSSObjectClass> _usedCSSObjectClasses = new List<CSSObjectClass>();
        public static IReadOnlyList<CSSColorClass> UsedCSSColorClasses { get => _usedCSSColorClasses as IReadOnlyList<CSSColorClass>; }
        private static List<CSSColorClass> _usedCSSColorClasses = new List<CSSColorClass>();
    }

    public class PaddedMetrics : Metrics
    {
        public PaddedMetrics(Metrics embeddedMetrics, float topPadding, float rightPadding, float bottomPadding, float leftPadding)
            : base(embeddedMetrics.CSSObjectClass)
        {
            _top = embeddedMetrics.Top - topPadding;
            _right = embeddedMetrics.Right + rightPadding;
            _bottom = embeddedMetrics.Bottom + bottomPadding;
            _left = embeddedMetrics.Left - leftPadding;
            _originX = embeddedMetrics.OriginX;
            _originY = embeddedMetrics.OriginY;

            TopPadding = topPadding;
            RightPadding = rightPadding;
            BottomPadding = bottomPadding;
            LeftPadding = leftPadding;

            EmbeddedMetrics = embeddedMetrics;
        }

        public override void WriteSVG(SvgWriter w)
        {
            throw new NotImplementedException();
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            EmbeddedMetrics.Move(dx, dy);
        }

        public float TopPadding { get; }
        public float RightPadding { get; }
        public float BottomPadding { get; }
        public float LeftPadding { get; }

        private Metrics EmbeddedMetrics { get; }
    }
}
