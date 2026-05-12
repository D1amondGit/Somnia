namespace Somnia.Game.Models;

/// <summary>
/// Состояние босса в его «KI-цикле»: телеграф → исполнение → откат.
/// Один таймер <see cref="EnemyModel.BossPhaseTimer"/> отсчитывает длительность каждой фазы,
/// затем босс выбирает следующее действие по HP.
/// </summary>
public enum BossAttackPhase
{
    /// <summary>Босс «думает», стоит на месте — короткая пауза между атаками.</summary>
    Idle = 0,

    /// <summary>Готовит slam: рисуется огромная красная окружность под игроком.</summary>
    SlamTelegraph,

    /// <summary>Slam ударил в этом кадре. Используется, чтобы один раз нанести урон.</summary>
    SlamImpact,

    /// <summary>Готовит volley: выпустит веер снарядов.</summary>
    VolleyTelegraph,

    /// <summary>Volley стреляет (короткий импульс длительностью кадр-два).</summary>
    VolleyFire,

    /// <summary>Берсерк-рывок к игроку.</summary>
    Charge
}
