using Moritz.Globals;
using Moritz.Xml;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Moritz.Symbols
{
    /// <summary>
    /// Barlines maintain their line and drawObjects metrics separately.
    /// The lines are drawn using implementations of an abstract function,
    /// The drawObjects are drawn by calling BarlineDrawObjectsMetrics.WriteSVG().  
    /// </summary>
    public abstract class Barline : AnchorageSymbol
    {
        protected Barline(Voice voice)
            : base(voice)
        {
        }


        /// <summary>
        /// This function should not be called.
        /// Call the other WriteSVG(...) function to write the barline's vertical line(s),
        /// and WriteDrawObjectsSVG(...) to write any DrawObjects. 
        /// </summary>
        public override void WriteSVG(SvgWriter w)
        {
            throw new ApplicationException();
        }

        public abstract void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem);

        public abstract void CreateMetrics(Graphics graphics);

        protected void SetCommonMetrics(Graphics graphics, List<DrawObject> drawObjects)
        {
            StaffMetrics staffMetrics = Voice.Staff.Metrics;
            foreach(DrawObject drawObject in DrawObjects)
            {
                if(drawObject is StaffNameText staffNameText)
                {
                    StaffNameMetrics = new StaffNameMetrics(CSSObjectClass.staffName, graphics, staffNameText.TextInfo);
                    // move the staffname vertically to the middle of this staff
                    float staffheight = staffMetrics.StafflinesBottom - staffMetrics.StafflinesTop;
                    float dy = (staffheight * 0.5F) + (Gap * 0.8F);
                    StaffNameMetrics.Move(0F, dy);
                }
                if(drawObject is FramedBarNumberText framedBarNumberText)
                {
                    BarnumberMetrics = new BarnumberMetrics(graphics, framedBarNumberText.TextInfo, framedBarNumberText.FrameInfo);
                    // move the bar number to its default (=lowest) position above this staff.
                    BarnumberMetrics.Move(0F, staffMetrics.StafflinesTop - BarnumberMetrics.Bottom - (Gap * 3));
                }
            }
        }

        public abstract void AddMetricsToEdge(HorizontalEdge horizontalEdge);
        protected void AddBasicMetricsToEdge(HorizontalEdge horizontalEdge)
        {
            if(StaffNameMetrics != null)
            {
                horizontalEdge.Add(StaffNameMetrics);
            }
            if(BarnumberMetrics != null)
            {
                horizontalEdge.Add(BarnumberMetrics);
            }
        }

        protected void MoveBarnumberAboveRegionBox(BarnumberMetrics barnumberMetrics, FramedRegionInfoMetrics regionInfoMetrics)
        {
            if(barnumberMetrics != null && regionInfoMetrics != null)
            {
                float padding = Gap * 1.5F;
                float shift = barnumberMetrics.Bottom - regionInfoMetrics.Top + padding;
                barnumberMetrics.Move(0, -shift);
            }
        }

        protected void MoveFramedTextBottomToDefaultPosition(Metrics framedTextMetrics)
        {
            float staffTop = this.Voice.Staff.Metrics.StafflinesTop;
            float defaultBottom = staffTop - (Gap * 3);
            if(framedTextMetrics != null)
            {
                framedTextMetrics.Move(0, defaultBottom - framedTextMetrics.Bottom);
            }
        }

        protected void MoveFramedTextAboveNoteObjects(Metrics framedTextMetrics, List<NoteObject> fixedNoteObjects)
        {
            if(framedTextMetrics != null)
            {
                float bottomPadding = Gap * 1.5F;
                float xPadding = Gap * 4;
                PaddedMetrics paddedMetrics = new PaddedMetrics(framedTextMetrics, 0F, xPadding, bottomPadding, xPadding);

                foreach(NoteObject noteObject in fixedNoteObjects)
                {
                    int overlaps = OverlapsHorizontally(paddedMetrics, noteObject);
                    if(overlaps == 0)
                    {
                        MovePaddedMetricsAboveNoteObject(paddedMetrics, noteObject);
                    }
                    else if(overlaps == 1) // noteObject is left of framedText
                    {
                        if(noteObject is ChordSymbol chordSymbol)
                        {
                            if(chordSymbol.Stem.Direction == VerticalDir.up && chordSymbol.BeamBlock != null)
                            {
                                MoveFramedTextAboveBeamBlock(framedTextMetrics, chordSymbol.BeamBlock);
                            }
                            else if(chordSymbol.ChordMetrics.NoteheadExtendersMetrics != null)
                            {
                                MoveFramedTextAboveNoteheadExtenders(framedTextMetrics, chordSymbol.ChordMetrics.NoteheadExtendersMetrics);
                            }
                        }
                    }
                    else if(overlaps == -1) // noteObject is right of framed text, so we need look no further in these noteObjects.
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// returns
        /// -1 if metrics is entirely to the left of the fixedNoteObject;
        /// 0 if metrics overlaps the fixedNoteObject;
        /// 1 if metrics is entirely to the right of the fixedNoteObject;
        /// </summary>
        /// <returns></returns>
        private int OverlapsHorizontally(Metrics metrics, NoteObject fixedNoteObject)
        {
            int rval = 0;
            Metrics fixedMetrics = fixedNoteObject.Metrics;
            if(metrics.Right < fixedMetrics.Left)
            {
                rval = -1;
            }
            else if(metrics.Left > fixedMetrics.Right)
            {
                rval = 1;
            }
            return rval;
        }
        /// <summary>
        /// Move paddedMetrics above the fixedNoteObject if it is not already.
        /// </summary>
        private void MovePaddedMetricsAboveNoteObject(PaddedMetrics paddedMetrics, NoteObject fixedNoteObject)
        {
            float verticalOverlap = 0F;
            if(fixedNoteObject.Metrics is ChordMetrics chordMetrics)
            {
                verticalOverlap = chordMetrics.OverlapHeight(paddedMetrics, 0F);
            }
            else if(fixedNoteObject.Metrics is RestMetrics restMetrics)
            {
                verticalOverlap = restMetrics.OverlapHeight(paddedMetrics, 0F);
            }
            else if(!(fixedNoteObject is Barline))
            {
                verticalOverlap = fixedNoteObject.Metrics.OverlapHeight(paddedMetrics, 0F);
            }

            if(verticalOverlap > 0)
            {
                verticalOverlap = (verticalOverlap > paddedMetrics.BottomPadding) ? verticalOverlap : paddedMetrics.BottomPadding;
                paddedMetrics.Move(0F, -verticalOverlap);
            }
        }

        private void MoveFramedTextAboveBeamBlock(Metrics framedTextMetrics, BeamBlock beamBlock)
        {
            float padding = Gap * 1.5F;

            float verticalOverlap = beamBlock.OverlapHeight(framedTextMetrics, padding);
            if(verticalOverlap > 0)
            {
                verticalOverlap = (verticalOverlap > padding) ? verticalOverlap : padding;
                framedTextMetrics.Move(0F, -verticalOverlap);
            }
        }

        private void MoveFramedTextAboveNoteheadExtenders(Metrics framedTextMetrics, List<NoteheadExtenderMetrics> noteheadExtendersMetrics)
        {
            float padding = Gap * 1.5F;
            int indexOfTopExtender = 0;
            for(int i = 1; i < noteheadExtendersMetrics.Count; ++i)
            {
                indexOfTopExtender = (noteheadExtendersMetrics[indexOfTopExtender].Top < noteheadExtendersMetrics[i].Top) ? indexOfTopExtender : i;
            }
            NoteheadExtenderMetrics topExtender = noteheadExtendersMetrics[indexOfTopExtender];
            float verticalOverlap = topExtender.OverlapHeight(framedTextMetrics, padding);
            if(verticalOverlap > 0)
            {
                verticalOverlap = (verticalOverlap > padding) ? verticalOverlap : padding;
                framedTextMetrics.Move(0F, -(verticalOverlap));
            }
        }

        /// <summary>
        /// This virtual function writes the staff name and barnumber to the SVG file (if they are present).
        /// Overrides write the region info (if present).
        /// The barline itself is drawn when the system (and staff edges) is complete.
        /// </summary>
        public virtual void WriteDrawObjectsSVG(SvgWriter w)
        {
            if(StaffNameMetrics != null)
            {
                StaffNameMetrics.WriteSVG(w);
            }
            if(BarnumberMetrics != null)
            {
                BarnumberMetrics.WriteSVG(w);
            }
        }

        protected void SetDrawObjects(List<DrawObject> drawObjects)
        {
            DrawObjects.Clear();
            foreach(DrawObject drawObject in drawObjects)
            {
                drawObject.Container = this;
                DrawObjects.Add(drawObject);
            }
        }

        internal virtual void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
        {
            if(StaffNameMetrics != null)
            {
                staffMetrics.Add(StaffNameMetrics);
            }
            if(BarnumberMetrics != null)
            {
                staffMetrics.Add(BarnumberMetrics);
            }
        }

        internal abstract void AlignFramedTextsXY(List<NoteObject> noteObjects0);

        protected void AlignBarnumberX()
        {
            if(BarnumberMetrics != null)
            {
                BarnumberMetrics.Move(Barline_LineMetrics.OriginX - BarnumberMetrics.OriginX, 0);
            }
        }

        public Metrics Barline_LineMetrics = null;
        public StaffNameMetrics StaffNameMetrics = null;
        public BarnumberMetrics BarnumberMetrics = null;

        /// <summary>
        /// Default is true
        /// </summary>
        public bool IsVisible = true;

        private PageFormat PageFormat { get { return Voice.Staff.SVGSystem.Score.PageFormat; } }
        protected float StafflineStrokeWidth { get { return PageFormat.StafflineStemStrokeWidth; } }
        protected float ThinStrokeWidth { get { return PageFormat.ThinBarlineStrokeWidth; } }
        protected float NormalStrokeWidth { get { return PageFormat.NormalBarlineStrokeWidth; } }
        protected float ThickStrokeWidth { get { return PageFormat.ThickBarlineStrokeWidth; } }
        protected float DoubleBarPadding { get { return PageFormat.ThickBarlineStrokeWidth * 0.75F; } }
        protected float Gap { get { return PageFormat.Gap; } }
        protected float TopY(float topStafflineY, bool isEndOfSystem)
        {
            float topY = topStafflineY;
            if(isEndOfSystem)
            {
                float halfStafflineWidth = (StafflineStrokeWidth / 2);
                topY -= halfStafflineWidth;
            }
            return topY;
        }
        protected float BottomY(float bottomStafflineY, bool isEndOfSystem)
        {
            float bottomY = bottomStafflineY;
            if(isEndOfSystem)
            {
                float halfStafflineWidth = (StafflineStrokeWidth / 2);
                bottomY += halfStafflineWidth;
            }
            return bottomY;
        }
    }

    /// <summary>
    /// A barline which is a single, thin line. OriginX is the line's x-coordinate.
    /// </summary>
    public class NormalBarline : Barline
    {
        public NormalBarline(Voice voice)
            : base(voice)
        {
        }

        /// <summary>
        /// Writes out the barline's vertical line(s).
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
        {
            float topY = TopY(topStafflineY, isEndOfSystem);
            float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

            w.SvgLine(CSSObjectClass.normalBarline, this.Barline_LineMetrics.OriginX, topY, this.Barline_LineMetrics.OriginX, bottomY);
        }

        public override string ToString()
        {
            return "normalBarline: ";
        }

        public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
        {
            AddBasicMetricsToEdge(horizontalEdge);
        }

        internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
        {
            #region alignX
            base.AlignBarnumberX();
            #endregion
            MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);
        }

        public override void CreateMetrics(Graphics graphics)
        {
            Barline_LineMetrics = new Barline_LineMetrics(-(NormalStrokeWidth / 2F), (NormalStrokeWidth / 2F));
            SetCommonMetrics(graphics, DrawObjects);
        }

        internal override void AddAncilliaryMetricsTo(StaffMetrics metrics)
        {
            base.AddAncilliaryMetricsTo(metrics);
        }
    }

    /// <summary>
    /// A barline whose 2 lines are (left to right) thick then thin. OriginX is the thick line's x-coordinate.
    /// </summary>
    public class StartRegionBarline : Barline
    {
        public StartRegionBarline(Voice voice, List<DrawObject> drawObjects)
            : base(voice)
        {
            SetDrawObjects(drawObjects);
        }

        /// <summary>
        /// Writes out the barline's vertical line(s).
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
        {
            float topY = TopY(topStafflineY, isEndOfSystem);
            float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

            float thickLeftLineOriginX = Barline_LineMetrics.OriginX;
            w.SvgStartGroup(CSSObjectClass.startRegionBarline.ToString());
            w.SvgLine(CSSObjectClass.thickBarline, thickLeftLineOriginX, topY, thickLeftLineOriginX, bottomY);

            float thinRightLineOriginX = thickLeftLineOriginX + (ThickStrokeWidth / 2F) + DoubleBarPadding + (ThinStrokeWidth / 2F);
            w.SvgLine(CSSObjectClass.thinBarline, thinRightLineOriginX, topY, thinRightLineOriginX, bottomY);
            w.SvgEndGroup();
        }
        /// <summary>
        /// This function writes the staff name, barnumber and region info to the SVG file (if they are present).
        /// The barline itself is drawn when the system (and staff edges) is complete.
        /// </summary>
        public override void WriteDrawObjectsSVG(SvgWriter w)
        {
            base.WriteDrawObjectsSVG(w);

            if(FramedRegionStartTextMetrics != null)
            {
                FramedRegionStartTextMetrics.WriteSVG(w);
            }
        }

        public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
        {
            if(FramedRegionStartTextMetrics != null)
            {
                horizontalEdge.Add(FramedRegionStartTextMetrics);
            }

            AddBasicMetricsToEdge(horizontalEdge);
        }

        public override string ToString()
        {
            return "startRegionBarline: ";
        }

        internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
        {
            #region alignX
            base.AlignBarnumberX();
            float originX = Barline_LineMetrics.OriginX;
            if(FramedRegionStartTextMetrics != null)
            {
                FramedRegionStartTextMetrics.Move(originX - FramedRegionStartTextMetrics.Left, 0);
            }
            #endregion

            MoveFramedTextBottomToDefaultPosition(FramedRegionStartTextMetrics);

            MoveFramedTextAboveNoteObjects(FramedRegionStartTextMetrics, fixedNoteObjects);
            MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);

            MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionStartTextMetrics);
        }

        public override void CreateMetrics(Graphics graphics)
        {
            float leftEdge = -(ThickStrokeWidth / 2F);
            float rightEdge = (ThickStrokeWidth / 2F) + DoubleBarPadding + ThinStrokeWidth;
            Barline_LineMetrics = new Barline_LineMetrics(leftEdge, rightEdge, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

            SetCommonMetrics(graphics, DrawObjects);

            foreach(DrawObject drawObject in DrawObjects)
            {
                if(drawObject is FramedRegionStartText frst)
                {
                    FramedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo);
                    break;
                }
            }
        }

        internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
        {
            base.AddAncilliaryMetricsTo(staffMetrics);

            if(FramedRegionStartTextMetrics != null)
            {
                staffMetrics.Add(FramedRegionStartTextMetrics);
            }
        }

        public FramedRegionInfoMetrics FramedRegionStartTextMetrics = null;
    }

    /// <summary>
    /// A barline whose 2 lines are (left to right) thin then thick. OriginX is the thick line's x-coordinate.
    /// </summary>
    public class EndRegionBarline : Barline
    {
        public EndRegionBarline(Voice voice, List<DrawObject> drawObjects)
            : base(voice)
        {
            SetDrawObjects(drawObjects);
        }

        /// <summary>
        /// Writes out the barline's vertical line(s).
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
        {
            float topY = TopY(topStafflineY, isEndOfSystem);
            float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

            float thinLeftLineOriginX = Barline_LineMetrics.OriginX - (ThickStrokeWidth / 2) - DoubleBarPadding - (ThinStrokeWidth / 2F);
            w.SvgStartGroup(CSSObjectClass.endRegionBarline.ToString());
            w.SvgLine(CSSObjectClass.thinBarline, thinLeftLineOriginX, topY, thinLeftLineOriginX, bottomY);

            float thickRightLineOriginX = Barline_LineMetrics.OriginX;
            w.SvgLine(CSSObjectClass.thickBarline, thickRightLineOriginX, topY, thickRightLineOriginX, bottomY);
            w.SvgEndGroup();
        }
        /// <summary>
        /// This function writes the staff name, barnumber and region info to the SVG file (if they are present).
        /// The barline itself is drawn when the system (and staff edges) is complete.
        /// </summary>
        public override void WriteDrawObjectsSVG(SvgWriter w)
        {
            base.WriteDrawObjectsSVG(w);

            if(FramedRegionEndTextMetrics != null)
            {
                FramedRegionEndTextMetrics.WriteSVG(w);
            }
        }

        public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
        {
            if(FramedRegionEndTextMetrics != null)
            {
                horizontalEdge.Add(FramedRegionEndTextMetrics);
            }

            AddBasicMetricsToEdge(horizontalEdge);
        }

        internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
        {
            #region alignX
            base.AlignBarnumberX();
            float originX = Barline_LineMetrics.OriginX;
            if(FramedRegionEndTextMetrics != null)
            {
                FramedRegionEndTextMetrics.Move(originX - FramedRegionEndTextMetrics.Right, 0);
            }
            #endregion

            MoveFramedTextBottomToDefaultPosition(FramedRegionEndTextMetrics);

            MoveFramedTextAboveNoteObjects(FramedRegionEndTextMetrics, fixedNoteObjects);
            MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);

            MoveBarnumberAboveRegionBox(BarnumberMetrics, FramedRegionEndTextMetrics);
        }

        internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
        {
            base.AddAncilliaryMetricsTo(staffMetrics);
            if(FramedRegionEndTextMetrics != null)
            {
                staffMetrics.Add(FramedRegionEndTextMetrics);
            }
        }

        public override void CreateMetrics(Graphics graphics)
        {
            float leftEdge = -((ThickStrokeWidth / 2F) + DoubleBarPadding + ThinStrokeWidth);
            float rightEdge = (ThickStrokeWidth / 2F);
            Barline_LineMetrics = new Barline_LineMetrics(leftEdge, rightEdge, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

            SetCommonMetrics(graphics, DrawObjects);

            foreach(DrawObject drawObject in DrawObjects)
            {
                if(drawObject is FramedRegionEndText frst)
                {
                    FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo);
                    break;
                }
            }
        }

        public override string ToString()
        {
            return "endRegionBarline: ";
        }

        public FramedRegionInfoMetrics FramedRegionEndTextMetrics = null;
    }

    /// <summary>_
    /// A barline whose 3 lines are (left to right) thin, thick, thin. OriginX is the thick line's x-coordinate.
    /// </summary>
    public class EndAndStartRegionBarline : Barline
    {
        public EndAndStartRegionBarline(Voice voice, List<DrawObject> drawObjects)
            : base(voice)
        {
            SetDrawObjects(drawObjects);
        }

        /// <summary>
        /// Writes out the barline's vertical line(s).
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
        {
            float topY = TopY(topStafflineY, isEndOfSystem);
            float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

            w.SvgStartGroup(CSSObjectClass.endAndStartRegionBarline.ToString());

            float thinLeftLineOriginX = Barline_LineMetrics.OriginX - (ThickStrokeWidth / 2F) - DoubleBarPadding - (ThinStrokeWidth / 2F);
            w.SvgLine(CSSObjectClass.thinBarline, thinLeftLineOriginX, topY, thinLeftLineOriginX, bottomY);

            float thickCentreLineOriginX = Barline_LineMetrics.OriginX;
            w.SvgLine(CSSObjectClass.thickBarline, thickCentreLineOriginX, topY, thickCentreLineOriginX, bottomY);

            float thinRightLineOriginX = thickCentreLineOriginX + (ThickStrokeWidth / 2F) + DoubleBarPadding + (ThinStrokeWidth / 2F);
            w.SvgLine(CSSObjectClass.thinBarline, thinRightLineOriginX, topY, thinRightLineOriginX, bottomY);

            w.SvgEndGroup();
        }
        /// <summary>
        /// This function writes the staff name, barnumber and region info to the SVG file (if they are present).
        /// The barline itself is drawn when the system (and staff edges) is complete.
        /// </summary>
        public override void WriteDrawObjectsSVG(SvgWriter w)
        {
            base.WriteDrawObjectsSVG(w);

            if(FramedRegionEndTextMetrics != null)
            {
                FramedRegionEndTextMetrics.WriteSVG(w);
            }

            if(FramedRegionStartTextMetrics != null)
            {
                FramedRegionStartTextMetrics.WriteSVG(w);
            }
        }

        public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
        {
            if(FramedRegionEndTextMetrics != null)
            {
                horizontalEdge.Add(FramedRegionEndTextMetrics);
            }

            if(FramedRegionStartTextMetrics != null)
            {
                horizontalEdge.Add(FramedRegionStartTextMetrics);
            }

            AddBasicMetricsToEdge(horizontalEdge);
        }

        public override string ToString()
        {
            return "endAndStartRegionBarline: ";
        }

        internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
        {
            #region alignX

            // An EndAndStartRegionBarline cannot be at the start of a system,
            // so it can't have a barnumber, and there's no reason to call base.AlignBarnumberX();
            M.Assert(BarnumberMetrics == null);

            float originX = Barline_LineMetrics.OriginX;
            if(FramedRegionEndTextMetrics != null)
            {
                FramedRegionEndTextMetrics.Move(originX - FramedRegionEndTextMetrics.Right, 0);
            }
            if(FramedRegionStartTextMetrics != null)
            {
                FramedRegionStartTextMetrics.Move(originX - FramedRegionStartTextMetrics.Left, 0);
            }
            #endregion

            MoveFramedTextBottomToDefaultPosition(FramedRegionStartTextMetrics);
            MoveFramedTextBottomToDefaultPosition(FramedRegionEndTextMetrics);

            MoveFramedTextAboveNoteObjects(FramedRegionStartTextMetrics, fixedNoteObjects);
            MoveFramedTextAboveNoteObjects(FramedRegionEndTextMetrics, fixedNoteObjects);
            MoveFramedTextAboveNoteObjects(BarnumberMetrics, fixedNoteObjects);
        }

        public override void CreateMetrics(Graphics graphics)
        {
            float rightEdge = (ThickStrokeWidth / 2F) + DoubleBarPadding + ThinStrokeWidth;
            float leftEdge = -rightEdge;
            Barline_LineMetrics = new Barline_LineMetrics(leftEdge, rightEdge, CSSObjectClass.thinBarline, CSSObjectClass.thickBarline);

            SetCommonMetrics(graphics, DrawObjects);

            foreach(DrawObject drawObject in DrawObjects)
            {
                if(drawObject is FramedRegionStartText frst)
                {
                    FramedRegionStartTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo);
                }
                if(drawObject is FramedRegionEndText fret)
                {
                    FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, fret.Texts, fret.FrameInfo);
                }
            }
        }

        internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)
        {
            base.AddAncilliaryMetricsTo(staffMetrics);

            if(FramedRegionStartTextMetrics != null)
            {
                staffMetrics.Add(FramedRegionStartTextMetrics);
            }
            if(FramedRegionEndTextMetrics != null)
            {
                staffMetrics.Add(FramedRegionEndTextMetrics);
            }
        }

        public FramedRegionInfoMetrics FramedRegionStartTextMetrics = null;
        public FramedRegionInfoMetrics FramedRegionEndTextMetrics = null;
    }

    /// <summary>
    /// A barline whose 2 lines are (left to right) normal then thick. OriginX is the thick line's x-coordinate.
    /// This barline type is always used for the final barline in a score. It can have FramedEndRegionInfo.
    /// </summary>
    public class EndOfScoreBarline : EndRegionBarline
    {
        public EndOfScoreBarline(Voice voice, List<DrawObject> drawObjects)
            : base(voice, drawObjects)
        {
        }

        /// <summary>
        /// Writes out the barline's vertical line(s).
        /// May be called twice per staff.barline:
        ///     1. for the range between top and bottom stafflines (if Barline.Visible is true)
        ///     2. for the range between the staff's lower edge and the next staff's upper edge
        ///        (if the staff's lower neighbour is in the same group)
        /// </summary>
        /// <param name="w"></param>
        public override void WriteSVG(SvgWriter w, float topStafflineY, float bottomStafflineY, bool isEndOfSystem)
        {
            float topY = TopY(topStafflineY, isEndOfSystem);
            float bottomY = BottomY(bottomStafflineY, isEndOfSystem);

            float normalLeftLineOriginX = Barline_LineMetrics.OriginX - (ThickStrokeWidth / 2) - DoubleBarPadding - (NormalStrokeWidth / 2F);
            w.SvgStartGroup(CSSObjectClass.endOfScoreBarline.ToString());
            w.SvgLine(CSSObjectClass.normalBarline, normalLeftLineOriginX, topY, normalLeftLineOriginX, bottomY);

            float thickRightLineOriginX = Barline_LineMetrics.OriginX;
            w.SvgLine(CSSObjectClass.thickBarline, thickRightLineOriginX, topY, thickRightLineOriginX, bottomY);
            w.SvgEndGroup();
        }

        // The following functions are inherited from EndRegionBarline.
        // public override void WriteDrawObjectsSVG(SvgWriter w)
        // public override void AddMetricsToEdge(HorizontalEdge horizontalEdge)
        // internal override void AlignFramedTextsXY(List<NoteObject> fixedNoteObjects)
        // internal override void AddAncilliaryMetricsTo(StaffMetrics staffMetrics)

        public override void CreateMetrics(Graphics graphics)
        {
            float leftEdge = -((ThickStrokeWidth / 2F) + DoubleBarPadding + NormalStrokeWidth);
            float rightEdge = (ThickStrokeWidth / 2F);
            Barline_LineMetrics = new Barline_LineMetrics(leftEdge, rightEdge, CSSObjectClass.normalBarline, CSSObjectClass.thickBarline);

            foreach(DrawObject drawObject in DrawObjects)
            {
                if(drawObject is FramedRegionEndText frst)
                {
                    FramedRegionEndTextMetrics = new FramedRegionInfoMetrics(graphics, frst.Texts, frst.FrameInfo);
                    break;
                }
            }
        }

        public override string ToString() { return "endOfScoreBarline: "; }
    }

}
