﻿using System.Collections.ObjectModel;
using System.Windows.Ink;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SketchNow.Models;

public partial class CanvasPage : ObservableObject
{
    [ObservableProperty]
    private StrokeCollection _strokes = [];

    /// <summary>
    /// To Notify the Not ObservableProperty <see cref="_strokes"/>
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand), nameof(RedoCommand), nameof(ClearCommand))]
    private int _counter;

    private readonly ObservableCollection<StrokeCollection> _undoStack = [];
    private readonly ObservableCollection<StrokeCollection> _redoStack = [];

    public CanvasPage()
    {
        Strokes.StrokesChanged += Strokes_Changed;
        _undoStack.Add(CloneStrokeCollection(_strokes));
    }
    private void ChangeCounter()
    {
        Counter++;
        Counter=0;
    }
    private void Strokes_Changed(object sender, StrokeCollectionChangedEventArgs e)
    {
        _undoStack.Add(CloneStrokeCollection(Strokes));
        _redoStack.Clear();
        ChangeCounter();
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _redoStack.Add(CloneStrokeCollection(Strokes));
        _undoStack.RemoveAt(_undoStack.Count - 1);
        Strokes = CloneStrokeCollection(_undoStack[^1]);
        ChangeCounter();
    }

    private bool CanUndo() => _undoStack.Count > 1;

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        _undoStack.Add(CloneStrokeCollection(Strokes));
        Strokes = CloneStrokeCollection(_redoStack[^1]);
        _redoStack.RemoveAt(_redoStack.Count - 1);
        ChangeCounter();
    }

    private bool CanRedo() => _redoStack.Count > 0;

    private StrokeCollection CloneStrokeCollection(StrokeCollection strokes)
    {
        var clonedStrokes = new StrokeCollection();
        foreach (var stroke in strokes)
        {
            clonedStrokes.Add(stroke.Clone());
        }
        clonedStrokes.StrokesChanged += Strokes_Changed;
        return clonedStrokes;
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    private void Clear()
    {
        Strokes.Clear();
    }
    private bool CanClear() => Strokes.Count > 0;
}
public partial class CanvasPages : ObservableObject
{
    /// <summary>
    /// Page 1 was reserved by default desktop canvas. For the convenience of managing <see cref="CanvasPages"/>, the latter <see cref="CanvasPages"/> are Board pages.
    /// </summary>
    [ObservableProperty] private ObservableCollection<CanvasPage> _pages = [new()];
    [ObservableProperty] private int _length;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPage))]
    [NotifyCanExecuteChangedFor(nameof(PreviousCommand), nameof(NextCommand))]
    private int _selectedIndex;
    public CanvasPage SelectedPage
    {
        get => Pages[SelectedIndex];
        set
        {
            Pages[SelectedIndex] = value;
            OnPropertyChanged();
        }
    }
    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next()
    {
        SelectedIndex++;
        OnPropertyChanged(nameof(SelectedPage));
    }
    private bool CanNext => SelectedIndex < Pages.Count - 1;
    [RelayCommand(CanExecute = nameof(CanPrevious))]
    private void Previous()
    {
        SelectedIndex--;
        OnPropertyChanged(nameof(SelectedPage));
    }
    private bool CanPrevious => SelectedIndex > 1;
    [RelayCommand]
    private void AddPage()
    {
        Pages.Insert(SelectedIndex + 1, new CanvasPage());
        SelectedIndex++;
        Length = Pages.Count;
        OnPropertyChanged(nameof(SelectedPage));
    }
    public CanvasPages()
    {
        Length = Pages.Count;
    }
}