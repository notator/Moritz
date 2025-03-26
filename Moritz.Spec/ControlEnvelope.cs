using Krystals5ObjectLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moritz.Spec
{
        /// <summary>
        /// A ControlEnvelope is an Envelope with a defined midi control type
        /// </summary>
        internal class ControlEnvelope : Envelope
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="midiControlType">The control index for the messages in the envelope.</param>
            /// <param name="envDefinition">A list of values defining equidistant peaks and troughs in the envelope. The first and last values are the boundary values.</param>
            /// <param name="count">The maximum number of values</param>
            ControlEnvelope(int midiControlType, List<int> envDefinition )
                :base(envDefinition, 127, 127, count)
            {
                _midiControlType = midiControlType;
            }

            private readonly int _midiControlType;
        }
}
