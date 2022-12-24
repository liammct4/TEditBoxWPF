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
