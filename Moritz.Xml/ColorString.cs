using Moritz.Globals;

using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace Moritz.Xml
{
    public class ColorString
    {
        /// <summary>
        /// ColorString is a string of 6 hexadecimal digits.
        /// </summary>
        /// <param name="r"></param>
        public ColorString(string colorString)
        {
            StringToComponents(colorString);
        }
        /// <summary>
        /// ColorString is a string of 6 hexadecimal digits.
        /// </summary>
        /// <param name="r"></param>
        public ColorString(Color color)
        {
            if(!color.IsEmpty)
            {
                _redComponent = color.R;
                _greenComponent = color.G;
                _blueComponent = color.B;
            }
        }

        public int RedComponent
        {
            get { return _redComponent; }
            set
            {
                Debug.Assert(value >= 0 && value <= 0xFF);
                _redComponent = value;
            }
        }
        public int GreenComponent
        {
            get { return _greenComponent; }
            set
            {
                Debug.Assert(value >= 0 && value <= 0xFF);
                _greenComponent = value;
            }
        }
        public int BlueComponent
        {
            get { return _blueComponent; }
            set
            {
                Debug.Assert(value >= 0 && value <= 0xFF);
                _blueComponent = value;
            }
        }

        public Color Color
        {
            get { return Color.FromArgb(_redComponent, _greenComponent, _blueComponent); }
        }
        /// <summary>
        /// Gets or sets a string containing a "#" followed by 6 hexadecimal digits.
        /// </summary>
        public string String
        {
            get
            {
                StringBuilder returnSB = new StringBuilder();
                string digitString;
                string[] letters = { "A", "B", "C", "D", "E", "F" };

                int combinedValue = (_redComponent << 16) + (_greenComponent << 8) + _blueComponent;
                for(int i = 0; i < 6; i++)
                {
                    int digit = combinedValue & 0xF;
                    if(digit <= 9)
                        digitString = digit.ToString();
                    else
                        digitString = letters[digit - 10];
                    returnSB.Insert(0, digitString);
                    combinedValue >>= 4;
                }
                return "#" + returnSB.ToString();
            }
            set
            {
                string colorString = value;
                if(colorString.Length > 1 && colorString[0] == '#')
                    colorString = colorString.Substring(1);

                StringToComponents(colorString);
            }
        }

        /// <summary>
        /// colorString must be a string of exactly 6 hexadecimal digits.
        /// </summary>
        /// <param name="colorString"></param>
        private void StringToComponents(string colorString)
        {
            Debug.Assert(Regex.IsMatch(colorString, @"^[0-9a-fA-F]{6}$"));
            int[] ints = new int[6];
            for(int i = 0; i < 6; i++)
            {
                switch(colorString[i])
                {
                    case 'F':
                    case 'f':
                        ints[5 - i] = 15;
                        break;
                    case 'E':
                    case 'e':
                        ints[5 - i] = 14;
                        break;
                    case 'D':
                    case 'd':
                        ints[5 - i] = 13;
                        break;
                    case 'C':
                    case 'c':
                        ints[5 - i] = 12;
                        break;
                    case 'B':
                    case 'b':
                        ints[5 - i] = 11;
                        break;
                    case 'A':
                    case 'a':
                        ints[5 - i] = 10;
                        break;
                    case '9':
                        ints[5 - i] = 9;
                        break;
                    case '8':
                        ints[5 - i] = 8;
                        break;
                    case '7':
                        ints[5 - i] = 7;
                        break;
                    case '6':
                        ints[5 - i] = 6;
                        break;
                    case '5':
                        ints[5 - i] = 5;
                        break;
                    case '4':
                        ints[5 - i] = 4;
                        break;
                    case '3':
                        ints[5 - i] = 3;
                        break;
                    case '2':
                        ints[5 - i] = 2;
                        break;
                    case '1':
                        ints[5 - i] = 1;
                        break;
                    case '0':
                        ints[5 - i] = 0;
                        break;
                }
            }
            int factor = 1;
            for(int i = 0; i < 2; i++)
            {
                _redComponent += ints[i + 4] * factor;
                _greenComponent += ints[i + 2] * factor;
                _blueComponent += ints[i] * factor;
                factor *= 16;
            }
        }

        public bool IsBlack
        {
            get
            {
                return (_redComponent == 0 && _greenComponent == 0 && _blueComponent == 0);
            }
        }

        private int _redComponent = 0;
        private int _greenComponent = 0;
        private int _blueComponent = 0;
    }
}
