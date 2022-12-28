using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TEditBoxWPF.Controls.ScrollableItemsControl
{
    internal class ScrollingVirtualizationPanel : VirtualizingStackPanel
    {
        public void ScrollIntoView(int index) => BringIndexIntoView(index);
    }
}
