using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Score.Notation;

namespace Moritz.Score
{
	public class BeamBlock : Metrics
	{
        public BeamBlock(ClefSymbol clef, List<OutputChordSymbol> chordsBeamedTogether, VerticalDir voiceStemDirection, float beamThickness, float strokeThickness)
            : base(null, 0, 0)
        {
            Chords = new List<OutputChordSymbol>(chordsBeamedTogether);
            SetBeamedGroupStemDirection(clef, chordsBeamedTogether, voiceStemDirection);
            foreach(OutputChordSymbol chord in chordsBeamedTogether)
                chord.BeamBlock = this; // prevents an isolated flag from being created

            _gap = Chords[0].Voice.Staff.Gap;
            _beamSeparation = _gap;
            _beamThickness = beamThickness;
            _strokeThickness = strokeThickness;
            _stemDirection = Chords[0].Stem.Direction;
            
            /******************************************************************************
             * Important to set stem tips to this value before justifying horizontally.
             * Allows collisions between the objects outside the tips (e.g. dynamics or ornaments)
             * to be detected correctly. */
            _defaultStemTipY = GetDefaultStemTipY(clef, chordsBeamedTogether);
        }

        /// <summary>
        /// This algorithm follows Gardner Read when the stemDirection is "none" (i.e. not forced):
        /// If there were no beam, and the majority of the stems would go up, then all the stems in the beam go up.
        /// ji: if there are the same number of default up and default down stems, then the direction is decided by
        /// the most extreme notehead in the beam group. If both extremes are the same (e.g. 1 ledgeline up and down)
        /// then the stems are all down.
        /// </summary>
        /// <param name="currentClef"></param>
        /// <param name="chordsBeamedTogether"></param>
        private void SetBeamedGroupStemDirection(ClefSymbol currentClef, List<OutputChordSymbol> chordsBeamedTogether, VerticalDir voiceStemDirection)
        {
            Debug.Assert(chordsBeamedTogether.Count > 1);
            VerticalDir groupStemDirection = voiceStemDirection;
            if(voiceStemDirection == VerticalDir.none)
            {   // here, there is only one voice in the staff, so the direction depends on the height of the noteheads.
                int upStems = 0;
                int downStems = 0;
                foreach(OutputChordSymbol chord in chordsBeamedTogether)
                {
                    VerticalDir direction = chord.DefaultStemDirection(currentClef);
                    if(direction == VerticalDir.up)
                        upStems++;
                    else
                        downStems++;
                }

                if(upStems == downStems)
                    groupStemDirection = GetDirectionFromExtremes(currentClef, chordsBeamedTogether);
                else if(upStems > downStems)
                    groupStemDirection = VerticalDir.up;
                else
                    groupStemDirection = VerticalDir.down;
            }
            foreach(OutputChordSymbol chord in chordsBeamedTogether)
            {
                chord.Stem.Direction = groupStemDirection;
            }
        }

        private VerticalDir GetDirectionFromExtremes(ClefSymbol currentClef, List<OutputChordSymbol> chordsBeamedTogether)
        {
            float headMinTop = float.MaxValue;
            float headMaxBottom = float.MinValue;
            float gap = chordsBeamedTogether[0].Voice.Staff.Gap;
            int numberOfStafflines = chordsBeamedTogether[0].Voice.Staff.NumberOfStafflines;

            foreach(OutputChordSymbol chord in chordsBeamedTogether)
            {
                foreach(Head head in chord.HeadsTopDown)
                {
                    float headY = head.GetOriginY(currentClef, gap);
                    headMinTop = headMinTop < headY ? headMinTop : headY;
                    headMaxBottom = headMaxBottom > headY ? headMaxBottom : headY;
                }
            }
            headMaxBottom -= (gap * (numberOfStafflines - 1));
            headMinTop *= -1;
            if(headMaxBottom > headMinTop)
                return VerticalDir.up;
            else
                return VerticalDir.down;
        }

