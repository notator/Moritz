
namespace Moritz.Score
{
	public class Stem
	{
        public Stem(ChordSymbol chordSymbol)
        {
            Chord = chordSymbol;
            if(chordSymbol.DurationClass == DurationClass.breve
                || chordSymbol.DurationClass == DurationClass.semibreve
                || chordSymbol.DurationClass == DurationClass.minim
                || chordSymbol.DurationClass == DurationClass.crotchet)
                BeamContinues = false;
            else
                BeamContinues = true; // theoretically, I can reset this default value later (when breaking beams programmatically)
        }

        public Stem(ChordSymbol chordSymbol, Stem stem)
        {
            Chord = chordSymbol;
            if(stem != null)
            {
                Direction = stem.Direction;
            }
        }

		public readonly ChordSymbol Chord;

		public VerticalDir Direction = VerticalDir.none;
        public bool BeamContinues = true;
        public bool Draw = true; // set to false for cautionary chords
	}
}
