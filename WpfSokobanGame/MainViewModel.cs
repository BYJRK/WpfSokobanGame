using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace WpfSokobanGame;

internal partial class MainViewModel : ObservableObject
{
    public Level Level { get; } = new();

    private int totalLevelCount;

    public MainViewModel()
    {
        totalLevelCount = Level.GetTotalLevelCount();
        Level.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Level.CurrentLevel))
            {
                NextLevelCommand.NotifyCanExecuteChanged();
                PrevLevelCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(Level.CanUndo))
            {
                UndoCommand.NotifyCanExecuteChanged();
            }
        };
    }

    [RelayCommand]
    private void MovePlayer(KeyEventArgs e)
    {
        if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right)
        {
            Level.MovePlayer(e.Key);
        }
    }

    [RelayCommand]
    private void Restart() => Level.Restart();

    [RelayCommand(CanExecute = nameof(CanNextLevel))]
    private void NextLevel() => Level.NextLevel();

    [RelayCommand(CanExecute = nameof(CanPrevLevel))]
    private void PrevLevel() => Level.PrevLevel();

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo() => Level.Undo();

    private bool CanUndo() => Level.CanUndo;

    private bool CanNextLevel() => Level.CurrentLevel < totalLevelCount;

    private bool CanPrevLevel() => Level.CurrentLevel > 1;
}