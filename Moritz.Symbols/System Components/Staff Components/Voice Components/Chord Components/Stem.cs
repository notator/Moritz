
using Moritz.Xml;

namespace Moritz.Symbols
{
    public class Stem
    {
        public Stem(ChordSymbol chordSymbol, bool beamContinues)
        {
            Chord = chordSymbol;
            if(chordSymbol.DurationClass == DurationClass.breve
                || chordSymbol.DurationClass == DurationClass.semibreve
                || chordSymbol.DurationClass == DurationClass.minim
                || chordSymbol.DurationClass == DurationClass.crotchet)
                BeamContinues = false;
            else
                BeamContinues = beamContinues;
        }

        public readonly ChordSymbol Chord;

        public VerticalDir Direction = VerticalDir.none;
        public bool BeamContinues = true;
        public bool Draw = true; // set to false for cautionary chords
    }
}
