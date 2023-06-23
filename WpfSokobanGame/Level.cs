using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using WpfSokobanGame.GameObjects;

namespace WpfSokobanGame;

internal partial class Level : ObservableObject
{
    #region Observable Properties

    public double LevelWidth => Blocks?.Max(b => b.ActualX) + Sprite.GridSize ?? 0;
    public double LevelHeight => Blocks?.Max(b => b.ActualY) + Sprite.GridSize ?? 0;

    [ObservableProperty]
    private int currentLevel = 1;

    [ObservableProperty]
    private int totalSteps = 0;

    [ObservableProperty]
    private bool canUndo;

    [ObservableProperty]
    private bool isWinning;

    /// <summary>
    /// 场上的所有墙、目标点、地面
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LevelWidth), nameof(LevelHeight))]
    private List<Sprite>? blocks;

    /// <summary>
    /// 场上的所有箱子
    /// </summary>
    [ObservableProperty]
    private List<MoveableObject>? crates;

    /// <summary>
    /// 玩家
    /// </summary>
    [ObservableProperty]
    private MoveableObject? player;

    #endregion Observable Properties

    public Level()
    {
        ParseLevelText(GetCurrentLevelText());
    }

    private string GetCurrentLevelText()
    {
        var type = typeof(Properties.Resources);
        var levelName = $"Level{CurrentLevel}";
        var prop = type.GetProperty(levelName, BindingFlags.NonPublic | BindingFlags.Static);
        var text = prop?.GetValue(null) as string;
        if (text is null)
            throw new NullReferenceException($"Level {CurrentLevel} not found");
        return text;
    }

    public int GetTotalLevelCount()
    {
        var type = typeof(Properties.Resources);
        var props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Static);
        return props.Count(f => f.Name.StartsWith("Level"));
    }

    /// <summary>
    /// 解析关卡字符串从而生成对应关卡
    /// </summary>
    private void ParseLevelText(string levelText)
    {
        var blocks = new List<Sprite>();
        var crates = new List<MoveableObject>();
        Reset();
        var lines = levelText.Split(Environment.NewLine);
        for (int i = 0; i < lines.Length; i++)
        {
            /**
             * #: Wall
             * @: Player
             * _: Floor
             * $: Crate
             * .: Target
             */
            for (int j = 0; j < lines[i].Length; j++)
            {
                switch (lines[i][j])
                {
                    case '#':
                        blocks.Add(new Sprite(j, i, SpriteType.Wall));
                        break;

                    case '_':
                        blocks.Add(new Sprite(j, i, SpriteType.Floor));
                        break;

                    case '.':
                        blocks.Add(new Sprite(j, i, SpriteType.Target));
                        break;

                    case '$':
                        crates.Add(new MoveableObject(j, i, SpriteType.Crate));
                        break;

                    case '@':
                        Player = new MoveableObject(j, i, SpriteType.Player);
                        break;
                }
            }
        }

        Blocks = blocks;
        Crates = crates;
    }

    internal void MovePlayer(Key key)
    {
        if (Player is null)
            throw new NullReferenceException("Player is null");
        var (x, y) = (Player.X, Player.Y);
        (int x, int y) offset;
        switch (key)
        {
            case Key.Up:
                offset = (0, -1);
                break;

            case Key.Down:
                offset = (0, 1);
                break;

            case Key.Left:
                offset = (-1, 0);
                break;

            case Key.Right:
                offset = (1, 0);
                break;

            default:
                return;
        }
        x += offset.x;
        y += offset.y;
        // 判断是否撞墙
        if (HasWallAt(x, y))
            return;

        // 判断是否能推动箱子
        var crate = HasCrateAt(x, y);
        if (crate is not null)
        {
            var (wx, wy) = (x, y);
            wx += offset.x;
            wy += offset.y;
            // 如果箱子被推动的方向有墙或其他箱子，则拒绝移动
            if (HasWallAt(wx, wy) || HasCrateAt(wx, wy) is not null)
                return;

            crate.X = wx;
            crate.Y = wy;

            AddHistory(crate, offset.x, offset.y);

            crate.CheckIfOnTarget(Blocks);
        }

        Player.X = x;
        Player.Y = y;

        AddHistory(Player, offset.x, offset.y);

        TotalSteps++;

        CheckIfWin();
    }

    /// <summary>
    /// 判断某个位置是否有墙
    /// </summary>
    private bool HasWallAt(int x, int y) => Blocks?.Any(s => s.Type == SpriteType.Wall && s.X == x && s.Y == y) ?? false;

    /// <summary>
    /// 判断某个位置是否有箱子
    /// </summary>
    private MoveableObject? HasCrateAt(int x, int y) => Crates?.Find(s => s.X == x && s.Y == y);

    private void CheckIfWin()
    {
        if (Crates.All(c => c.IsOnTarget))
        {
            IsWinning = true;
        }
    }

    public void Restart()
    {
        ParseLevelText(GetCurrentLevelText());
    }

    private void Reset()
    {
        TotalSteps = 0;
        history.Clear();
        CanUndo = false;
        IsWinning = false;
    }

    internal void NextLevel()
    {
        CurrentLevel++;
        ParseLevelText(GetCurrentLevelText());
    }

    internal void PrevLevel()
    {
        CurrentLevel--;
        ParseLevelText(GetCurrentLevelText());
    }

    #region Undo

    private Stack<(MoveableObject obj, int x, int y)> history = new();

    public void Undo()
    {
        if (!CanUndo) return;

        var (obj, x, y) = history.Pop();
        obj.X -= x;
        obj.Y -= y;
        if (obj.Type == SpriteType.Player)
            TotalSteps--;
        CanUndo = history.Count > 0;

        // 如果上一个记录是箱子，那么说明是这次推动的，将会一并推动
        if (history.Count > 0 && history.Peek().obj.Type == SpriteType.Crate)
            Undo();
    }

    private void AddHistory(MoveableObject obj, int x, int y)
    {
        history.Push((obj, x, y));
        CanUndo = history.Count > 0;
    }

    #endregion
}