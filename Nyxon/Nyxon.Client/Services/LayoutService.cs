using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Services;

namespace Nyxon.Client.Services
{
    public class LayoutService : ILayoutService
    {
        public string? HeaderTitle { get; private set; }

        public event Action? OnChange; //mian layout is subscribed to this event

        public void SetTitle(string? title)
        {
            HeaderTitle = title;
            OnChange.Invoke();
        }
    }
}