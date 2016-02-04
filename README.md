Moritz
======

[Moritz](http://james-ingram-act-two.de/moritz3/moritz3.html) was named after Max's terrible twin in Wilhelm Busch's [_Max and Moritz_](http://en.wikipedia.org/wiki/Max_and_Moritz).<br />
[Max](https://cycling74.com/) is a computer program that specializes in controlling information (sounds) at the MIDI event level and below. Moritz deals with the MIDI event level and above (musical form). MIDI events are the common interface (at the chord symbol level) between these levels of information.<br />
Moritz is a Visual Studio (Community 2015 edition) solution, written in C#, containing two related Windows desktop programs that I use for creating scores that can be read and performed on the web:
  * [Krystals 4.0](http://james-ingram-act-two.de/krystals/krystals4.html)
  * [Assistant Composer](http://james-ingram-act-two.de/moritz3/assistantComposer/assistantComposer.html)

A [Krystal](http://james-ingram-act-two.de/krystals/krystalsIntro.html) is an [_Abstract Data Type_](http://en.wikipedia.org/wiki/Abstract_data_type) that I use for organizing large scale data structures. The Krystals 4.0 program, which is used to create krystals, has not changed since the last version of Moritz, but the type is used by the Assistant Composer, so needs to be kept here.<br />
The Assistant Composer generates scores, written in SVG, that contain embedded MIDI information (see [SVG-MIDI file format](http://james-ingram-act-two.de/open-source/svgScoreExtensions.html)). Such scores are designed to be playable by my Assistant Performer software [1].<br />
The Assistant Composer underwent a major update/overhaul in the autumn of 2014, and the code should now be much easier to understand. (My coding style is a bit pedestrian by present day C# standards, but maybe that's not such a bad thing.)<br />
The biggest change, apart from cleaning up the code, is that scores can now contain both _input_ and _output chords_. This enables much greater control over what happens when midi input information arrives during a live performance: Parallel processing can be used to enable a non-blocking, "advanced prepared piano" scenario. Single key presses can trigger either simple events or complex sequences of events, depending on how the links inside the score are organized.

Anyone is welcome to dive into and use any of my code in any way they like, but it would probably be better to contact me if you want to do that. Moritz is rather large, and the code is by nature rather complicated. There are, however, some parts of it that could easily be isolated and used for more general purposes. For example, the code for writing chord symbols (using the CLicht music font's metrics) could be used in any standard music notation program. Or, the SVG-MIDI file format and the code for spacing standard chord symbols across systems could easily be adapted for automatically notating Standard MIDI files...

February 2016<br />
James Ingram

[1] The Assistant Performer is no longer part of Moritz. It is now a separate, open-source web application, written in Javascript. Links: [About the Assistant Performer](http://james-ingram-act-two.de/open-source/aboutAssistantPerformer.html), [on the web](http://james-ingram-act-two.de/open-source/assistantPerformer/assistantPerformer.html), [on GitHub](https://github.com/notator/assistant-performer).
