using _Project._Code.Core.Keys;

namespace _Project._Code.Locale
{
    public interface IWidgetShower
    {
        WidgetId ID { get; }
        void Show();
        void Hide();
    }
}