using System.Collections.Generic;
using System.Diagnostics;

using Moritz.Globals;
using Moritz.Xml;
using Moritz.Spec;

namespace Moritz.Symbols
{
    public abstract class Staff
    {
        public Staff(SvgSystem svgSystem, string staffName, int numberOfStafflines, float gap, float stafflineStemStrokeWidth)
        {
            SVGSystem = svgSystem;
            Staffname = staffName;
            Debug.Assert(numberOfStafflines > 0);
            NumberOfStafflines = numberOfStafflines; 
            Gap = gap;
            StafflineStemStrokeWidth = stafflineStemStrokeWidth;
        }

        public abstract void WriteSVG(SvgWriter w, int systemNumber, int staffNumber, List<CarryMsgs> carryMsgsPerChannel);

        /// <summary>
        /// staffIsVisble is the global pageFormat setting.
        /// Single, empty staves are also not displayed -- though their rest lengths are written in the score.
        /// carryperChannel is null for InputStaves.
        /// </summary>
        public virtual void WriteSVG(SvgWriter w, bool staffIsVisible, int systemNumber, int staffNumber, List<CarryMsgs> carryMsgsPerChannel)
        {
            if(this.Metrics == null)
            {
                staffIsVisible = false;
            }
            if(staffIsVisible)
            {            
                w.WriteAttributeString("score", "staffName", null, this.Staffname);

                CSSClass stafflinesClass = (Metrics.CSSClass == CSSClass.inputStaff) ? CSSClass.inputStafflines : CSSClass.stafflines;
                CSSClass stafflineClass = (Metrics.CSSClass == CSSClass.inputStaff) ? CSSClass.inputStaffline : CSSClass.staffline;

                w.SvgStartGroup(stafflinesClass.ToString());
                float stafflineY = this.Metrics.StafflinesTop;
                for(int staffLineIndex = 0; staffLineIndex < NumberOfStafflines; staffLineIndex++)
                {
					w.SvgLine(stafflineClass, this.Metrics.StafflinesLeft, stafflineY, this.Metrics.StafflinesRight, stafflineY);

                    if(staffLineIndex < (NumberOfStafflines - 1))
                        stafflineY += Gap;
                }
                w.SvgEndGroup();
            }
			int voiceNumber = 1;
            foreach(Voice voice in Voices)
            {
				voice.WriteSVG(w, staffIsVisible, systemNumber, staffNumber, voiceNumber++, carryMsgsPerChannel);
            }
        }

        /// <summary>
        /// Returns true if the staff contains at least one ChordSymbol. Otherwise false.
        /// </summary>
        public bool ContainsAChordSymbol
        {
            get
            {
                bool containsAChordSymbol = false;
                foreach(Voice voice in this.Voices)
                {
                    foreach(NoteObject noteObject in voice.NoteObjects)
                    {
                        if(noteObject is ChordSymbol)
                        {
                            containsAChordSymbol = true;
                            break;
                        }
                    }
                    if(containsAChordSymbol == true)
                    {
                        break;
                    }
                }
                return containsAChordSymbol;
            }
        }