        private float GetDefaultStemTipY(ClefSymbol currentClef, List<OutputChordSymbol> chordsBeamedTogether)
        {
            float headMinTop = float.MaxValue;
            float headMaxBottom = float.MinValue;
            float gap = chordsBeamedTogether[0].Voice.Staff.Gap;
            int numberOfStafflines = chordsBeamedTogether[0].Voice.Staff.NumberOfStafflines;
            VerticalDir direction = chordsBeamedTogether[0].Stem.Direction;

            foreach(OutputChordSymbol chord in chordsBeamedTogether)
            {
                foreach(Head head in chord.HeadsTopDown)
                {
                    float headY = head.GetOriginY(currentClef, gap);
                    headMinTop = headMinTop < headY ? headMinTop : headY;
                    headMaxBottom = headMaxBottom > headY ? headMaxBottom : headY;
                }
            }

            if(direction == VerticalDir.up)
                return headMinTop - (gap * numberOfStafflines);
            else
                return headMaxBottom + (gap * numberOfStafflines);
        }

        /// <summary>
        /// This beam is attached to chords in one voice of a 2-voice staff.
        /// The returned chords are the chords in the other voice whose msPosition
        /// is greater than or equal to the msPosition at the start of this beamBlock,
        /// and less than or equal to the msPosition at the end of this beamBlock. 
        /// </summary>
        public List<OutputChordSymbol> EnclosedChords(Voice otherVoice)
        {
            Debug.Assert(Chords.Count > 1);
            int startMsPos = Chords[0].MsPosition;
            int endMsPos = Chords[Chords.Count-1].MsPosition;
            List<OutputChordSymbol> enclosedChordSymbols = new List<OutputChordSymbol>();
            foreach(OutputChordSymbol otherChord in otherVoice.ChordSymbols)
            {
                if(otherChord.MsPosition >= startMsPos && otherChord.MsPosition <= endMsPos)
                    enclosedChordSymbols.Add(otherChord);
                if(otherChord.MsPosition > endMsPos)
                    break;
            }
            return enclosedChordSymbols;
        }
        /// <summary>
        /// The system has been justified horizontally, so all objects are at their final horizontal positions.
        /// This function
        ///  1. creates all the contained beams horizontally with their top edges at 0F.
        ///  2. expands the beams vertically, and moves them to the closest position on the correct side of their noteheads.
        ///  3. shears the beams.
        ///  4. moves the stem tips and related objects (dynamics, ornament numbers) to their correct positions wrt the outer beam.
        /// Especially note that neither this beam Block or its contained beams ever move _horizontally_.
        /// </summary>
        public void FinalizeBeamBlock()
        {
            #region create the individual beams all with top edge horizontal at 0F.
            HashSet<DurationClass> durationClasses = new HashSet<DurationClass>()
            {
                DurationClass.fiveFlags,
                DurationClass.fourFlags,
                DurationClass.threeFlags,
                DurationClass.semiquaver,
                DurationClass.quaver
            };
            float stubWidth =  _gap * 1.2F;
            foreach(DurationClass durationClass in durationClasses)
            {
                List<Beam> beams = CreateBeams(durationClass, stubWidth);
                _left = float.MaxValue;
                _right = float.MinValue;
                foreach(Beam beam in beams)
                {
                    _left = _left < beam.LeftX ? _left : beam.LeftX;
                    _right = _right > beam.RightX ? _right : beam.RightX;
                    Beams.Add(beam);
                }
                _originX = _left;
                // _left, _right and _originX never change again after they have been set here
            }
            SetVerticalBounds();
            #endregion
            Dictionary<DurationClass, float> durationClassBeamThickness = GetBeamThicknessesPerDurationClass(durationClasses);
            List<ChordMetrics> chordsMetrics = GetChordsMetrics();
            ExpandVerticallyAtNoteheads(chordsMetrics, durationClassBeamThickness);
            Shear(chordsMetrics);
            SetToStaff(durationClassBeamThickness);
            MoveStemTips();
        }

        List<ChordMetrics> GetChordsMetrics()
        {
            List<ChordMetrics> chordsMetrics = new List<ChordMetrics>();
            foreach(OutputChordSymbol chord in Chords)
            {
                chordsMetrics.Add((ChordMetrics)chord.Metrics);
            }
            return chordsMetrics;
        }

        private void SetVerticalBounds()
        {
            _top = float.MaxValue;
            _bottom = float.MinValue;
            foreach(Beam beam in Beams)
            {
                float beamBoundsTop = beam.LeftTopY < beam.RightTopY ? beam.LeftTopY : beam.RightTopY;
                float beamBoundsBottom = beam.LeftTopY > beam.RightTopY ? beam.LeftTopY : beam.RightTopY;
                beamBoundsBottom += _beamThickness;
                _bottom = _bottom > beamBoundsBottom ? _bottom : beamBoundsBottom;
                _top = _top < beamBoundsTop ? _top : beamBoundsTop;
            }
            _originY = _top;
        }

