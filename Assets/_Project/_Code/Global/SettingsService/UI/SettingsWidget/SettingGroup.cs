using System;
using System.Collections.Generic;

namespace _Project._Code.Global.SettingsService.UI
{
    public sealed class SettingGroup
    {
        public string Title => _title;
        public IReadOnlyList<IDisposable> Presenters => _presenters;
        
        private readonly string _title;
        private readonly List<IDisposable> _presenters = new();

        public SettingGroup(string title)
        {
            _title = title;
        }
        
        public void Add(IDisposable presenter) => _presenters.Add(presenter);
        
        public void Dispose()
        {
            for (int i = 0; i < _presenters.Count; i++)
                _presenters[i].Dispose();
        }
    }
}