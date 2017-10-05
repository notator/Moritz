using System.Collections.Generic;
using Moritz.Globals;
using System.ComponentModel;

namespace Moritz.Xml
{
    /// <summary>
    /// This enum contains _all_ the element classes that Moritz can write to SVG scores.
    /// When creating a particular score, Moritz records the classes that are actually used,
    /// and only writes CSS *definitions* as necessary.
    /// Moritz tries to minimize the CSS definitions, taking into account the way the elements nest.
    /// Each element class has a container, as noted in the comments below.
    /// </summary>
    public enum CSSClass
    {
        none,    // Exists and is recorded, but is never used as an element class and never given a CSS definition!
        metadata, // Used and defined on every page. Not recorded. Container:svg. This metadata element is compatible with Inkscape's.
        frame,       // Used and defined for single pages, not for scroll formatted scores. Not recorded. Container:svg.
        timeStamp,  // Used and defined on every page. Not recorded. Container:systems.
        titles,  // Used and defined on page 1. Not recorded. Container:systems.
        mainTitle,  // Used and defined on page 1. Not recorded. Container:titles. 
        author,     // Used and defined on page 1. Not recorded. Container:titles.
        systems, // Used and defined on every page. Not recorded. Container:svg.
        system, // Used and defined on every page. Not recorded. Container:systems.
        staffName, inputStaffName, // recorded and used. Container:voice/inputVoice.
        staff, inputStaff, // recorded and used. Not defined. Container:system.
        stafflines, inputStafflines, // used but not recorded. Not defined. Container:staff/inputStaff.
        staffline, inputStaffline, // used but not recorded. Defined if staff/inputStaff exists. Container:stafflines/inputStafflines.
        voice, inputVoice, // used, but has no Metrics, inherits definition from staff or inputStaff. Container:staff/inputStaff.

        // all event symbol graphics are contained in a graphics group. Moritz never writes a CSS definition for this group.
        graphics, // Contained by: chord, inputChord, rest, inputRest.

        #region clefs
        /// Clefs whose ID (e.g. "trebleClef8") is recorded in the ClefMetrics.UsedClefIDs list will be written to the defs.
        clef, inputClef,    // Used, recorded. (A normal clef at the beginning of a staff) Container:voice/inputVoice.
        smallClef, inputSmallClef,  // Used, recorded. (A clef change in a voice.)Container:voice/inputVoice.
        /// The following classes are not recorded, but will be defined if they are used by the recorded clefs.
        /// These classes are used in the SVG defs element.
        clefOctaveNumber, inputClefOctaveNumber,   // A number above or below a normal clef. Container:clef/inputClef.
        smallClefOctaveNumber, inputSmallClefOctaveNumber, // A number above or below a small clef.Container:clef/inputClef.
        clefX, inputClefX,      // The 'x' above or below a normal clef. (Arial) Container:clef/inputClef.
        smallClefX, inputSmallClefX, // The 'x' above or below a small clef. (Arial) Container:clef/inputClef.
        #endregion

        #region barNumbers
        // recorded and used but never defined. If it exists, both barNumberNumber and barNumberFrame must be defined.
        barNumber, // Container: The first voice in a system, but not in the first system in the score.
        // not recorded, but used if barNumber exists (it is part of barNumber). Must be defined if barNumber exists.      
        barNumberFrame, // Container:barNumber
        // recorded and used if barNumber exists.  
        barNumberNumber, // Container:barNumber
        #endregion barNumbers

        #region chord classes
        chord, // Container:voice
        inputChord, // Container:inputVoice
        cautionaryChord, // Container:voice (there are no cautionaryChords in inputVoices)
        #endregion chord classes

        #region chord components
        stem, inputStem, // used and recorded. Defined if they exist. Container:chord/inputChord
        // flags and inputFlags whose ID (e.g. "inputLeft3Flags") has been recorded in FlagIDs will be written to the defs.
        flag, inputFlag, // path classes used in defs. Defined if flags or input flags exist. Container:chord/inputChord
        notehead, inputNotehead, cautionaryNotehead, // Container:chord/inputChord/cautionaryChord
        accidental, inputAccidental, cautionaryAccidental, // Container:chord/inputChord/cautionaryChord
        ledgerlines, inputLedgerlines,   // recorded and used. There are no cautionaryLedgerlines. Container:chord/cautionaryChord and inputChord
        ledgerline, inputLedgerline,    // used, but not recorded. Defined if ledgerlines/inputLedgerlines exist. Container: ledgerlines/inputLedgerlines
        ornament,       // Container:chord. There are no input or cautionary ornaments
        lyric, inputLyric,   // Container:chord/inputChord. There are no cautionaryLyrics
        dynamic, inputDynamic,   // Container:chord/inputChord. There are no cautionaryDynamics
        cautionaryBracket,  // Container:chord/cautionaryChord. There are no inputCautionaryBrackets
        noteExtender,       // Container:chord/cautionaryChord. there are no inputNoteExtenders
        #endregion chord components

        #region rest
        rest, inputRest, // Container:voice/inputVoice
        #endregion rest

        #region beams
        beamBlock,      // Used, recorded, not defined. Container:voice
        beam,           // Used, not recorded. Defined if beamBlock exists. Container:beamBlock
        opaqueBeam,     // Used, not recorded. Defined if beamBlock exists. Container:beamBlock
        inputBeamBlock, // Used, recorded, not defined. Container:inputVoice
        inputBeam,      // Used, not recorded. Defined if inputBeamBlock exists. Container:inputBeamBlock
        inputOpaqueBeam, // Used, not recorded. Defined if inputBeamBlock exists. Container:inputBeamBlock
        #endregion

        #region barlines
        barline, // always used, recorded and defined. A normal barline and endBarline component. Container:voice/inputVoice
        endBarline, // always used and recorded. A barline + thickBarline group. Never defined. Container:voice/inputVoice
        thickBarline, // always used and recorded. An endBarline component. Always defined. Container:endBarline
        staffConnectors, // A group of barline (and maybe endBarline) in a system. Never defined. Container:system. 
        #endregion barlines

		#region colours
		transparent, // no colour
		black,
		white,
		red,
		#region Tombeau 1 notehead subclasses
		// The actual colours (written to the CSS *definitions* in the score) are defined in MoritzStatics.NoteheadColors
		fffColor,
		ffColor,
		fColor,
		mfColor,
		mpColor,
		pColor,
		ppColor,
		pppColor,
		ppppColor
		#endregion Tombeau 1 notehead subclasses
		#endregion colours
	};

    public enum CSSLineCap { butt, round, square };
}