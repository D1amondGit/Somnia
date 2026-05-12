namespace Somnia.Game.Models;

/// <summary>
/// Фазы UI/жизненного цикла раунда. Старый <c>int UiState</c> в <see cref="Session.GameplaySessionState"/>
/// хранит эти же значения — константы нужны, чтобы не было «магических чисел» по коду.
/// </summary>
public static class GameplayPhase
{
    public const int Playing = 0;
    public const int Paused = 1;
    public const int GameOver = 2;
    public const int Title = 3;
    public const int Settings = 4;
}
