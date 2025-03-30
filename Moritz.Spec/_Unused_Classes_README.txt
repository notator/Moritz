_Unused_Classes_README.txt

The classes Moritz.Spec.Mode.cs and Moritz.Spec.FortePitchClassSet.cs were created while composing an early version
of Tombeau 1, but they are currently unused in this repository branch.

The Mode class is no longer used anywhere because:
All references by Tombeau 1 and its related classes to Mode have been removed.
The Mode class is also also referenced by the following files in Moritz.Spec:
		MidiChordDef.cs
		BasicMidiChordDef.cs
		ModeProximity.cs
But those references are themselves not used anywhere.

The FortePitchClassSet class is no longer used anywhere because:
It was referenced only by Tombeau 1, and those references have been deleted.

I've come to the conclusion that its not a good idea to use FortePitchClassSets as a compositional strategy.
Compositions are about relations bewteen _particular_ pitch sets in a _particular_ composition, not about pitch sets _in_general_.
A composition has to create relations that are perceptible _in_a_particular_context_, so the context can't be ignored by the
analysis. Composing is the creation and clarification of a particular _context_.

Modes may come in useful later. But see the comments in Tombeau1Algorithm.cs.