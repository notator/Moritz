using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

using Multimedia.Midi;

using Moritz.Globals;
using Moritz.Score.Midi;

namespace Moritz.Score.Notation
{
    ///<summary>
    /// A UniqueRestDef is a unique RestDef which is saved locally in an SVG file.
    /// Each rest in an SVG file has an ID of the form "rest"+uniqueNumber.
    /// UniqueRestDefs created programmatically have a null ID.
    ///<summary>
    public class UniqueRestDef : RestDef, IUniqueDef
    {
        public UniqueRestDef(RestDef midiRestDef)
            :base(midiRestDef.MsDuration)
        {
            ID = null;
            MsPosition = 0;
        }

        public UniqueRestDef(int msPosition, int msDuration)
            : base(msDuration)
        {
            ID = null;
            MsPosition = msPosition;
        }

        /// <summary>
        /// Used while loading a score
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msDuration"></param>
        public UniqueRestDef(string id, int msDuration)
            :base(0)
        {
            ID = id;
            MsDuration = msDuration;
            MsPosition = 0; // This value will probably be set later.

            //// The reader is at the beginning of a "score:rest" element having an ID attribute
            //Debug.Assert(r.Name == "score:rest" && r.IsStartElement() && r.AttributeCount > 0);
            //int nAttributes = r.AttributeCount;
            //for(int i = 0; i < nAttributes; i++)
            //{
            //    r.MoveToAttribute(i);
            //    switch(r.Name)
            //    {
            //        case "id":
            //            ID = r.Value;
            //            break;
            //        case "msDuration":
            //            _msDuration = int.Parse(r.Value);
            //            break;
            //    }
            //}
        }

        #region IUniqueDef
        public override string ToString()
        {
            return ("MsPosition=" + MsPosition.ToString() + " MsDuration=" + MsDuration.ToString() + " UniqueRestDef" );
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustDuration(double factor)
        {
            MsDuration = (int)(_msDuration * factor);
        }

        public readonly string ID;

        // MsDuration is inherited from RestDef whose _msDuration is immutable
        public override int MsDuration { get { return _msDuration; } set { _msDuration = value; } }
        private int _msDuration = 0;
        public int MsPosition { get { return _msPosition; } set { _msPosition = value; } }
        private int _msPosition = 0;
        #endregion IUniqueDef
    }
}
