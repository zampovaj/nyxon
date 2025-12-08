using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Services;

namespace Nyxon.Client.Services
{
    public class LayoutService
    {
        public bool IsLoggedIn { get; private set; } = false;
        public string? HeaderTitle { get; private set; } = "";
        public string Username { get; private set;} = "";
        public event Action? OnChange;

        public void SetLoggedInState(bool isLoggedIn)
        {
            IsLoggedIn = isLoggedIn;
            if (!isLoggedIn) HeaderTitle = null;
            NotifyListeners();
        }
        public void SetHeaderTitle(string? headerTitle)
        {
            HeaderTitle = headerTitle;
            NotifyListeners();
        }
        private void NotifyListeners() => OnChange?.Invoke();
    }
}