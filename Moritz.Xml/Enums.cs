
using System.ComponentModel;

namespace Moritz.Xml
{
    /// <summary>
    /// Can be used to construct a LogicalWidth.
    /// Not used while reading or writing XML.
    /// </summary>
    public enum DurationClass
    {
        none = 0,
        [Description("cautionary")]
        cautionary = 1,
        [Description("1/128")]
        fiveFlags = 2,
        [Description("1/64")]
        fourFlags = 3,
        [Description("1/32")]
        threeFlags = 4,
        [Description("1/16")]
        semiquaver = 5,
        [Description("1/8")]
        quaver = 6,
        [Description("1/4")]
        crotchet = 7,
        [Description("1/2")]
        minim = 8,
        [Description("1/1")]
        semibreve = 9,
        [Description("2/1")]
        breve = 10
    };
    /// <summary>
    /// Attribute in (layout.distances) "systems"
    /// CapXML Attribute name: "pageJustified"
    /// When writing CapXML files, retrieve the field name as a string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum PageJustified { none, exceptLast, all };
    /// <summary>
    /// Attribute of all DrawObjects. All DrawObjects can be anchored to chords/rests/explicitBarlines.
    /// If the object is anchored to a page, this attribute is ignored.
    /// CapXML Attribute name: "horizAlign"
    /// CapXML strings are:
    /// "0" : head, 
    /// "1" : stem.
    /// When writing CapXML files, retrieve the Description attribute string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum ChordHorizAlign
    {
        [Description("0")] head,
        [Description("1")] stem
    };
    /// <summary>
    /// Attribute of all DrawObjects. All DrawObjects can be anchored to chords/rests.
    /// If the object is anchored to a page, this attribute is ignored.
    /// CapXML Attribute name: "vertAlign"
    ///  CapXML strings are: 
    ///  "0" : stafflines, 
    ///  "1" : outer note (applies to chords with several notes: head opposite to the stem),
    ///  "2" : inner note (applies to chords with several notes: first head from stem end), 
    ///  "3" : stem end,
    ///  "4" : top,
    ///  "5" : bottom
    /// When writing CapXML files, retrieve the Description attribute string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum VertAlign
    {
        [Description("0")]
        stafflines,
        [Description("1")]
        outerNote,
        [Description("2")]
        innerNote,
        [Description("3")]
        stemEnd,
        [Description("4")]
        top,
        [Description("5")]
        bottom
    };
    /// <summary>
    /// CapXML-1.0.8 Type.
    /// CapXML-1.0.8 Type name: "HorizAlign"
    /// Attribute of CapXML elements "verse", "instrumentNames", "text"
    /// When writing CapXML files, retrieve the field name as a string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum TextHorizAlign { left, center, right };
    /// <summary>
    /// CapXML-2.0 Type.
    /// CapXML-2.0 Type name: "FrameType"
    /// When writing CapXML files, retrieve the field name as a string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum TextFrameType { none, rectangle, ellipse, circle };
    /// <summary>
	/// CapXML-1.0.8 Type.
	/// CapXML-1.0.8 Type name: "VerticalDir"
	/// Attribute of CapXML elements "voice", (chord and rest)"display", "tupletBracket", "stem"
	/// When writing CapXML files, retrieve the field name as a string using
	///		M.GetEnumDescription()
	/// </summary>
	public enum VerticalDir { none, up, down };
    /// <summary>
    /// The meanings are:
    ///    full: (capella's default) barlines are drawn from the top ("to") staffline down to the top ("to")
    ///          staffline of the next staff below, or to the bottom ("from") staffline, if there is no staff below.
    ///    none: no barlines are drawn (but notes are spaced horizontally as if they are there)
    ///    internal: barlines are drawn up from the bottom ("from") staffline to the top ("to") staffline.
    ///    external: barlines are drawn down from the bottom ("from") barline to the top ("to") staffline
    ///          of the staff below (if there is one).
    /// </summary>
    /// <remarks>
    /// Attribute in CapXML element "barlines"
    /// Attribute name: "mode"
    /// When writing CapXML files, retrieve the field name or Description attribute
    /// as a string using
    ///		M.GetEnumDescription()
    /// </remarks>
    public enum BarlinesMode
    {
        full,
        none,
        [Description("internal")]
        internal_,
        external
    };
    /// <summary>
    /// Attribute in CapXML element (layout.staves.staffLayout)"distances"
    /// Attribute name: "lineDist"
    /// When writing CapXML files, retrieve the field name as a string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum LineDist { normal, small };
    /// <summary>
    /// Attribute in CapXML element "barcount"
    /// Attribute name: "frame"	(Determines how the bar numbers are framed.)
    /// CapXML values:
    ///  "0" : no frame
    ///  "1" : rectangular frame
    ///  "2" : oval frame
    /// When writing CapXML files, retrieve the Description attribute string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum BarCountFrame
    {
        [Description("0")]
        noFrame,
        [Description("1")]
        rectangularFrame,
        [Description("2")]
        ovalFrame
    };
    /// <summary>
    /// *** New in capella 2008:
    /// Attribute in CapXML element "barcount"
    /// Attribute name: "fillOpacity" (Opacity of the fill color for bar-number frames.)
    /// CapXML uses only the following values:
    ///  "255" : opaque
    ///  "128" : semiTransparent
    /// When writing CapXML files, retrieve the Description attribute string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum FillOpacity
    {
        [Description("255")]
        opaque = 255,
        [Description("128")]
        semiTransparent = 128
    };
    #region enums which only occur in systems
    /// <summary>
    /// Attribute of "system". Specifies if and how instrument names should be displayed.
    /// Attribute name: "instrNotation"
    /// CapXML values:
    ///   "none" : no name
    ///   "short" : short name
    ///   "long" : long name
    /// When writing CapXML files, retrieve the Description attribute string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum InstrNotation
    {
        [Description("none")]
        none,
        [Description("short")]
        short_,
        [Description("long")]
        long_
    };
    /// <summary>
    /// Attribute of "system". Specifies the default beam display characteristics for this system. Can be
    /// overridden by setting chord or rest BeamAttributes (see below).
    /// Attribute name: "beamGrouping"
    /// CapXML values:
    ///  "0" : flags only, 
    ///  "1" : small beam groups
    ///  "2" : medium
    ///  "3" : large
    ///  "4" : whole bar beams
    /// When writing CapXML files, retrieve the Description attribute string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum SystemBeamGrouping
    {
        [Description("0")]
        flagsOnly,
        [Description("1")]
        small,
        [Description("2")]
        medium,
        [Description("3")]
        large,
        [Description("4")]
        wholeBars
    };
    #endregion
    #region enums which only occur in chords
    /// <summary>
    /// Attribute of (chord)"beam". Specifies if and how the chord displays beams.
    /// Attribute name: "group"
    /// CapXML values:
    ///   "auto" : auto (automatic beaming, according to the system.beamGrouping - see above)
    ///   "force" : force (all beams continue from this chord)
    ///   "split" : stopAll (all beams stop at this chord)
    ///   "divide" : stopAllButOne (all but one beams stop at this chord).
    /// When writing CapXML files, retrieve the Description attribute string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum BeamContinuation
    {
        [Description("long")]
        auto,
        [Description("force")]
        force,
        [Description("split")]
        stopAll,
        [Description("divide")]
        stopAllButOne
    };

