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
	/// Handles virtualization rendering and safe <see cref="Control"/>
	/// placement within a virtualized itemcontrol.<br/><br/>
	/// 
	/// <see cref="ItemContainerGenerator"/>'s which use virtualized panels dynamically
	/// generate and destroy rendered items based on whether they are visible
	/// to the user.<br/><br/>
	/// 
	/// When rendered items are destroyed, any changes made to items
	/// are also destroyed, such as manually added objects. <br/><br/>
	/// 
	/// This class will safely regenerate objects within a virtualized panel.
	/// </summary>
	public class VirtualizedTextObject<T> where T: FrameworkElement
	{
		/// <summary>
		/// The parent of this text object.
		/// </summary>
		public TEditBox Parent { get; }

		/// <summary>
		/// The control to render. Offset the control using hte
		/// </summary>
		public T VirtualizedObject { get; }

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
				previousLine = value.Position;
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
		/// The virtualized panel which the control belongs to.
		/// </summary>
		public ItemsControl VirtualizationPanel { get; }

		public bool IsPlaced
		{
			get => _isPlaced;
			set
			{
				_isPlaced = value;

				if (!value && previousBox != null)
				{
					previousBox.Children.Remove(VirtualizedObject);
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
		private int previousLine;

		public VirtualizedTextObject(TEditBox parent, TLine line, ItemsControl virtualizationPanel, T control)
		{
			Parent = parent;
			VirtualizedObject = control;
			VirtualizationPanel = virtualizationPanel;
			Line = line;

			virtualizationPanel.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
			virtualizationPanel.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
		}

		/// <summary>
		/// Informs the object to update the placement of the virtualized object.
		/// </summary>
		private void ItemContainerGenerator_StatusChanged(object? sender, EventArgs e)
		{
			if (VirtualizationPanel.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				Place();
			}
		}

		/// <summary>
		/// Informs the object to reset the position of the caret to line 0, character 0.
		/// </summary>
		private void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
			{
				Position = TIndex.Start;
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

			VirtualizedObject.HorizontalAlignment = HorizontalAlignment.Left;
			VirtualizedObject.Margin = new Thickness(marginFromCharacterPosition, 0, 0, 0);

			// If the user can already see the box.
			if (!box.Children.Contains(VirtualizedObject))
			{
				// The rectangle has to be disconnected before 
				if (previousBox != null && previousBox.Children.Contains(VirtualizedObject))
				{
					previousBox.Children.Remove(VirtualizedObject);
				}

				box.Children.Add(VirtualizedObject);

				previousBox = box;
			}
		}
	}
}
