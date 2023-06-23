using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfSokobanGame.GameObjects;

partial class Sprite : ObservableObject
{
    /// <summary>
    /// 格子宽度
    /// </summary>
    public const double GridSize = 100;

    public SpriteType Type { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActualX))]
    int x;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActualY))]
    int y;

    public Sprite(int x, int y, SpriteType type)
    {
        X = x;
        Y = y;
        Type = type;
    }

    /// <summary>
    /// 实际在画板上的 X 坐标
    /// </summary>
    public double ActualX => X * GridSize;
    /// <summary>
    /// 实际在画板上的 Y 坐标
    /// </summary>
    public double ActualY => Y * GridSize;
}