        private List<Beam> CreateBeams(DurationClass durationClass, float stubWidth)
        {
            List<Beam> newBeams = new List<Beam>();
            bool inBeam = false;
            float beamLeft = -1F;
            float beamRight = -1F;

            ChordMetrics rightMostChordMetrics = (ChordMetrics)Chords[Chords.Count - 1].Metrics;
            float rightMostStemX = rightMostChordMetrics.StemMetrics.OriginX;

            int stemNumber = 1;
            foreach(OutputChordSymbol chord in Chords)
            {
                ChordMetrics chordMetrics = (ChordMetrics)chord.Metrics;
                float stemX = chordMetrics.StemMetrics.OriginX;

                bool hasLessThanOrEqualBeams = HasLessThanOrEqualBeams(durationClass, chord.DurationClass);
                if(!inBeam && hasLessThanOrEqualBeams)
                {
                    beamLeft = stemX;
                    beamRight = stemX;
                    inBeam = true;
                }
                else if(inBeam && hasLessThanOrEqualBeams)
                {
                    beamRight = stemX;
                }

                if(inBeam && ((!hasLessThanOrEqualBeams) || stemX == rightMostStemX)) // different durationClass or end of beamBlock
                {
                    bool isStub = false;
                    if(beamLeft == beamRight)
                    {
                        // Create a beamStub. All beam stubs are initially to the left of the stem to which they are attached.
                        // Beam stubs attached to the first stem in a beamed group are replaced later in this function.
                        beamLeft -= stubWidth;
                        isStub = true;
                    }

                    Beam newBeam = NewBeam(durationClass, beamLeft, beamRight, isStub);
                    newBeams.Add(newBeam);
                    inBeam = false;
                }
                stemNumber++;
            }

            ChordMetrics leftMostChordMetrics = (ChordMetrics)Chords[0].Metrics;
            float leftMostStemX = leftMostChordMetrics.StemMetrics.OriginX;

            if(newBeams.Count > 0 && newBeams[0].LeftX < leftMostStemX && newBeams[0].RightX == leftMostStemX)
            {
                // Replace a beamStub on the left of the first stem by a beam stub on the right.
                Beam newBeam = NewBeam(durationClass, leftMostStemX, leftMostStemX + stubWidth, true);
                newBeams.RemoveAt(0);
                newBeams.Insert(0, newBeam);
            }

            return newBeams;
        }

