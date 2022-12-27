using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEditBoxWPF.TextStructure
{
	/// <summary>
	/// Represents the position of a character within a <see cref="TEditBox"/>.
	/// </summary>
	public struct TIndex
	{
		/// <summary>
		/// Gets an index starting at line 0 and character 0.
		/// </summary>
		public static TIndex Start => new TIndex(0, 0);

		/// <summary>
		/// The line of this index.
		/// </summary>
		public int Line;

		/// <summary>
		/// The character of this index.
		/// </summary>
		public int Character;

		public TIndex(int line, int character)
		{
			Line = line;
			Character = character;
		}

		/// <summary>
		/// Offsets this index by a certain amount. This does not
		/// modify the original index.
		/// </summary>
		/// <param name="offsetIndex">The index to offset this index by.</param>
		/// <returns>A new offset index.</returns>
		public TIndex Offset(TIndex offsetIndex) => Offset(offsetIndex.Line, offsetIndex.Character);

		/// <summary>
		/// Offsets this index by a certain amount. This does not
		/// modify the original index.
		/// </summary>
		/// <param name="lineOffset">The number to offset the <see cref="Line"/> of this index by.</param>
		/// <param name="characterOffset">The number to offset the <see cref="Character"/> of this index by.</param>
		/// <returns>A new offset index.</returns>
		public TIndex Offset(int lineOffset, int characterOffset) => new(Line + lineOffset, Character + characterOffset);

		/// <summary>
		/// Offsets this index line by the value provided.
		/// This does not modify the original index.
		/// </summary>
		/// <param name="lineOffset">The number to offset the <see cref="Line"/> by.</param>
		/// <returns>A new offset index.</returns>
		public TIndex OffsetLine(int lineOffset) => Offset(lineOffset, 0);

		/// <summary>
		/// Offsets this index character by the value provided.
		/// This does not modify the original index.
		/// </summary>
		/// <param name="characterOffset">The number to offset the <see cref="Character"/> by.</param>
		/// <returns>A new offset index.</returns>
		public TIndex OffsetCharacter(int characterOffset) => Offset(0, characterOffset);

		public static bool operator ==(TIndex a, TIndex b) => a.Equals(b);
		public static bool operator !=(TIndex a, TIndex b) => !a.Equals(b);
		public static bool operator >(TIndex a, TIndex b)
		{
			if (a.Line == b.Line && a.Character > b.Character)
			{
				return true;
			}
			else if (a.Line > b.Line)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public static bool operator <(TIndex a, TIndex b)
		{
			if (a.Line == b.Line && a.Character < b.Character)
			{
				return true;
			}
			else if (a.Line < b.Line)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public static bool operator >=(TIndex a, TIndex b) => a > b || a == b;
		public static bool operator <=(TIndex a, TIndex b) => a < b || a == b;

		public override bool Equals(object obj)
		{
			if (obj is TIndex index)
			{
				return index == this;
			}
			else
			{
				return false;
			}
		}
	}
}
