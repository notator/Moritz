
using Moritz.Xml;

namespace Moritz.Symbols
{
    public abstract class Beam
    {
        /// <summary>
        /// Creates a horizontal Beam whose top edge is at 0F.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="y"></param>
        public Beam(float left, float right)
        {
            LeftX = left;
            RightX = right;
            _leftTopY = 0F;
            _rightTopY = 0F;
        }

        public void MoveYs(float dLeftY, float dRightY)
        {
            _leftTopY += dLeftY;
            _rightTopY += dRightY;
        }

        public abstract void ShiftYsForBeamBlock(float outerLeftY, float gap, VerticalDir stemDirection, float beamThickness);

        /// <summary>
        /// Shifts a horizontal beam vertically to the correct position (wrt the beamBlock) for its duration class 
        /// </summary>
        /// <param name="outerLeftY"></param>
        /// <param name="gap"></param>
        /// <param name="stemDirection"></param>
        /// <param name="beamThickness"></param>
        /// <param name="nGaps"></param>
        protected void ShiftYsForBeamBlock(float outerLeftY, float gap, VerticalDir stemDirection, float beamThickness, int nGaps)
        {
            float dy = 0F;
            if(stemDirection == VerticalDir.down)
            {
                dy = -(beamThickness + (gap * nGaps));
            }
            else
            {
                dy = gap * nGaps;
            }
            dy += outerLeftY - _leftTopY;
            MoveYs(dy, dy);
        }

        /// <summary>
        /// Exposed as public function by each IBeamStub
        /// </summary>
        protected void ShearStub(float shearAxis, float tanAlpha, float stemX)
        {
            if(LeftX == stemX || RightX == stemX)
            {
                float dLeftY = (LeftX - shearAxis) * tanAlpha;
                float dRightY = (RightX - shearAxis) * tanAlpha;
                MoveYs(dLeftY, dRightY);
            }
            // else do nothing
        }

        public readonly float LeftX;
        public readonly float RightX;
        public readonly float StrokeWidth;
        public float LeftTopY { get { return _leftTopY; } }
        public float RightTopY { get { return _rightTopY; } }

        protected float _leftTopY;
        protected float _rightTopY;
    }

    internal class QuaverBeam : Beam
    {
        public QuaverBeam(float left, float right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(float outerLeftY, float gap, VerticalDir stemDirection, float beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 0);
        }
    }
    internal class SemiquaverBeam : Beam
    {
        public SemiquaverBeam(float left, float right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(float outerLeftY, float gap, VerticalDir stemDirection, float beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 1);
        }

    }
    internal class ThreeFlagsBeam : Beam
    {
        public ThreeFlagsBeam(float left, float right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(float outerLeftY, float gap, VerticalDir stemDirection, float beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 2);
        }

    }
    internal class FourFlagsBeam : Beam
    {
        public FourFlagsBeam(float left, float right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(float outerLeftY, float gap, VerticalDir stemDirection, float beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 3);
        }

    }
    internal class FiveFlagsBeam : Beam
    {
        public FiveFlagsBeam(float left, float right)
            : base(left, right)
        {
        }

        public override void ShiftYsForBeamBlock(float outerLeftY, float gap, VerticalDir stemDirection, float beamThickness)
        {
            ShiftYsForBeamBlock(outerLeftY, gap, stemDirection, beamThickness, 4);
        }
    }

    /**********************************************************************************************/
    public interface IBeamStub
    {
        DurationClass DurationClass { get; }
        void ShearBeamStub(float shearAxis, float tanAlpha, float stemX);
    }

    internal class SemiquaverBeamStub : SemiquaverBeam, IBeamStub
    {
        public SemiquaverBeamStub(float left, float right)
            : base(left, right)
        {
        }

        public void ShearBeamStub(float shearAxis, float tanAlpha, float stemX)
        {
            base.ShearStub(shearAxis, tanAlpha, stemX);
        }

        public DurationClass DurationClass { get { return DurationClass.semiquaver; } }
    }
    internal class ThreeFlagsBeamStub : ThreeFlagsBeam, IBeamStub
    {
        public ThreeFlagsBeamStub(float left, float right)
            : base(left, right)
        {
        }

        public void ShearBeamStub(float shearAxis, float tanAlpha, float stemX)
        {
            base.ShearStub(shearAxis, tanAlpha, stemX);
        }

        public DurationClass DurationClass { get { return DurationClass.threeFlags; } }
    }
    internal class FourFlagsBeamStub : FourFlagsBeam, IBeamStub
    {
        public FourFlagsBeamStub(float left, float right)
            : base(left, right)
        {
        }

        public void ShearBeamStub(float shearAxis, float tanAlpha, float stemX)
        {
            base.ShearStub(shearAxis, tanAlpha, stemX);
        }

        public DurationClass DurationClass { get { return DurationClass.fourFlags; } }
    }
    internal class FiveFlagsBeamStub : FiveFlagsBeam, IBeamStub
    {
        public FiveFlagsBeamStub(float left, float right)
            : base(left, right)
        {
        }

        public void ShearBeamStub(float shearAxis, float tanAlpha, float stemX)
        {
            base.ShearStub(shearAxis, tanAlpha, stemX);
        }

        public DurationClass DurationClass { get { return DurationClass.fiveFlags; } }
    }
}
