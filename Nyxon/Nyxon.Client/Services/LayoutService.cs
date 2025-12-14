using System;

namespace Nyxon.Client.Services
{
    public class LayoutService
    {
        public event Action? OnChange;

        public string HeaderTitle { get; private set; } = "";
        
        public void SetHeaderTitle(string title)
        {
            HeaderTitle = title;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}