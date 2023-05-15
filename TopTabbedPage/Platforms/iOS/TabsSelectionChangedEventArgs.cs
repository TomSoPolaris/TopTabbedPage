using System;
namespace TopTabbedPage
{
    public class TabsSelectionChangedEventArgs : EventArgs
    {
        public nuint SelectedIndex { get; }

        public TabsSelectionChangedEventArgs(nuint selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
}
