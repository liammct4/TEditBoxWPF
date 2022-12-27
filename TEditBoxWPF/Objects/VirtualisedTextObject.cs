using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using TEditBoxWPF.LineStructure;
using TEditBoxWPF.TextStructure;
using TEditBoxWPF.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TEditBoxWPF.Objects
{
	/// <summary>
	/// Represents a non-text object such as a caret within a <see cref="TEditBox"/>.
	/// 
	/// Handles virtualisation rendering and safe <see cref="Control"/>
	/// placement within a virtualised itemcontrol.<br/><br/>
	/// 
	/// <see cref="ItemContainerGenerator"/>'s which use virtualised panels dynamically
	/// generate and destroy rendered items based on whether they are visible
	/// to the user.<br/><br/>
	/// 
	/// When rendered items are destroyed, any changes made to items
	/// are also destroyed, such as manually added objects. <br/><br/>
	/// 
	/// This class will safely regenerate objects within a virtualised panel.
	/// </summary>
	public class VirtualisedTextObject<T> where T: FrameworkElement
	{
		/// <summary>
		/// The parent of this text object.
		/// </summary>
		public TEditBox Parent { get; }

		/// <summary>
		/// The control to render. Offset the control using hte
		/// </summary>
		public T VirtualisedObject { get; }

		/// <summary>
		/// The line object which this control is placed in.
		/// </summary>
		public TLine Line
		{
			get => line;
			set
			{
				if (value.Parent != Parent)
				{
					throw new InvalidOperationException("The line provided is not contained within the same textbox.");
				}

				line = value;
				Place();
			}
		}
		private TLine line;

		/// <summary>
		/// The line and character position of the control in the textbox.
		/// </summary>
		public TIndex Position
		{
			// The line position is linked to Line, while the character is arbitrary. 
			get => new(Line.Position, characterPos);
			set
			{
				characterPos = value.Character;
				Line = Parent.Lines[value.Line];
			}
		}

		/// <summary>
		/// The virtualised panel which the control belongs to.
		/// </summary>
		public ItemsControl VirtualisationPanel { get; }

		public bool IsPlaced
		{
			get => _isPlaced;
			set
			{
				_isPlaced = value;

				if (!value && previousBox != null)
				{
					previousBox.Children.Remove(VirtualisedObject);
				}
				else
				{
					Place();
				}
			}
		}
		private bool _isPlaced;
		private Grid previousBox;
		private int characterPos;

		public VirtualisedTextObject(TEditBox parent, TLine line, ItemsControl virtualisationPanel, T control)
		{
			Parent = parent;
			VirtualisedObject = control;
			VirtualisationPanel = virtualisationPanel;
			Line = line;

			virtualisationPanel.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
		}

		/// <summary>
		/// Informs the object to update the placement of the virtualised object.
		/// </summary>
		private void ItemContainerGenerator_StatusChanged(object? sender, EventArgs e)
		{
			if (VirtualisationPanel.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				VirtualisedObject.HorizontalAlignment = HorizontalAlignment.Left;
				Place();
			}
		}

		/// <summary>
		/// Attempts to place the control in the line. If the line is not
		/// generated, the object will not be placed. This method is automatically
		/// linked with the items panel.
		/// </summary>
		public void Place()
		{
			if (Line.Parent.TextDisplay.ItemContainerGenerator.ContainerFromItem(Line) is not ContentPresenter container || !IsPlaced)
			{
				return;
			}

			// The textblock which renders the text is wrapped around a Grid, the object is placed in the grid.
			Grid box = container.GetDescendantByType<Grid>();

			string marginWidth = Line.Text[0..Math.Max(0, characterPos)];
			double marginFromCharacterPosition = Parent.measurer.MeasureTextSize(marginWidth, true).Width;

			VirtualisedObject.Margin = new Thickness(marginFromCharacterPosition, 0, 0, 0);

			// If the user can already see the box.
			if (!box.Children.Contains(VirtualisedObject))
			{
				// The rectangle has to be disconnected before 
				if (previousBox != null && previousBox.Children.Contains(VirtualisedObject))
				{
					previousBox.Children.Remove(VirtualisedObject);
				}

				box.Children.Add(VirtualisedObject);

				previousBox = box;
			}
		}
	}
}
