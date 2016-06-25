using System;
using System.Collections.Generic;
using System.Diagnostics;

using Krystals4ObjectLibrary;

using Moritz.Globals;
using Moritz.Palettes;
using Moritz.Spec;

namespace Moritz.Algorithm.Tombeau1
{
	public partial class Tombeau1Algorithm : CompositionAlgorithm
	{
        private class Tombeau1ReadonlyConstants
        {
            public Tombeau1ReadonlyConstants(List<List<MidiChordDef>> pitchWheelCoreMidiChordDefs,
                                    List<List<MidiChordDef>> ornamentCoreMidiChordDefs,
                                    List<List<MidiChordDef>> paletteMidiChordDefs)
            {
                _pitchWheelCoreMidiChordDefs = pitchWheelCoreMidiChordDefs;
                _ornamentCoreMidiChordDefs = ornamentCoreMidiChordDefs;
                _paletteMidiChordDefs = paletteMidiChordDefs;
            }

            public static List<List<MidiChordDef>> PitchWheelCoreMidiChordDefs
            {
                get
                {
                    return DeepCloneOf(_pitchWheelCoreMidiChordDefs);
                }
            }
            public static List<List<MidiChordDef>> OrnamentCoreMidiChordDefs
            {
                get
                {
                    return DeepCloneOf(_ornamentCoreMidiChordDefs);
                }
            }
            public static List<List<MidiChordDef>> PaletteMidiChordDefs
            {
                get
                {
                    return DeepCloneOf(_paletteMidiChordDefs);
                }
            }
            private static List<List<T>> DeepCloneOf<T>(List<List<T>> original)
            {
                List<List<T>> rval = new List<List<T>>();
                foreach(List<T> mcdList in original)
                {
                    List<T> newMcdList = new List<T>();
                    rval.Add(newMcdList);
                    foreach(T t in mcdList)
                    {
                        ICloneable c = t as ICloneable;
                        if(c != null)
                        {
                            newMcdList.Add((T) c.Clone());
                        }
                    }
                }
                return rval;
            }

            private static List<List<MidiChordDef>> _pitchWheelCoreMidiChordDefs = new List<List<MidiChordDef>>();
            private static List<List<MidiChordDef>> _ornamentCoreMidiChordDefs = new List<List<MidiChordDef>>();
            private static List<List<MidiChordDef>> _paletteMidiChordDefs = new List<List<MidiChordDef>>();

        }       
    }
}
