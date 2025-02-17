using System.ComponentModel;

namespace Moritz.Xml
{
    /// <summary>
    /// These enums contains _all_ the CSS classes that Moritz can write to SVG scores.
    /// When creating a particular score, Moritz records the classes that are actually used,
    /// and only writes CSS *definitions* as necessary.
    /// Moritz tries to minimize the CSS definitions, taking into account the way the elements nest.
    /// </summary>

    /// <summary>
    /// This enum contains _all_ the *object* classes that Moritz can write to SVG scores.
    /// Each object class has a container, as noted in the comments below.
    /// </summary>
    public enum CSSObjectClass
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
        staffName, // recorded and used. Container:voice.
        staff, // recorded and used. Not defined. Container:system.
        stafflines, // used but not recorded. Not defined. Container:staff.
        staffline, // used but not recorded. Defined if staff exists. Container:stafflines.
        voice, // used, but has no Metrics, inherits definition from staff. Container:staff.

        // all event symbol graphics are contained in a graphics group. Moritz never writes a CSS definition for this group.
        graphics, // Contained by: chord, rest.

        #region clefs
        /// Clefs whose ID (e.g. "trebleClef8") is recorded in the ClefMetrics.UsedClefIDs list will be written to the defs.
        clef, // Used, recorded. (A normal clef at the beginning of a staff) Container:voice.
        smallClef, // Used, recorded. (A clef change in a voice.) Container:voice.
        /// The following classes are not recorded, but will be defined if they are used by the recorded clefs.
        /// These classes are used in the SVG defs element.
        clefOctaveNumber, // A number above or below a normal clef. Container:clef.
        smallClefOctaveNumber, // A number above or below a small clef.Container:clef.
        clefX, // The 'x' above or below a normal clef. (Arial) Container:clef.
        smallClefX, // The 'x' above or below a small clef. (Arial) Container:clef.
        #endregion

        #region drawObjects
        drawObjects,
        #region barNumbers
        // recorded and used but never defined. If it exists, both barNumberNumber and barNumberFrame must be defined.
        barNumber, // Container: The first voice in a system, but not in the first system in the score.
                   // not recorded, but used if barNumber exists (it is part of barNumber). Must be defined if barNumber exists.      
        barNumberFrame, // Container:barNumber
                        // recorded and used if barNumber exists.  
        barNumberNumber, // Container:barNumber
        #endregion barNumbers

        #region regionInfo
        framedRegionInfo, // Container: The first voice in a system.
                          // not recorded, but is used if its contents are used. Must be defined if regionInfoString exists.
        regionInfoString,   // Container: regionInfo
                            // recorded and used if regionInfo exists
        regionInfoFrame,    // Container: regionInfo
                            // recorded and used if regionInfo exists
        #endregion regionInfo
        #endregion drawObjects

        #region chord classes
        chord, // Container:voice
        cautionaryChord, // Container:voice
        #endregion chord classes

        #region chord components
        stem, // used and recorded. Defined if it exists. Container:chord
        // flags whose ID has been recorded in FlagIDs will be written to the defs.
        flag, // path classes used in defs. Defined if flags exist. Container:chord
        notehead, cautionaryNotehead, // Container:chord/cautionaryChord
        accidental, cautionaryAccidental, // Container:chord/cautionaryChord
        ledgerlines, // recorded and used. There are no cautionaryLedgerlines. Container:chord/cautionaryChord
        ledgerline, // used, but not recorded. Defined if ledgerlines exist. Container: ledgerlines
        ornament,       // Container:chord. There are no cautionary ornaments
        // There is currently only one lyric class,
        // but there might be more later -- for different languages or verses etc. 
        lyric,   // Container:chord. There are no cautionaryLyrics
        dynamic,   // Container:chord. There are no cautionaryDynamics
        cautionaryBracket,  // Container:chord/cautionaryChord.
        noteExtender,       // Container:chord/cautionaryChord.
        #endregion chord components

        #region rest
        rest, // Container:voice
        #endregion rest

        #region beams
        beamBlock,      // Used, recorded, not defined. Container:voice
        beam,           // Used, not recorded. Defined if beamBlock exists. Container:beamBlock
        opaqueBeam,     // Used, not recorded. Defined if beamBlock exists. Container:beamBlock
        #endregion

        #region barlines
        normalBarline, // always used, recorded and defined. A normal barline and endBarline component. Container:voice.
        thinBarline, // a double-bar component. Always defined. Container:regionStartbarline etc.
        thickBarline, // a double-bar component. Always defined. Container:regionStartbarline etc.
        staffConnectors, // A group of barline (and maybe endBarline) in a system. Never defined. Container:system.
        startRegionBarline,
        endRegionBarline,
        endAndStartRegionBarline,
        endOfScoreBarline,
        #endregion barlines
    };

    /// <summary>
    /// This enum contains _all_ the colour classes that Moritz can write to SVG scores.
    /// These are used as subclasses of the CSSObjectClasses in SVG files, like this:
    ///     class="notehead fffColor"
    /// Note that Moritz does not use ffff although it is defined in CLicht.
    /// (These colours were found using a separate, self-written program.)
    /// </summary>
    public enum CSSColorClass
    {
        none, // no colour (i.e. transparent)
        black,
        white,
        red,
        #region Tombeau 1 notehead subclasses
        [Description("#FF0000")]
        fffColor,
        [Description("#D50055")]
        ffColor,
        [Description("#B5007E")]
        fColor,
        [Description("#8800B5")]
        mfColor,
        [Description("#0000CA")]
        mpColor,
        [Description("#0069A8")]
        pColor,
        [Description("#008474")]
        ppColor,
        [Description("#009F28")]
        pppColor,
        [Description("#00CA00")]
        ppppColor
        #endregion Tombeau 1 notehead subclasses
    };

    public enum CSSLineCap { butt, round, square };
}