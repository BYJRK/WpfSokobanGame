using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace WpfSokobanGame.GameObjects;

partial class MoveableObject : Sprite
{
    [ObservableProperty]
    bool isOnTarget;

    public MoveableObject(int x, int y, SpriteType type) : base(x, y, type)
    {
    }

    public void CheckIfOnTarget(IEnumerable<Sprite>? blocks)
    {
        // 先不考虑效率，因为关卡很小
        IsOnTarget = blocks?.Any(b => b.Type == SpriteType.Target && b.X == X && b.Y == Y) ?? false;
    }
}
