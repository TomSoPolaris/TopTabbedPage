using System;
namespace TopTabbedPage
{
    public interface ITabBarDelegate
    {
        void DidChangeSelectedIndex(MaterialDesignTabBar tabBar, nuint selectedIndex);
    }
}
