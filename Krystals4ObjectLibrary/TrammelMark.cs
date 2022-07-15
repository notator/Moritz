using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krystals4ObjectLibrary
{
    public class TrammelMark
    {
        public TrammelMark(int value)
        {
            _value = value;
            _distancesIndex = value - 1;
        }
        #region Properties
        public uint Position { get { return _position; } set { _position = value; } }
        public uint Distance { get { return _distance; } set { _distance = value; } }
        public int DistancesIndex { get { return _distancesIndex; } }
        public int Value { get { return _value; } }
        public uint PositionKey { get { return (uint)(_position * 10000); } } // key in sorted expansion list
        #endregion Properties
        #region private variables
        private uint _position = 0; // the current position of this trammelMark in the distances expansion
        private uint _distance = 0; // the current distance to be added during expansion
        private readonly int _distancesIndex; // index in the distances array
        private readonly int _value; // this trammelMark's value
        #endregion private variables
    }
}
