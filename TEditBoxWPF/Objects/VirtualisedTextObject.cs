using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TEditBoxWPF.LineStructure;

namespace TEditBoxWPF.Objects
{
	/// <summary>
	/// Represents a non-text object such as a caret within a <see cref="TEditBox"/>.
	/// 
	/// Handles virtualisation rendering and safe <see cref="Visual"/>
	/// placement within a virtualised itemcontrol.
	/// </summary>
	internal abstract class VirtualisedTextObject<T> where T: Visual
	{
		public T VirtualisedObject { get; }
		public TLine Line { get; set; }

		public VirtualisedTextObject(TLine line, T control)
		{
			VirtualisedObject = control;
			Line = line;
		}
	}
}
