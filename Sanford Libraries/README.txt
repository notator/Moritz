
Multimedia.dll and Multimedia.Midi.dll
======================================

These .dll files come from Version 4 of Leslie Sanford's C# Midi Toolkit 

The licence in the source files reads:

/* Copyright (c) 2005 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 *
 * Leslie Sanford
 * Email: jabberdabber@hotmail.com
 */


The source code for Version 5 of the C# Midi Toolkit is available (February 2012) at

  http://www.codeproject.com/Articles/6228/C-MIDI-Toolkit

I have never had any serious problems using Version 4, and converting to Version 5 
would mean quite a lot of rather unproductive work. The source code for Version 4 
seems no longer to be available on-line, but I have a copy should it be needed.
 
NOTE: I have recompiled Mutimedia.Midi.dll after commenting out an AssertValid() 
call at the end of the following function:

  Multimedia.Midi.Sequencing.TrackClasses.Track.cs.Insert(int position, IMidiMessage message)

This call was originally removed on Leslie Sanford's advice in August 2007 to avoid
long waits while loading files.
While integrating these libraries into Moritz (23.11.2011), I discovered that
(without stopping or otherwise affecting the performance) this assertion could fail
while Moritz was playing very fast notes. The assertion simply seems not to matter, 
so I've commented it out again. Dangerous maybe, but effective! :-)

James Ingram
February 2012
