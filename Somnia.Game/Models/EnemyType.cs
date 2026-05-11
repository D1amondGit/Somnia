namespace Somnia.Game.Models;

public enum EnemyType
{
    /// <summary>Стандартный ближник: подходит и кусает.</summary>
    Melee,

    /// <summary>Дистанционник: держит дистанцию, плюётся снарядами.</summary>
    Shooter,

    /// <summary>Берсерк: быстрый, тонкий, ломится напрямую и взрывается на контакте.</summary>
    Charger,

    /// <summary>Снайпер: стоит на расстоянии, телеграфирует луч, стреляет дальним быстрым выстрелом.</summary>
    Sniper,

    /// <summary>Босс акта 1: огромная HP, медленный, набор сценарных атак (Slam/Volley/Charge).</summary>
    Boss
}