        #region composition
        /// <summary>
        /// Sets the visibility of naturals in all the chords on this multi-bar staff.
        /// Force naturals to be displayed if this staff contains simultaneous chords which share the same
        /// diatonic pitches, and one of them is not a sharp or flat.
        /// Naturals are also forced if any of the note heights in the chordSymbol is natural and the same 
        /// height as a sharp or flat in the most recent (synchronous) chords on this _staff_.
        /// Returns the final MomentSymbol (which only contains ChordSymbols) on this staff, so that the first
        /// MomentsSymbol in the corresponding staff on the next system can be adjusted too.
        /// If the chordSymbol has no heads (as in Study2b2), nothing happens.
        /// </summary>
        public NoteObjectMoment FinalizeAccidentals(NoteObjectMoment previousStaffMoment)
        {          
            foreach(NoteObjectMoment thisStaffMoment in this.ChordSymbolMoments)
            {
                if(previousStaffMoment != null)
                {
                    ForceNaturals(previousStaffMoment, thisStaffMoment);
                }
                previousStaffMoment = thisStaffMoment;

                ForceNaturals(thisStaffMoment);
            }
            return previousStaffMoment;
        }
        /// <summary>
        /// Forces naturals to be displayed when the most recent staff moment containing chords shares 
        /// diatonic pitches with thisStaffMoment, and those pitches have different accidentals.
        /// </summary>
        private void ForceNaturals(NoteObjectMoment previousStaffMoment, NoteObjectMoment thisStaffMoment)
        {
            foreach(ChordSymbol chordSymbol in thisStaffMoment.ChordSymbols)
            {
                foreach(Head head in chordSymbol.HeadsTopDown)
                {
                    if(head.Alteration == 0)
                    {
                        bool found = false;
                        head.DisplayAccidental = DisplayAccidental.suppress; // naturals are suppressed by default
                        foreach(ChordSymbol previousChordSymbol in previousStaffMoment.ChordSymbols)
                        {
                            foreach(Head previousHead in previousChordSymbol.HeadsTopDown)
                            {
                                if((head.Pitch == previousHead.Pitch) && previousHead.Alteration != 0)
                                {
                                    head.DisplayAccidental = DisplayAccidental.force;
                                    found = true;
                                    break;
                                }
                            }
                            if(found)
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Forces naturals to be displayed when there are two synchronous chordSymbols in thisStaffMoment, 
        /// and they share diatonic pitches having different accidentals.
        /// </summary>
        private void ForceNaturals(NoteObjectMoment thisStaffMoment)
        {
            List<ChordSymbol> momentChordSymbols = new List<ChordSymbol>(2);
            foreach(ChordSymbol chordSymbol in thisStaffMoment.ChordSymbols)
            {
                momentChordSymbols.Add(chordSymbol);
            }
            if(momentChordSymbols.Count == 2)
            {
                foreach(Head head1 in momentChordSymbols[0].HeadsTopDown)
                {
                    foreach(Head head2 in momentChordSymbols[1].HeadsTopDown)
                    {
                        if(head1.Pitch == head2.Pitch)
                        {
                            if(head1.Alteration != 0 && head2.DisplayAccidental != DisplayAccidental.force)
                            {
                                head2.DisplayAccidental = DisplayAccidental.force;
                            }
                            if(head2.Alteration != 0 && head1.DisplayAccidental != DisplayAccidental.force)
                            {
                                head1.DisplayAccidental = DisplayAccidental.force;
                            }
                        }
                    }
                }
            }
        }

        #region AdjustRestsVertically

        /// <summary>
        /// This function is only called for 2-Voice staves.
        /// All ChordMetrics and RestMetrics have been created for the 2-Voice staff.
        /// The rests in both voices are currently aligned on the same middle staffline.
        /// This function moves rests vertically to prevent collisions with objects in the other voice:
        /// 	1. Rests are moved onto/above the top or bottom staffline, depending on which voice they are in.
        /// 	2. Rests are moved outwards so as to remove any remaining collisions with chords or rests in the
        /// 	   other voice.
        /// All rests end up at their final vertical position.
        /// </summary>
        public void AdjustRestsVertically()
        {
            Debug.Assert(Voices.Count == 2);
            MoveRestsOntoOuterStafflines();
            RemoveVerticalRestCollisions();
        }

        /// <summary>
        /// Moves rests onto the top and bottom stafflines. Most rests move two gaps from the centre staffline,
        /// but breve and semibreve rests in the top voice move to a position 1 gap above the top staffline, and
        /// breve and semibreve rests in the bottom voice move onto the bottom staffline.
        /// Breve and semibreve rests outside the stafflines have a single, visible "ledgerline".
        /// </summary>
        private void MoveRestsOntoOuterStafflines()
        {
            for(int voiceIndex = 0; voiceIndex < 2; ++voiceIndex)
            {
                foreach(NoteObject noteObject in Voices[voiceIndex].NoteObjects)
                {
                    RestSymbol rest = noteObject as RestSymbol;
                    if(rest != null)
                    {
                        RestMetrics metrics = (RestMetrics)rest.Metrics;
                        if(voiceIndex == 0)
                        {
                            if(rest.DurationClass == DurationClass.breve || rest.DurationClass == DurationClass.semibreve)
                                metrics.LedgerlineVisible = true; // only affects breves, semibreves and minims
                            metrics.Move(0F, this.Gap * -2);
                        }
                        else
                        {
                            if(rest.DurationClass == DurationClass.breve || rest.DurationClass == DurationClass.semibreve)
                                metrics.Move(0F, this.Gap * 3);
                            else
                                metrics.Move(0F, this.Gap * 2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// There are two voices on this 5-line staff, and rests are currently aligned
        /// on the top and bottom stafflines.
        /// </summary>
        private void RemoveVerticalRestCollisions()
        {
            Debug.Assert(Voices.Count == 2);
            AdjustRestRestCollisions();
            AdjustRestsForVerticalChordCollisions(0);
            AdjustRestsForVerticalChordCollisions(1);       
        }
        private void AdjustRestRestCollisions()
        {
            List<NoteObject> upperObjects = Voices[0].NoteObjects;
            List<NoteObject> lowerObjects = Voices[1].NoteObjects;

            // If a rest in the top voice collides with a rest in the lower voice at the same msPosition, 
            // both rests are moved in gap steps outwards until they no longer overlap.
            foreach(NoteObject topObject in upperObjects)
            {
                RestSymbol topRest = topObject as RestSymbol;
                if(topRest != null)
                {
                    RestMetrics upperRestMetrics = topRest.Metrics as RestMetrics;
                    foreach(NoteObject lowerObject in lowerObjects)
                    {
                        RestSymbol lowerRestSymbol = lowerObject as RestSymbol;
                        if(lowerRestSymbol != null)
                        {
                            if(topRest.AbsMsPosition < lowerRestSymbol.AbsMsPosition)
                                break;
                            if(topRest.AbsMsPosition == lowerRestSymbol.AbsMsPosition)
                            {
                                RestMetrics lowerRestMetrics = (RestMetrics)lowerRestSymbol.Metrics;
                                float verticalOverlap = lowerRestMetrics.OverlapHeight(upperRestMetrics, 0F);
                                bool moveBottomRest = true;
                                while(verticalOverlap > 0)
                                {
                                    float newMinBottom = upperRestMetrics.Bottom - verticalOverlap;
                                    
                                    if(upperRestMetrics.Bottom > newMinBottom)
                                    {
                                        if(moveBottomRest)
                                        {
                                            lowerRestMetrics.LedgerlineVisible = true; // only affects breves, semibreves and minims
                                            lowerRestMetrics.Move(0F, Gap);
                                            moveBottomRest = false;
                                        }
                                        else
                                        {
                                            upperRestMetrics.LedgerlineVisible = true; // only affects breves, semibreves and minims
                                            upperRestMetrics.Move(0F, -Gap);
                                            moveBottomRest = true;
                                        }
                                    }
                                    verticalOverlap = lowerRestMetrics.OverlapHeight(upperRestMetrics, 0F);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void AdjustRestsForVerticalChordCollisions(int restsChannelIndex)
        {
            Debug.Assert(restsChannelIndex == 0 || restsChannelIndex == 1);

            List<NoteObject> restObjects;
            List<NoteObject> chordObjects;
            bool shiftRestUp;
            if(restsChannelIndex == 0)
            {
                shiftRestUp = true;
                restObjects = Voices[0].NoteObjects;
                chordObjects = Voices[1].NoteObjects;
            }
            else
            {
                shiftRestUp = false;
                restObjects = Voices[1].NoteObjects;
                chordObjects = Voices[0].NoteObjects;
            }

            // Move rests in the top voice up by gap increments if they are synchronous with an overlapping chord in the lower voice.
            // Move rests in the bottom voice down by gap increments if they are synchronous with an overlapping chord in the top voice. 
            foreach(NoteObject restObject in restObjects)
            {
                RestSymbol restSymbol = restObject as RestSymbol;
                if(restSymbol != null)
                {
                    foreach(NoteObject chordObject in chordObjects)
                    {
                        OutputChordSymbol chordSymbol = chordObject as OutputChordSymbol;
                        if(chordSymbol != null)
                        {
                            if(chordSymbol.AbsMsPosition > restSymbol.AbsMsPosition)
                                break;

                            if(chordSymbol.AbsMsPosition == restSymbol.AbsMsPosition)
                            {
                                RestMetrics restMetrics = restSymbol.RestMetrics;
                                ChordMetrics chordMetrics = chordSymbol.ChordMetrics;
                                //float verticalOverlap = chordMetrics.OverlapHeight(restMetrics, Gap / 2F);
                                float verticalOverlap = chordMetrics.OverlapHeight(restMetrics, 0F);
                                if(verticalOverlap > 0)
                                {
                                    if(shiftRestUp)
                                    {
                                        //float newMaxBottom = chordMetrics.Top - Gap;
                                        float newMaxBottom = chordMetrics.Top;
                                        newMaxBottom += DurationClassDeltaAbove(restSymbol.DurationClass, Gap);
                                        while(restMetrics.Bottom > newMaxBottom)
                                        {
                                            restMetrics.LedgerlineVisible = true; // only affects breves, semibreves and minims
                                            restMetrics.Move(0F, -Gap);
                                        }
                                        break; // to next rest symbol
                                    }
                                    else
                                    {
                                        //float newMinTop = chordMetrics.Bottom + Gap;
                                        float newMinTop = chordMetrics.Bottom;
                                        newMinTop += DurationClassDeltaBelow(restSymbol.DurationClass, Gap);
                                        while(restMetrics.Top < newMinTop)
                                        {
                                            restMetrics.LedgerlineVisible = true; // only affects breves, semibreves and minims
                                            restMetrics.Move(0F, Gap);
                                        }
                                        break; // to next rest symbol
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private float DurationClassDeltaAbove(DurationClass durationClass, float gap)
        {
            float delta = 0F;
            switch(durationClass)
            {
                case DurationClass.semibreve:
                    delta = gap / -2F;
                    break;
                case DurationClass.breve:
                case DurationClass.minim:
                case DurationClass.crotchet:
                case DurationClass.quaver:
                case DurationClass.semiquaver:
                case DurationClass.threeFlags:
                    break;
                case DurationClass.fourFlags:
                    delta = gap / -2F;
                    break;
                case DurationClass.fiveFlags:
                    delta = gap * -1.5F;
                    break;
            }
            return delta;
        }
        private float DurationClassDeltaBelow(DurationClass durationClass, float gap)
        {
            float delta = 0F;
            switch(durationClass)
            {
                case DurationClass.breve:
                case DurationClass.semibreve:
                    break;
                case DurationClass.minim:
                    delta = gap / 2F;
                    break;
                case DurationClass.crotchet:
                    delta = gap / 3F;
                    break;
                case DurationClass.quaver:
                case DurationClass.semiquaver:
                    break;
                case DurationClass.threeFlags:
                    delta = gap / 2F;
                    break;
                case DurationClass.fourFlags:
                    delta = gap / -2F;
                    break;
                case DurationClass.fiveFlags:
                    delta = gap * -1.5F;
                    break;
            }
            return delta;
        }
        #endregion AdjustRestsVertically

        /// <summary>
        /// This staff has two voices, both of which contain properly formatted chords.
        /// The first (upper) voice has its stems up, the second (lower) voice has its stems down.
        /// When two chords have the same MsPosition and overlap graphically, this function 
        ///   1. adjusts the stem lengths so that there will be no collisions between the flagBlocks
        ///      and the noteheads in the other chord. 
        ///   2. adjusts the horizontal position of the lower chord.
        ///   3. adjusts the horizontal positions of all accidentals so that they are placed as if
        ///      this were just one chord. Altered unisons are written with the accidentals positioned
        ///      left-right to correspond with the positions of their respective noteheads.
        /// </summary>
        public void AdjustTwoPartChords()
        {
            Debug.Assert(Voices.Count == 2);
            Debug.Assert(Voices[0].StemDirection == VerticalDir.up);
            Debug.Assert(Voices[1].StemDirection == VerticalDir.down);
            foreach(ChordSymbol upperChord in Voices[0].ChordSymbols)
            {
                foreach(ChordSymbol lowerChord in Voices[1].ChordSymbols)
                {
                    if(upperChord.AbsMsPosition == lowerChord.AbsMsPosition)
                    {
                        AdjustStemLengths(upperChord, lowerChord);
                        // N.B. the stems lengths must have been corrected for
                        // crossing parts before calling the following function.
                        /// The positions of accidentals are adjusted in both chords to avoid collisions 
                        /// (even if the lower chord does not move).
                        AdjustLowerChordXPosition(upperChord, lowerChord);
                        break;
                    }
                }
            }
        }


        private void AdjustStemLengths(ChordSymbol upperChord, ChordSymbol lowerChord)
        {
            bool isInput = (upperChord is InputChordSymbol);
            upperChord.ChordMetrics.AdjustStemLengthAndFlagBlock(upperChord.DurationClass, upperChord.FontHeight, lowerChord.ChordMetrics.HeadsMetrics, isInput);
            lowerChord.ChordMetrics.AdjustStemLengthAndFlagBlock(lowerChord.DurationClass, lowerChord.FontHeight, upperChord.ChordMetrics.HeadsMetrics, isInput);
        }

        /// <summary>
        /// Adjusts the heights of stem tips attached to beam blocks in staves which have 2 voices.
        /// The stem tips are moved vertically (all to the same height) to take account of chords in the other 
        /// voice which would otherwise collide with the beamBlock.
        /// This is done so that account can be taken of the stems and auxilliaries when justifying horizontally.
        /// The BeamBlocks themselves are finalized later, at which point the stem tips are moved again.
        /// </summary>
        public void AdjustBeamedStemHeights(int voiceIndex)
        {
            Debug.Assert(Voices.Count == 2);
            Debug.Assert(voiceIndex == 0 || voiceIndex == 1);
            Debug.Assert(Voices[0].StemDirection == VerticalDir.up);
            Debug.Assert(Voices[1].StemDirection == VerticalDir.down);

            int adjustVoiceIndex = 0;
            int otherVoiceIndex = 1;
            if(voiceIndex == 1)
            {
                adjustVoiceIndex = 1;
                otherVoiceIndex = 0;
            }
            foreach(ChordSymbol chordSymbol in Voices[adjustVoiceIndex].ChordSymbols)
            {
                if(chordSymbol.BeamBlock != null)
                {
                    BeamBlock beamBlock = chordSymbol.BeamBlock;
                    List<ChordSymbol> enclosedChords = beamBlock.EnclosedChords(Voices[otherVoiceIndex]);
                    foreach(ChordSymbol chord in beamBlock.Chords)
                    {
                        foreach(ChordSymbol otherChord in enclosedChords)
                        {
                            chord.ChordMetrics.AdjustStemLengthAndFlagBlock(chord.DurationClass, chord.FontHeight, otherChord.ChordMetrics.HeadsMetrics, (chord is InputChordSymbol));
                        }
                    }
                    if(adjustVoiceIndex == 0)
                    {
                        float minStemTip = float.MaxValue;
                        foreach(ChordSymbol beamedChord in beamBlock.Chords)
                        {
                            minStemTip = minStemTip < beamedChord.ChordMetrics.StemMetrics.Top ? minStemTip : beamedChord.ChordMetrics.StemMetrics.Top;
                        }
                        foreach(ChordSymbol beamedChord in beamBlock.Chords)
                        {
                            beamedChord.ChordMetrics.MoveOuterStemTip(minStemTip, VerticalDir.up);
                        }
                    }
                    else
                    {
                        float maxStemTip = float.MinValue;
                        foreach(ChordSymbol beamedChord in beamBlock.Chords)
                        {
                            maxStemTip = maxStemTip > beamedChord.ChordMetrics.StemMetrics.Bottom ? maxStemTip : beamedChord.ChordMetrics.StemMetrics.Bottom;
                        }
                        foreach(ChordSymbol beamedChord in beamBlock.Chords)
                        {
                            beamedChord.ChordMetrics.MoveOuterStemTip(maxStemTip, VerticalDir.down);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Adjusts the heights of stem tips and beamBlocks in staves which have 2 voices.
        /// Only adjusts stems of chords belonging to beamBlocks.
        /// </summary>
        public void AdjustStemAndBeamBlockHeights(int voiceIndex)
        {
            Debug.Assert(Voices.Count == 2);
            Debug.Assert(voiceIndex == 0 || voiceIndex == 1);
            Debug.Assert(Voices[0].StemDirection == VerticalDir.up);
            Debug.Assert(Voices[1].StemDirection == VerticalDir.down);

            int adjustVoiceIndex = 0;
            int otherVoiceIndex = 1;
            if(voiceIndex == 1)
            {
                adjustVoiceIndex = 1;
                otherVoiceIndex = 0;
            }
            foreach(ChordSymbol chordSymbol in Voices[adjustVoiceIndex].ChordSymbols)
            {
                DurationClass durationClass = chordSymbol.DurationClass;
                if(durationClass == DurationClass.quaver
                || durationClass == DurationClass.semiquaver
                || durationClass == DurationClass.threeFlags
                || durationClass == DurationClass.fourFlags
                || durationClass == DurationClass.fiveFlags)
                {
                    // fix stem and beam
                    if(chordSymbol.BeamBlock != null)
                        chordSymbol.BeamBlock.ShiftStemsForOtherVoice(Voices[otherVoiceIndex]);
                }
            }
        }

        /// <summary>
        /// This function moves the lowerChord to the left or right in order to avoid collisions with 
        /// the noteheads of the upper chord.
        /// The positions of noteheads and ledgerlines relative to their own stem is never changed.
        /// The positions of accidentals are adjusted in both chords after the lower chord has moved.
        /// Stem lengths, and the positions of flags and beamBlocks, are adjusted later in FinalizeBeamBlocks().
        /// 
        /// The stems are currently at the standard x-positions for stems up and down at the same MsPosition.
        /// Their lengths have been changed if necessary, for crossing parts. So collisions can be checked
        /// reliably in this function.
        /// There are 7 possible horizontal positions for the stem of the lower chord:
        ///     1. the standard position (aligned as when the lower chord is well below the upper chord)
        ///     2. hairline left of upper noteheads
        ///     3. aligned with upper stem 
        ///          (top notehead of the bottom chord is half a space below the bottom notehead of the upper chord)
        ///     4. thin hairline right of upper stem
        ///          (top notehead of the bottom chord is at the same height as the bottom notehead of the upper chord)
        ///     5. thick hairline right of upper stem
        ///          (top notehead of the bottom chord is above the bottom notehead of the upper chord)
        ///     If both upper and lower chords have sideways shifted noteheads, and there are no notehead collisions:
        ///         6. thick hairline right of right-side note head on upper stem
        ///     else
        ///         7. a head width left of its original position
        /// The position selected, is the first of these which can be applied without causing any collisions.
        /// The principle is that the total width of all the noteheads should be minimized.
        /// Accidentals are rearranged (top to bottom, to the left of the combined chord) once the
        /// noteheads and ledgerlines have been given their final positions.
        /// </summary>
        private void AdjustLowerChordXPosition(ChordSymbol upperChord, ChordSymbol lowerChord)
        {
            Debug.Assert(upperChord.AbsMsPosition == lowerChord.AbsMsPosition);
            if(!(upperChord is CautionaryChordSymbol))
            {
                Debug.Assert(upperChord.Stem.Direction == VerticalDir.up);
            }
            if(!(lowerChord is CautionaryChordSymbol))
            {
                Debug.Assert(lowerChord.Stem.Direction == VerticalDir.down);
            }

            List<HeadMetrics> upperChordHeadMetrics = upperChord.ChordMetrics.HeadsMetrics; // a clone
            List<HeadMetrics> lowerChordHeadMetrics = lowerChord.ChordMetrics.HeadsMetrics; // a clone
            StemMetrics lowerChordStemMetrics = lowerChord.ChordMetrics.StemMetrics; // a clone

            float deltaX = LowerChordDeltaX(upperChordHeadMetrics, lowerChordHeadMetrics, lowerChordStemMetrics);

            if(deltaX != 0)
            {
                lowerChord.ChordMetrics.Move(deltaX, 0F); // move the whole chord, including accidentals
            }
            // adjust the positions of accidentals in both chords
            lowerChord.AdjustAccidentalsX(upperChord);
        }

        /// <summary>
        /// Returns the amount by which to move the lower of two synchronous 
        /// chords to avoid notehead and stem collisions between them.
        /// </summary>
        private float LowerChordDeltaX(List<HeadMetrics> upperHM, List<HeadMetrics> lowerHM, StemMetrics lowerChordStemMetrics)
        {
            float deltaX = 0F;
            float iota = 0.001F; // tolerance for float comparisons
            float stemThickness = SVGSystem.Score.PageFormat.StafflineStemStrokeWidth;
            float upperHeadRightStemX = upperHM[upperHM.Count - 1].RightStemX;
            float lowerHeadLeftStemX = lowerHM[0].LeftStemX;
            float verticalChordOverlap = upperHM[upperHM.Count - 1].Top - lowerHM[0].Top + Gap;

            // position 1: if there is no vertical overlap between the chords, deltaX is 0
            if(verticalChordOverlap > 0F)
            {  
                // position 2: hairline left of upper noteheads
                float testDeltaX = - (stemThickness * 2F);
                if(NoNoteheadCollisions(testDeltaX + iota, upperHM, lowerHM))
                {
                    deltaX = testDeltaX;
                }

                if(deltaX == 0F)
                {   // (nearly) aligned with upper stem
                    testDeltaX = upperHeadRightStemX - lowerHeadLeftStemX;
                    if((upperHM[upperHM.Count - 1].Bottom > lowerHM[0].Top && upperHM[upperHM.Count - 1].Top < lowerHM[0].Top)
                    && NoNoteheadCollisions(testDeltaX + iota, upperHM, lowerHM))
                    {
                        // position 3: The lowest note of the upper chord is a half space above the 
                        // highest notehead in the lower chord, so the stems should align exactly.
                        deltaX = testDeltaX - stemThickness; // Strange, but effective! 
                    }
                    if(deltaX == 0)
                    {
                        if(lowerHM[0].OriginY > ((upperHM[upperHM.Count - 1].Top + upperHM[upperHM.Count - 1].OriginY) / 2)
                            && NoNoteheadCollisions(testDeltaX + iota, upperHM, lowerHM))
                        {
                            // position 4: The lowest note of the upper chord is at the same height as 
                            // highest notehead in the lower chord, so the stems should be slightly separated.
                            deltaX = testDeltaX + (stemThickness * 0.6F); // Strange, but effective!
                        }
                    }
                    if(deltaX == 0F)
                    {
                        testDeltaX += (stemThickness * 1.8F);
                        if(NoNoteheadCollisions(testDeltaX + iota, upperHM, lowerHM))
                        {
                            // position 5: The lowest note of the upper chord is below the highest notehead 
                            // in the lower chord, so the stems should be separated slightly more.
                            if(NoHeadStemCollisions(testDeltaX + iota, upperHM, lowerChordStemMetrics)) // stem metrics can be null...
                            {
                                deltaX = testDeltaX;
                            }
                        }
                    }
                }

                if(deltaX == 0F && HasSidewaysShiftedNotehead(upperHM) && HasSidewaysShiftedNotehead(lowerHM))
                {
                    // position 6: align the lower stem a hairline right of a right-shifted notehead on the upper chord 
                    testDeltaX = ((upperHeadRightStemX - lowerHeadLeftStemX) * 2) + (stemThickness * 1.8F);
                    if(NoNoteheadCollisions(testDeltaX + iota, upperHM, lowerHM))
                    {
                        deltaX = testDeltaX;
                    }
                }

                if(deltaX == 0F)
                {
                    // position 7: align lower stem a head width left of its original position
                    deltaX = lowerHeadLeftStemX - lowerHM[0].RightStemX;
                }
            }

            return deltaX;
        }

        private bool NoNoteheadCollisions(float deltaX, List<HeadMetrics> upperHMList, List<HeadMetrics> lowerHMList)
        {
            // N.B. Notehead metrics.Left and metrics.Right include horizontal padding,
            // so this function does not use the standard Metrics.Overlaps functions.
            bool noteheadCollisions = false;
            for(int i = upperHMList.Count - 1; i >= 0; --i) // hopefully, reverse finds collisions more quickly
            {
                upperHMList[i].Move(-deltaX, 0F);
                foreach(HeadMetrics lowerHM in lowerHMList)
                {
                    if(lowerHM.OverlapsHead(upperHMList[i]))
                    {
                        noteheadCollisions = true;
                        break;
                    }
                }
                upperHMList[i].Move(deltaX, 0F);
                if(noteheadCollisions)
                    break;
            }
            return !noteheadCollisions;
        }

        private bool NoHeadStemCollisions(float deltaX, List<HeadMetrics> upperHMList, StemMetrics lowerStemMetrics)
        {
            // N.B. Notehead metrics.Left and metrics.Right include horizontal padding,
            // so this function does not use the standard Metrics.Overlaps functions.
            bool noHeadStemCollisions = true;
            if(lowerStemMetrics != null)
            {
                lowerStemMetrics.Move(deltaX, 0F);
                foreach(HeadMetrics upperHM in upperHMList)
                {
                    if(upperHM.OverlapsStem(lowerStemMetrics))
                    {
                        noHeadStemCollisions = false;
                        break;
                    }
                }
                lowerStemMetrics.Move(-deltaX, 0F); // its a clone of the real stemMetrics, but move it back anyway.
            }
            return noHeadStemCollisions;
        }
        /// <summary>
        /// True if there is a notehead on each side of the stem, otherwise false.
        /// </summary>
        private bool HasSidewaysShiftedNotehead(List<HeadMetrics> topDownHeadMetrics)
        {
            bool hasLeftShiftedNotehead = false;
            
            if(topDownHeadMetrics.Count > 1)
            {
                float leftLimit = 
                    topDownHeadMetrics[0].Left - ((topDownHeadMetrics[0].Right - topDownHeadMetrics[0].Left) / 4F);

                for(int i = 1; i < topDownHeadMetrics.Count; ++i)
                {
                    if(topDownHeadMetrics[i].Left < leftLimit)
                    {
                        hasLeftShiftedNotehead = true;
                        break;
                    }
                }
            }
            return hasLeftShiftedNotehead;
        }

        #endregion composition

        #region enumerators
        /// <summary>
        /// An enumerator representing a list of MomentSymbols in order of MsPosition.
        /// Each MomentSymbol contains a list of synchronous chordSymbols
        /// Symbols in the MomentSymbols are in order of voice.
        /// </summary>
        public IEnumerable<NoteObjectMoment> ChordSymbolMoments
        {
            get
            {
                List<NoteObjectMoment> momentSymbols = MomentSymbols<OutputChordSymbol>();
                foreach(NoteObjectMoment momentSymbol in momentSymbols)
                    yield return momentSymbol;
            }
        }
        /// <summary>
        /// Returns a list of MomentSymbols containing objects of the type given in the Type argument.
        /// Type can be DurationSymbol or any class which inherits from DurationSymbol.
        /// All DurationSymbol msPositions must be set before calling this function.
        /// The MomentSymbols are in order of msPosition.
        /// The contained symbols are in order of voice (top-bottom of this system).
        /// </summary>
        /// <typeparam name="Type">DurationSymbol, ChordSymbol, RestSymbol</typeparam>
        private List<NoteObjectMoment> MomentSymbols<Type>()
        {
            Dictionary<int, NoteObjectMoment> dict = new Dictionary<int, NoteObjectMoment>();
            foreach(Voice voice in this.Voices)
            {
                foreach(NoteObject noteObject in voice.NoteObjects)
                {
                    DurationSymbol symbol = noteObject as DurationSymbol;
                    if(symbol != null && symbol is Type)
                    {
                        Debug.Assert(symbol.AbsMsPosition >= 0,
                             "Symbol.MsPosition must be set before calling this funcion!");
                        if(!dict.ContainsKey(symbol.AbsMsPosition))
                        {
                            NoteObjectMoment nom = new NoteObjectMoment(symbol.AbsMsPosition);
                            nom.Add(symbol);
                            dict.Add(symbol.AbsMsPosition, nom);
                        }
                        else
                        {
                            dict[symbol.AbsMsPosition].Add(symbol);
                        }
                    }
                }
            }
            List<NoteObjectMoment> momentSymbols = new List<NoteObjectMoment>();
            while(dict.Count > 0)
            {
                int smallestKey = int.MaxValue;
                Debug.Assert(dict.Count > 0);
                foreach(int key in dict.Keys)
                {
                    smallestKey = key < smallestKey ? key : smallestKey;
                }
                momentSymbols.Add(dict[smallestKey]);
                dict.Remove(smallestKey);
            }
            return momentSymbols;
        }
        #endregion enumerators

        #region staff element fields
        /// <summary>
        /// A sequence of Voice elements (the staff's voices).
        /// Moritz only supports two voices per staff.
        /// </summary>
        public List<Voice> Voices = new List<Voice>(2);
        public readonly string Staffname;
        /// <summary>
        /// This staff's container
        /// </summary>
        internal readonly SvgSystem SVGSystem;
        internal readonly int NumberOfStafflines = 0;
        internal readonly float Gap = 0;
        internal readonly float StafflineStemStrokeWidth = 0;
        // Empty staves are invisble. Their Metrics attribute remains null.
        internal StaffMetrics Metrics = null;
        #endregion
    }
}
