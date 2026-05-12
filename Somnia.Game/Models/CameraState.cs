using Microsoft.Xna.Framework;

namespace Somnia.Game.Models;

/// <summary>
/// Состояние камеры: позиция мира (для скролла, если когда-нибудь будет) + смещение шейка.
/// На итоговый рендер передаётся <see cref="WorldTransform"/>, ввод — <see cref="InputTransform"/>
/// (без шейка, чтобы прицел не дёргался).
/// </summary>
public sealed class CameraState
{
    public Vector2 ShakeOffset;
    public float ShakeTrauma;

    public Matrix InputTransform => Matrix.Identity;

    public Matrix WorldTransform =>
        Matrix.CreateTranslation(ShakeOffset.X, ShakeOffset.Y, 0f);
}
