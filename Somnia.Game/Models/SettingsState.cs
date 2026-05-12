namespace Somnia.Game.Models;

/// <summary>
/// Состояние экрана настроек: текущий пункт фокуса и значения громкостей (0..1).
/// </summary>
public sealed class SettingsState
{
    public int SelectedIndex { get; set; }
    public int OptionCount { get; set; } = 3;

    public void MoveUp()
    {
        SelectedIndex = (SelectedIndex - 1 + OptionCount) % OptionCount;
    }

    public void MoveDown()
    {
        SelectedIndex = (SelectedIndex + 1) % OptionCount;
    }
}
