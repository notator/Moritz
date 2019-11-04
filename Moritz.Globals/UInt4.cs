using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace Moritz.Globals
{
	//
	// Summary:
	//     Represents an unsigned integer (limited to range 0..11)
	// This is a copy of Moritz.Globals.UInt7 with UInt7 changed to UInt4, and MaxValue set to 11.
	public struct UInt4 : IComparable, IFormattable, IComparable<int>, IEquatable<int>
	{
		//
		// Summary:
		//     Represents the largest possible value of Moritz.Globals.UInt4. This field is constant.
		public const int MaxValue = 11;
		//
		// Summary:
		//     Represents the smallest possible value of Moritz.Globals.UInt4. This field is constant.
		public const int MinValue = 0;

		public int Int { get; }

		/// <summary>
		/// An exception is thrown if argument value is less than MinValue or greater than MaxValue.
		/// </summary>
		/// <param name="value"></param>
		public UInt4(int value)
		{
			if( value < MinValue || value > MaxValue)
			{
				throw new ArgumentOutOfRangeException();
			}
			Int = value;
		}

		public static explicit operator int(UInt4 v) => v.Int;
		public static explicit operator UInt4(int value) => new UInt4(value);

		public static UInt4 operator +(UInt4 a, UInt4 b) => new UInt4(a.Int + b.Int);
		public static UInt4 operator *(UInt4 a, UInt4 b) => new UInt4(a.Int * b.Int);
		public static UInt4 operator %(UInt4 a, UInt4 b) => new UInt4(a.Int % b.Int);
		public static bool operator ==(UInt4 a, UInt4 b) => a.Int == b.Int;
		public static bool operator !=(UInt4 a, UInt4 b) => a.Int != b.Int;
		public static bool operator >=(UInt4 a, UInt4 b) => a.Int >= b.Int;
		public static bool operator <=(UInt4 a, UInt4 b) => a.Int <= b.Int;
		public static bool operator >(UInt4 a, UInt4 b) => a.Int > b.Int;
		public static bool operator <(UInt4 a, UInt4 b) => a.Int < b.Int;

		//
		// Summary:
		//     Returns a value indicating whether this instance is equal to a specified Moritz.Globals.UInt4
		//     value.
		//
		// Parameters:
		//   obj:
		//     An object to compare to this instance.
		//
		// Returns:
		//     true if value is an int that has the same value as this instance; otherwise, false.
		public bool Equals(UInt4 other) => other.Int == Int;
		//
		// Summary:
		//     Returns a value indicating whether this instance is equal to a specified int
		//     value.
		//
		// Parameters:
		//   value:
		//     An int to compare to this instance.
		//
		// Returns:
		//     true if obj has the same value as this instance; otherwise, false.
		public bool Equals(int value) => value == Int;
		//
		// Summary:
		//     Returns the hash code for this instance.
		//
		// Returns:
		//     A 32-bit signed integer hash code.
		public override int GetHashCode() => base.GetHashCode();

		//
		// Summary:
		//     Compares this instance to a specified 32-bit signed integer and returns an
		//     indication of their relative values.
		// Parameters:
		//   value:
		//     An integer to compare.
		// Returns:
		//     A signed number indicating the relative values of this instance and value.
		//     Return value Description:
		//          Less than zero: This instance is less than value.
		//          Zero: This instance is equal to value.
		//          Greater than zero: This instance is greater than value.
		public int CompareTo(int other) => Int.CompareTo(other);
		//
		// Summary:
		//     Compares this instance to a specified object and returns an indication of their
		//     relative values.
		//
		// Parameters:
		//   value:
		//     An object to compare, or null.
		//
		// Returns:
		//     A signed number indicating the relative values of this instance and value.Return
		//     Value Description Less than zero This instance is less than value. Zero This
		//     instance is equal to value. Greater than zero This instance is greater than value.-or-
		//     value is null.
		//
		// Exceptions:
		//   T:System.ArgumentException:
		//     value is not a Moritz.Globals.UInt4.
		public int CompareTo(object obj)
		{
			if(! (obj is UInt4))
			{
				throw new ArgumentException();
			}
			else
			{
				int argInt = ((UInt4)obj).Int;
				return Int.CompareTo(argInt);
			}
		}

		//
		// Summary:
		//     Converts the string representation of a number in a specified style and culture-specific
		//     format to its 7-bit unsigned integer equivalent.
		//
		// Parameters:
		//   s:
		//     A string that represents the number to convert. The string is interpreted by
		//     using the style specified by the style parameter.
		//
		//   style:
		//     A bitwise combination of enumeration values that indicate the style elements
		//     that can be present in s. A typical value to specify is System.Globalization.NumberStyles.Integer.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information about s.
		//
		// Returns:
		//     A 7-bit unsigned integer equivalent to the number specified in s.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     s is null.
		//
		//   T:System.ArgumentException:
		//     style is not a System.Globalization.NumberStyles value. -or-style is not a combination
		//     of System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber
		//     values.
		//
		//   T:System.FormatException:
		//     s is not in a format compliant with style.
		//
		//   T:System.OverflowException:
		//     s represents a number that is less than Moritz.Globals.UInt4.MinValue or greater than
		//     Moritz.Globals.UInt4.MaxValue. -or-s includes non-zero, fractional digits.
		public static UInt4 Parse(string s, NumberStyles style, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		//
		// Summary:
		//     Converts the string representation of a number in a specified culture-specific
		//     format to its 7-bit unsigned integer equivalent.
		//
		// Parameters:
		//   s:
		//     A string that represents the number to convert.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information about s.
		//
		// Returns:
		//     A 7-bit unsigned integer equivalent to the number specified in s.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     s is null.
		//
		//   T:System.FormatException:
		//     s is not in the correct format.
		//
		//   T:System.OverflowException:
		//     s represents a number less than Moritz.Globals.UInt4.MinValue or greater than Moritz.Globals.UInt4.MaxValue.
		public static UInt4 Parse(string s, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}



		//
		// Summary:
		//     Converts the string representation of a number to its 7-bit unsigned integer
		//     equivalent.
		//
		// Parameters:
		//   s:
		//     A string that represents the number to convert.
		//
		// Returns:
		//     A 7-bit unsigned integer equivalent to the number contained in s.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     s is null.
		//
		//   T:System.FormatException:
		//     s is not in the correct format.
		//
		//   T:System.OverflowException:
		//     s represents a number less than Moritz.Globals.UInt4.MinValue or greater than Moritz.Globals.UInt4.MaxValue.
		public static UInt4 Parse(string s)
		{
			throw new NotImplementedException();
		}
		//
		// Summary:
		//     Converts the string representation of a number in a specified style to its 7-bit
		//     unsigned integer equivalent.
		//
		// Parameters:
		//   s:
		//     A string that represents the number to convert. The string is interpreted by
		//     using the style specified by the style parameter.
		//
		//   style:
		//     A bitwise combination of the enumeration values that specify the permitted format
		//     of s. A typical value to specify is System.Globalization.NumberStyles.Integer.
		//
		// Returns:
		//     A 7-bit unsigned integer equivalent to the number specified in s.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     s is null.
		//
		//   T:System.ArgumentException:
		//     style is not a System.Globalization.NumberStyles value. -or-style is not a combination
		//     of System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber
		//     values.
		//
		//   T:System.FormatException:
		//     s is not in a format compliant with style.
		//
		//   T:System.OverflowException:
		//     s represents a number less than Moritz.Globals.UInt4.MinValue or greater than Moritz.Globals.UInt4.MaxValue.
		//     -or-s includes non-zero, fractional digits.
		
		public static UInt4 Parse(string s, NumberStyles style)
		{
			throw new NotImplementedException();
		}
		//
		// Summary:
		//     Tries to convert the string representation of a number to its 7-bit unsigned
		//     integer equivalent. A return value indicates whether the conversion succeeded
		//     or failed.
		//
		// Parameters:
		//   s:
		//     A string that represents the number to convert.
		//
		//   result:
		//     When this method returns, contains the 7-bit unsigned integer value that is
		//     equivalent to the number contained in s, if the conversion succeeded, or zero
		//     if the conversion failed. The conversion fails if the s parameter is null or
		//     System.String.Empty, is not in the correct format. , or represents a number less
		//     than Moritz.Globals.UInt4.MinValue or greater than Moritz.Globals.UInt4.MaxValue. This parameter
		//     is passed uninitialized; any value originally supplied in result will be overwritten.
		//
		// Returns:
		//     true if s was converted successfully; otherwise, false.
		public static bool TryParse(string s, out UInt4 result)
		{
			throw new NotImplementedException();
		}
		//
		// Summary:
		//     Tries to convert the string representation of a number in a specified style and
		//     culture-specific format to its 7-bit unsigned integer equivalent. A return value
		//     indicates whether the conversion succeeded or failed.
		//
		// Parameters:
		//   s:
		//     A string that represents the number to convert. The string is interpreted by
		//     using the style specified by the style parameter.
		//
		//   style:
		//     A bitwise combination of enumeration values that indicates the permitted format
		//     of s. A typical value to specify is System.Globalization.NumberStyles.Integer.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information about s.
		//
		//   result:
		//     When this method returns, contains the 7-bit unsigned integer value equivalent
		//     to the number contained in s, if the conversion succeeded, or zero if the conversion
		//     failed. The conversion fails if the s parameter is null or System.String.Empty,
		//     is not in a format compliant with style, or represents a number less than Moritz.Globals.UInt4.MinValue
		//     or greater than Moritz.Globals.UInt4.MaxValue. This parameter is passed uninitialized;
		//     any value originally supplied in result will be overwritten.
		//
		// Returns:
		//     true if s was converted successfully; otherwise, false.
		//
		// Exceptions:
		//   T:System.ArgumentException:
		//     style is not a System.Globalization.NumberStyles value. -or-style is not a combination
		//     of System.Globalization.NumberStyles.AllowHexSpecifier and System.Globalization.NumberStyles.HexNumber
		//     values.
		public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UInt4 result)
		{
			throw new NotImplementedException();
		}

		//
		// Summary:
		//     Converts the numeric value of this instance to its equivalent string representation.
		//
		// Returns:
		//     The string representation of the value of this instance, which consists of a
		//     sequence of digits ranging from 0 to 9, without a sign or leading zeros.
		[SecuritySafeCritical]
		public override string ToString()
		{
			return Int.ToString();
		}
		//
		// Summary:
		//     Converts the numeric value of this instance to its equivalent string representation
		//     using the specified format.
		//
		// Parameters:
		//   format:
		//     A numeric format string.
		//
		// Returns:
		//     The string representation of the value of this instance as specified by format.
		//
		// Exceptions:
		//   T:System.FormatException:
		//     The format parameter is invalid.
		[SecuritySafeCritical]
		public string ToString(string format)
		{
			throw new NotImplementedException();
		}
		//
		// Summary:
		//     Converts the numeric value of this instance to its equivalent string representation
		//     using the specified format and culture-specific format information.
		//
		// Parameters:
		//   format:
		//     A numeric format string.
		//
		//   provider:
		//     An object that supplies culture-specific formatting information.
		//
		// Returns:
		//     The string representation of the value of this instance, as specified by format
		//     and provider.
		//
		// Exceptions:
		//   T:System.FormatException:
		//     format is invalid.
		[SecuritySafeCritical]
		public string ToString(string format, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}
		//
		// Summary:
		//     Converts the numeric value of this instance to its equivalent string representation
		//     using the specified culture-specific format information.
		//
		// Parameters:
		//   provider:
		//     An object that supplies culture-specific formatting information.
		//
		// Returns:
		//     The string representation of the value of this instance, which consists of a
		//     sequence of digits ranging from 0 to 9, without a sign or leading zeros.
		[SecuritySafeCritical]
		public string ToString(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		bool IEquatable<int>.Equals(int other)
		{
			throw new NotImplementedException();
		}
	}
}
