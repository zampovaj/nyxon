using System;

namespace Nyxon.Client.Services
{
    public interface ILayoutService
    {
        string? HeaderTitle { get; }
        event Action? OnChange;
        void SetTitle(string? title);
    }
}