        /// <summary>
        /// returns true if the currentDC has a less than or equal number of beams than the stemDC
        /// </summary>
        /// <param name="currentDC"></param>
        /// <param name="stemDC"></param>
        /// <returns></returns>
        private bool HasLessThanOrEqualBeams(DurationClass currentDC, DurationClass stemDC)
        {
            bool hasLessThanOrEqualBeams = false;
            switch(currentDC)
            {
                case DurationClass.fiveFlags:
                    if(stemDC == DurationClass.fiveFlags)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.fourFlags:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.threeFlags:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags
                    || stemDC == DurationClass.threeFlags)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.semiquaver:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags
                    || stemDC == DurationClass.threeFlags
                    || stemDC == DurationClass.semiquaver)
                        hasLessThanOrEqualBeams = true;
                    break;
                case DurationClass.quaver:
                    if(stemDC == DurationClass.fiveFlags
                    || stemDC == DurationClass.fourFlags
                    || stemDC == DurationClass.threeFlags
                    || stemDC == DurationClass.semiquaver
                    || stemDC == DurationClass.quaver)
                        hasLessThanOrEqualBeams = true;
                    break;
            }
            return hasLessThanOrEqualBeams;
        }
        private Beam NewBeam(DurationClass durationClass, float leftX, float rightX, bool isStub)
        {
            Beam newBeam = null;
            switch(durationClass)
            {
                case DurationClass.fiveFlags:
                    if(isStub)
                        newBeam = new FiveFlagsBeamStub(leftX, rightX);
                    else
                        newBeam = new FiveFlagsBeam(leftX, rightX);
                    break;
                case DurationClass.fourFlags:
                    if(isStub)
                        newBeam = new FourFlagsBeamStub(leftX, rightX);
                    else
                        newBeam = new FourFlagsBeam(leftX, rightX);
                    break;
                case DurationClass.threeFlags:
                    if(isStub)
                        newBeam = new ThreeFlagsBeamStub(leftX, rightX);
                    else
                        newBeam = new ThreeFlagsBeam(leftX, rightX);
                    break;
                case DurationClass.semiquaver:
                    if(isStub)
                        newBeam = new SemiquaverBeamStub(leftX, rightX);
                    else
                        newBeam = new SemiquaverBeam(leftX, rightX);
                    break;
                case DurationClass.quaver:
                    newBeam = new QuaverBeam(leftX, rightX);
                    break;
                default:
                    Debug.Assert(false, "Illegal beam duration class");
                    break;
            }
            return newBeam;
        }

        public void Move(float dy)
        {
            foreach(Beam beam in Beams)
            {
                beam.MoveYs(dy, dy);
            }
            SetVerticalBounds();
        }
        /// <summary>
        /// Moves the horizontal beams to their correct vertical positions re the chords
        /// leaving the outer leftY of the beamBlock at leftY
        /// </summary>
        /// <param name="outerLeftY"></param>
        private void ExpandVerticallyAtNoteheads(List<ChordMetrics> chordsMetrics, Dictionary<DurationClass, float> durationClassBeamThickness)
        {
            ExpandVerticallyOnStaff();
            MoveToNoteheads(chordsMetrics, durationClassBeamThickness);
            SetVerticalBounds();
        }

        /// <summary>
        /// Expands the beamBlock vertically, leaving its outer edge on the top line of the staff 
        /// </summary>
        private void ExpandVerticallyOnStaff()
        {
            float staffOriginY = Chords[0].Voice.Staff.Metrics.OriginY;
            foreach(Beam beam in Beams)
            {
                beam.ShiftYsForBeamBlock(staffOriginY, _gap, _stemDirection, _beamThickness);
            }
        }
        /// <summary>
        /// Moves this horizontal beamBlock vertically until it is on the right side (above or below)
        /// the noteheads, and as close as possible to the noteheads.
        /// If there is only a single (quaver) beam, it ends up with its inner edge one octave from
        /// the OriginY of the closest notehead in the group.
        /// Otherwise the smallest distance between any beam and the closest notehead will be a sixth.
        /// </summary>
        /// <returns></returns>
        private void MoveToNoteheads(List<ChordMetrics> chordsMetrics, Dictionary<DurationClass, float> durationClassBeamThickness)
        {
            float staffOriginY = Chords[0].Voice.Staff.Metrics.OriginY;
            if(_stemDirection == VerticalDir.down)
            {
                float lowestBottom = float.MinValue;
                for(int i = 0; i < Chords.Count; ++i)
                {
                    ChordMetrics chordMetrics = chordsMetrics[i];
                    HeadMetrics bottomHeadMetrics = chordMetrics.BottomHeadMetrics;
                    float beamBottom =
                        bottomHeadMetrics.OriginY + durationClassBeamThickness[Chords[i].DurationClass];
                    if(((bottomHeadMetrics.OriginY - staffOriginY) % _gap) != 0)
                        beamBottom +=(_gap * 2.5F);
                    else
                        beamBottom += (_gap * 2.65F);

                    lowestBottom = lowestBottom > beamBottom ? lowestBottom : beamBottom;
                }
                if(Beams.Count == 1) // only a quaver beam
                    lowestBottom += _gap;

                Move(lowestBottom - staffOriginY);
            }
            else // stems up
            {
                float highestTop = float.MaxValue;
                for(int i = 0; i < Chords.Count; ++i)
                {
                    ChordMetrics chordMetrics = chordsMetrics[i];
                    HeadMetrics topHeadMetrics = chordMetrics.TopHeadMetrics;
                    float beamTop =
                        topHeadMetrics.OriginY - durationClassBeamThickness[Chords[i].DurationClass];
                    if(((topHeadMetrics.OriginY - staffOriginY) % _gap) != 0)
                        beamTop -= (_gap * 2.5F);
                    else
                        beamTop -= (_gap * 2.65F);

                    highestTop = highestTop < beamTop ? highestTop : beamTop;
                }
                if(Beams.Count == 1) // only a quaver beam
                    highestTop -= _gap;

                Move(highestTop - staffOriginY);
            }
        }
        private Dictionary<DurationClass, float> GetBeamThicknessesPerDurationClass(HashSet<DurationClass> durationClasses)
        {
            Dictionary<DurationClass, float> btpdc = new Dictionary<DurationClass, float>();
            float thickness = 0F;
            foreach(DurationClass dc in durationClasses)
            {
                switch(dc)
                {
                    case DurationClass.fiveFlags:
                        thickness = (4 * _beamSeparation) + _beamThickness;
                        btpdc.Add(DurationClass.fiveFlags, thickness);
                        break;
                    case DurationClass.fourFlags:
                        thickness = (3 * _beamSeparation) + _beamThickness;
                        btpdc.Add(DurationClass.fourFlags, thickness);
                        break;
                    case DurationClass.threeFlags:
                        thickness = (2 * _beamSeparation) + _beamThickness;
                        btpdc.Add(DurationClass.threeFlags, thickness);
                        break;
                    case DurationClass.semiquaver:
                        thickness = _beamSeparation + _beamThickness;
                        btpdc.Add(DurationClass.semiquaver, thickness);
                        break;
                    case DurationClass.quaver:
                        thickness = _beamThickness;
                        btpdc.Add(DurationClass.quaver, thickness);
                        break;
                }
            }
            return btpdc;           
        }

        private void Shear(List<ChordMetrics> chordsMetrics)
        {
            float tanAlpha = ShearAngle(chordsMetrics);
            float shearAxis = ShearAxis(chordsMetrics, tanAlpha);
            if(Beams.Count == 1 && (tanAlpha > 0.02 || tanAlpha < -0.02))
            {
                if(_stemDirection == VerticalDir.up)
                    Move(_gap * 0.75F);
                else
                    Move(-_gap * 0.75F);
            }
            foreach(ChordMetrics chordMetrics in chordsMetrics)
            {
                float width = chordMetrics.StemMetrics.OriginX - shearAxis;
                float stemX = chordMetrics.StemMetrics.OriginX; 
                float dy = width * tanAlpha;
                foreach(Beam beam in Beams)
                {
                    IBeamStub beamStub = beam as IBeamStub;
                    if(beamStub != null)
                    {
                        beamStub.ShearBeamStub(shearAxis, tanAlpha, stemX);
                    }
                    else
                    {
                        if(beam.LeftX == stemX)
                            beam.MoveYs(dy, 0F);
                        else if(beam.RightX == stemX)
                            beam.MoveYs(0F, dy);
                    }
                }
            }
            SetVerticalBounds();
        }

        private float ShearAngle(List<ChordMetrics> chordsMetrics)
        {
            ChordMetrics leftChordMetrics = chordsMetrics[0];
            ChordMetrics rightChordMetrics = chordsMetrics[chordsMetrics.Count -1]; 
            float height =
                    (((rightChordMetrics.TopHeadMetrics.OriginY + rightChordMetrics.BottomHeadMetrics.OriginY) / 2)
                   - ((leftChordMetrics.TopHeadMetrics.OriginY + leftChordMetrics.BottomHeadMetrics.OriginY) / 2));

            float width = rightChordMetrics.StemMetrics.OriginX - leftChordMetrics.StemMetrics.OriginX;
            float tanAlpha = (height / width) / 3;

            if(tanAlpha > 0.15F)
                tanAlpha = 0.15F;
            if(tanAlpha < -0.15F)
                tanAlpha = -0.15F;

            return tanAlpha;
        }

        /// </summary>
        /// <param name="chordDaten"></param>
        /// <returns></returns>
        private float ShearAxis(List<ChordMetrics> chordsMetrics, float tanAlpha)
        {
            List<float> innerNoteheadHeights = GetInnerNoteheadHeights(chordsMetrics);
            float smallestDistance = float.MaxValue;
            foreach(float distance in innerNoteheadHeights)
                smallestDistance = smallestDistance < distance ? smallestDistance : distance;

            List<int> indices = new List<int>();
            for(int i = 0; i < innerNoteheadHeights.Count; ++i)
            {
                if(innerNoteheadHeights[i] == smallestDistance)
                    indices.Add(i);
            }
            if((_stemDirection == VerticalDir.down && tanAlpha <= 0)
            || (_stemDirection == VerticalDir.up && tanAlpha > 0))
                return chordsMetrics[indices[indices.Count - 1]].StemMetrics.OriginX;
            else
                return chordsMetrics[indices[0]].StemMetrics.OriginX;
        }

        private List<float> GetInnerNoteheadHeights(List<ChordMetrics> chordsMetrics)
        {
            List<float> distances = new List<float>();
            if(_stemDirection == VerticalDir.down)
            {
                foreach(ChordMetrics chordMetrics in chordsMetrics)
                {
                    distances.Add(-(chordMetrics.BottomHeadMetrics.OriginY));
                }
            }
            else
            {
                foreach(ChordMetrics chordMetrics in chordsMetrics)
                {
                    distances.Add(chordMetrics.TopHeadMetrics.OriginY);
                }
            }
            return distances;
        }

        /// <summary>
        /// Resets the height and angle of the beamBlock, when it is too low.
        /// </summary>
        /// <param name="chordsMetrics"></param>
        /// <param name="durationClassBeamThickness"></param>
        private void SetToStaff(Dictionary<DurationClass, float> durationClassBeamThickness)
        {
            float staffTop = Chords[0].Voice.Staff.Metrics.OriginY;
            float staffBottom = staffTop + (_gap * (Chords[0].Voice.Staff.NumberOfStafflines -1));
            float staffMiddleY = (staffTop + staffBottom) / 2F;
            float deltaY;
            if(this._stemDirection == VerticalDir.up)
            {
                if(Beams.Count == 1)
                {
                    deltaY = staffMiddleY + (_gap * 0.35F) - this._bottom;
                    if(deltaY < 0F)
                        Move(deltaY);
                }
                else if(this._bottom >= staffBottom)
                {
                    deltaY = staffMiddleY + (_gap * 1.35F) - this._bottom;
                    if(deltaY < 0F)
                        Move(deltaY);
                }
            }
            else // this._stemDirection == VerticalDir.down
            {
                if(Beams.Count == 1)
                {
                    deltaY = staffMiddleY - (_gap * 0.35F) - this._top;
                    if(deltaY > 0F)
                        Move(deltaY);
                }
                else if(this._top <= staffTop)
                {
                    deltaY = staffMiddleY - (_gap * 1.35F) - this._top;
                    if(deltaY > 0F) 
                        Move(deltaY);
                }
            }
        }

        private void MoveStemTips()
        {
            float staffOriginY = Chords[0].Voice.Staff.Metrics.OriginY;
            QuaverBeam quaverBeam = null;
            foreach(Beam beam in Beams)
            {
                quaverBeam = beam as QuaverBeam;
                if(quaverBeam != null)
                    break;
            }
            Debug.Assert(quaverBeam != null);
            float tanAlpha = (quaverBeam.RightTopY - quaverBeam.LeftTopY) / (quaverBeam.RightX - quaverBeam.LeftX);

            foreach(OutputChordSymbol chord in Chords)
            {
                ChordMetrics chordMetrics = ((ChordMetrics)chord.Metrics);
                StemMetrics stemMetrics = chordMetrics.StemMetrics; // a clone

                Debug.Assert(chord.Stem.Direction == _stemDirection); // just to be sure.

                float stemTipDeltaY = ((stemMetrics.OriginX - this._left) * tanAlpha);
                float stemTipY = quaverBeam.LeftTopY + stemTipDeltaY;
                chordMetrics.MoveOuterStemTip(stemTipY, _stemDirection); // dont just move the clone! Moves the auxilliaries too.
            }
        }

        /// <summary>
        /// The tangent of this beam's angle. This value is positive if the beam slopes upwards to the right.
        /// </summary>
        /// <returns></returns>
        private float TanAngle
        {
            get
            {
                QuaverBeam qBeam = null;
                foreach(Beam beam in Beams)
                {
                    qBeam = beam as QuaverBeam;
                    if(qBeam != null)
                        break;
                }
                Debug.Assert(qBeam != null);
                float tan = ((qBeam.LeftTopY - qBeam.RightTopY) / (qBeam.RightX - qBeam.LeftX));
                return tan;
            }
        }
        /// <summary>
        /// Returns a list of HLine representing the outer edge of the outer (=quaver) beam.
        /// </summary>
        /// <returns></returns>
        public List<HLine> OuterEdge()
        {
            QuaverBeam qBeam  = null;
            foreach(Beam beam in Beams)
            {
                qBeam = beam as QuaverBeam;
                if(qBeam != null)
                    break;
            }
            Debug.Assert(qBeam != null);

            float heightDiff = qBeam.LeftTopY - qBeam.RightTopY; // N.B. old bug. This value should be positive!

            float stepHeight = (_beamThickness * 0.2F);
            int nSteps = (int)(heightDiff / stepHeight);
            if(nSteps < 0)
                nSteps *= -1;
            stepHeight = heightDiff / nSteps;
            float stepWidth = (_right - _left) / nSteps;

            float left = _left;
            float top = 0F;
            if(_stemDirection == VerticalDir.up)
                top = qBeam.LeftTopY;
            else
                top = qBeam.LeftTopY + _beamThickness;

            float tanAlpha = stepHeight / stepWidth;

            List<HLine> outerEdge = new List<HLine>();
            for(int i = 0; i < nSteps; i++)
            {
                outerEdge.Add(new HLine(left, left + stepWidth, top));
                left += stepWidth;
                top -= (stepWidth * tanAlpha);
            }
            return outerEdge;
        }

        public void ShiftStemsForOtherVoice(Voice otherVoice)
        {
            float minMsPosition = Chords[0].MsPosition;
            float maxMsPosition = Chords[Chords.Count - 1].MsPosition;
            List<OutputChordSymbol> otherChords = new List<OutputChordSymbol>();
            foreach(OutputChordSymbol otherChordSymbol in otherVoice.ChordSymbols)
            {
                if(otherChordSymbol.MsPosition >= minMsPosition && otherChordSymbol.MsPosition <= maxMsPosition)
                    otherChords.Add(otherChordSymbol);
                if(otherChordSymbol.MsPosition > maxMsPosition)
                    break;
            }
            if(otherChords.Count > 0)
            {
                float minimumDistanceToChords = _gap * 2F;
                float distanceToChords = DistanceToChords(otherChords);
                if(_stemDirection == VerticalDir.up) // move the beam up
                {
                    if(distanceToChords < minimumDistanceToChords)
                    {
                        float deltaY = -(minimumDistanceToChords - distanceToChords);
                        this.Move(deltaY);
                        foreach(OutputChordSymbol chord in Chords)
                        {
                            float newStemTipY = chord.ChordMetrics.StemMetrics.Top + deltaY;
                            chord.ChordMetrics.MoveOuterStemTip(newStemTipY, VerticalDir.up);
                        }
                    }
                }
                else // _stemDirection == VerticalDir.down, move the beam down
                {
                    if(distanceToChords < minimumDistanceToChords)
                    {
                        float deltaY = minimumDistanceToChords - distanceToChords;
                        this.Move(deltaY);
                        foreach(OutputChordSymbol chord in Chords)
                        {
                            float newStemTipY = chord.ChordMetrics.StemMetrics.Bottom + deltaY;
                            chord.ChordMetrics.MoveOuterStemTip(newStemTipY, VerticalDir.down);
                        }
                    }
                }
            }   
        }

        /// <summary>
        /// Returns the smallest distance between the inner edge of this beam and the noteheads
        /// in the other chords. The other chords are in a second voice on this staff.
        /// </summary>
        private float DistanceToChords(List<OutputChordSymbol> otherChords)
        {
            float minimumDistanceToChords = float.MaxValue;
            foreach(OutputChordSymbol chord in otherChords)
            {
                float distanceToChord = float.MaxValue;
                if(_stemDirection == VerticalDir.up)
                    distanceToChord = VerticalDistanceToHead(chord.ChordMetrics.TopHeadMetrics, chord.MsPosition);
                else
                    distanceToChord = VerticalDistanceToHead(chord.ChordMetrics.BottomHeadMetrics, chord.MsPosition);

                minimumDistanceToChords = minimumDistanceToChords < distanceToChord ? minimumDistanceToChords : distanceToChord;
            }
            return minimumDistanceToChords;
        }

        /// <summary>
        /// The distance between the inner edge of this beamBlock and the headmetrics.
        /// This is a positive value
        ///     a) if the _stemDirection is VerticalDir.up and the headMetrics is completely below this beamBlock,
        /// or  b) if the _stemDirection is VerticalDir.down and the headMetrics is completely above this beamBlock.
        /// </summary>
        private float VerticalDistanceToHead(HeadMetrics headMetrics, float headMsPosition)
        {
            float headX = headMetrics.OriginX;
            float headY = headMetrics.Top;
            float minDistanceToHead = float.MaxValue;
            float tanA = this.TanAngle;
            if(_stemDirection == VerticalDir.up)
            {
                float beamBottomAtHeadY; 
                foreach(Beam beam in Beams)
                {
                    float beamBeginMsPosition = BeamBeginMsPosition(beam);
                    float beamEndMsPosition = BeamEndMsPosition(beam);
                    if(beamBeginMsPosition <= headMsPosition && beamEndMsPosition >= headMsPosition)
                    {
                        beamBottomAtHeadY = beam.LeftTopY - ((headX - beam.LeftX) * tanA) + _beamThickness;
                        float distanceToHead = headY - beamBottomAtHeadY;
                        minDistanceToHead = minDistanceToHead < distanceToHead ? minDistanceToHead : distanceToHead;
                    }
                }
            }
            else // _stemDirection == VerticalDir.down
            {
                headY = headMetrics.Bottom;
                float beamTopAtHeadY;
                foreach(Beam beam in Beams)
                {
                    float beamBeginMsPosition = BeamBeginMsPosition(beam);
                    float beamEndMsPosition = BeamEndMsPosition(beam);
                    if(beamBeginMsPosition <= headMsPosition && beamEndMsPosition >= headMsPosition)
                    {
                        beamTopAtHeadY = beam.LeftTopY - ((headX - beam.LeftX) * tanA);
                        float distanceToHead = beamTopAtHeadY - headY;
                        minDistanceToHead = minDistanceToHead < distanceToHead ? minDistanceToHead : distanceToHead;
                    }
                }
            }
            return minDistanceToHead;
        }

        private float BeamBeginMsPosition(Beam beam)
        {
            Debug.Assert(this.Beams.Contains(beam));
            float beamBeginMsPosition = float.MinValue;
            foreach(OutputChordSymbol chord in Chords)
            {
                float stemX = chord.ChordMetrics.StemMetrics.OriginX;
                if(stemX == beam.LeftX || stemX == beam.RightX) // rightX can be a beam stub
                {
                    beamBeginMsPosition = chord.MsPosition;
                    break;
                }
            }
            Debug.Assert(beamBeginMsPosition != float.MinValue);
            return beamBeginMsPosition;
        }

        private float BeamEndMsPosition(Beam beam)
        {
            Debug.Assert(this.Beams.Contains(beam));
            float beamEndMsPosition = float.MinValue;
            for(int i = Chords.Count - 1; i >= 0; --i)
            {
                OutputChordSymbol chord = Chords[i];
                float stemX = chord.ChordMetrics.StemMetrics.OriginX;
                if(stemX == beam.LeftX || stemX == beam.RightX) // rightX can be a beam stub
                {
                    beamEndMsPosition = chord.MsPosition;
                    break;
                }
            }
            Debug.Assert(beamEndMsPosition != float.MinValue);
            return beamEndMsPosition;
        }

        public override void WriteSVG(SvgWriter w)
        {
            w.SvgStartGroup("beamBlock", null);
            foreach(Beam beam in Beams)
            {
                if(!(beam is QuaverBeam))
                {
                    float topLeft = 0F;
                    float topRight = 0F;
                    if(_stemDirection == VerticalDir.down)
                    {
                        topLeft = beam.LeftTopY + _beamThickness;
                        topRight = beam.RightTopY + _beamThickness;
                    }
                    else
                    {
                        topLeft = beam.LeftTopY - _beamThickness;
                        topRight = beam.RightTopY - _beamThickness;
                    }
                    w.SvgBeam(null, beam.LeftX, beam.RightX, topLeft, topRight, _beamThickness * 1.5F, 0F, 0.65F);
                }
                w.SvgBeam(null, beam.LeftX, beam.RightX, beam.LeftTopY, beam.RightTopY, _beamThickness, _strokeThickness, 1.0F);
            }
            w.SvgEndGroup();
        }

		public readonly List<OutputChordSymbol> Chords = null;
        public float DefaultStemTipY { get { return _defaultStemTipY; } }
        private readonly float _defaultStemTipY;
        private readonly float _gap;
        private readonly float _beamSeparation; // the distance from the top of one beam to the top of the next beam
        private readonly float _beamThickness;
        private readonly float _strokeThickness;
        private readonly VerticalDir _stemDirection = VerticalDir.none;

        public HashSet<Beam> Beams = new HashSet<Beam>();
	}
}
