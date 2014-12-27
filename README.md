Moritz
======

Moritz was named after Max’s terrible twin (see Max and Moritz): Max is a program which specializes in controlling information (sounds) at the MIDI event level and below. Moritz deals with the MIDI event level and above (musical form). MIDI events are the common interface (at the chord symbol level) between these levels of information.

Moritz is in fact a wrapper for two related Windows desktop programs, written in C#, that I use for creating scores that can be performed by anyone on the web using Assistant Performer software (see below).

There is a top-level description of the current version of Moritz at:
  * http://james-ingram-act-two.de/moritz3/moritz3.html

The two embedded programs are:
  * Krystals 4.0 (http://james-ingram-act-two.de/krystals/krystals4.html)
  * Assistant Composer

The Assistant Composer generates scores written in SVG containing embedded MIDI information. A full description will be appearing in January 2015 at:
  * http://james-ingram-act-two.de/moritz3/assistantComposer/assistantComposer.html

Moritz has undergone a major update/overhaul this autumn (2014), so the code should now be much easier to understand. My coding style is a bit pedestrian by present day C# standards, but maybe that's not such a bad thing. :-)
The biggest change is that the Assistant Composer now writes scores containing both input and output chords. This enables much greater control over what happens when midi input information arrives during a live performance: Parallel processing can be used to enable a non-blocking, "advanced prepared piano" scenario. Single key presses can trigger either simple events or complex sequences of events, depending on how the score is organized.

The SVG-MIDI format is currently documented at:
  * http://james-ingram-act-two.de/open-source/svgScoreExtensions.html

A proper XML-Schema should be available soon.
The Assistant Performer is itself no longer part of Moritz. It is now written in Javascript, and is kept in the following GitHub repository:
  * https://github.com/notator/assistant-performer

The Assistant Performer is due to be updated for the new file format during the coming months.

Anyone is welcome to dive into and use any of my code in any way they like, but it would probably be better to contact me if you want to do that with the Assistant Composer. The code is by nature rather complicated. There are, however, some parts of it that could easily be isolated and used for more general purposes. For example, if one accepts that tuplets should not exist in a modern music notation, the SVG-MIDI file format and the code for spacing standard chord symbols across systems could easily be adapted for use in the automatic, lossless transcription of Standard MIDI files. My SVG-MIDI scores are in fact just such transcriptions...

December 2014
James Ingram