    /// <summary>
    /// Attribute of (chord)"articulation". Specifies the chord's articulations.
    /// Attribute name: "type"
    /// The CapXML value is a space-separated list containing the following values:
    ///   "staccato"
    ///   "tenuto"
    ///   "staccatissimo"
    ///   "normalAccent"
    ///   "strongAccent"
    ///   "weakBeat"
    ///   "strongBeat"
    /// Currently (CapXML-1.0.8), the only multiple value is "staccato tenuto"
    /// When writing CapXML files, retrieve the field value as a string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum Articulation { staccato, tenuto, staccatissimo, normalAccent, strongAccent, weakBeat, strongBeat };

    /// <summary>
    /// Attribute of (head.alter)"display". Specifies if and how an accidental should be displayed.
    /// CapXML values:
    ///   "auto" : automatically
    ///   "suppress" : don't display an accidental
    ///   "force" : always display an accidental
    ///   "parenth" : show accidental in parentheses
    /// When writing CapXML files, retrieve the field value as a string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum DisplayAccidental { auto, suppress, force, parenth };
    /// <summary>
    /// Attribute of "head". Specifies the shape of a notehead.
    /// CapXML values:
    ///   "auto" : default
    ///   "diamond" : diamond
    ///   "crossCircle" : crossCircle
    ///   "crossDiamond" : crossDiamond
    ///   "triangle" : triangle
    ///   "square" : square
    ///   -- and new values for CapXML2, followed by
    ///   "none" : none
    /// When writing CapXML files, retrieve the field value as a string using
    ///		M.GetEnumDescription()
    /// </summary>
    public enum HeadShape
    {
        auto, diamond, crossCircle, crossDiamond, triangle, square,
        // begin CapXML2
        openDiamond, crossCircle2, openTriangle, openSquare, thickCross,
        thickCrossCircle, slash, filledNote, filledDiamond, filledTriangle, filledSquare,
        // end CapXML2
        none
    };

    #endregion
}
