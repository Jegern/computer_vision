﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Services;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class MainViewModel : ViewModel
{
    #region Fields

    private readonly FileService _fileService = new();
    private readonly DialogService _dialogService = new();
    private BitmapSource? _originalPicture;
    private byte[]? _pictureBytes;
    // private int[]? _histogram;
    private Point _pictureMousePosition;
    
    private string? _title;

    private BitmapSource? OriginalPicture
    {
        get => _originalPicture;
        set => Set(ref _originalPicture, value);
    }

    private new byte[]? PictureBytes
    {
        set
        {
            if (Set(ref _pictureBytes, value))
                Store?.TriggerPictureBytesEvent(_pictureBytes!);
        }
    }

    // private int[]? Histogram
    // {
    //     set
    //     {
    //         if (Set(ref _histogram, value)) ;
    //         //_store?.TriggerHistogramEvent()
    //     }
    // }

    private Point PictureMousePosition
    {
        get => _pictureMousePosition;
        set
        {
            if (Set(ref _pictureMousePosition, value))
                Store?.TriggerMousePositionEvent(PictureMousePosition);
        }
    }
    
    public string? Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public MainViewModel()
    {
    }

    public MainViewModel(ViewModelStore? store) : base(store)
    {
        OpenImageCommand = new Command(
            OpenImageCommand_OnExecuted, 
            OpenImageCommand_CanExecute);
        SaveImageCommand = new Command(
            SaveImageCommand_OnExecuted, 
            SaveImageCommand_CanExecute);
        ReturnOriginalImageCommand = new Command(
            ReturnOriginalImageCommand_OnExecuted, 
            ReturnOriginalImageCommand_CanExecute);
        MouseMoveCommand = new Command(
            MouseMoveCommand_OnExecuted, 
            MouseMoveCommand_CanExecute);
    }

    #region Commands

    #region OpenImageCommand

    public Command? OpenImageCommand { get; }

    private bool OpenImageCommand_CanExecute(object? parameter) => true;

    private void OpenImageCommand_OnExecuted(object? parameter)
    {
        if (!_dialogService.OpenFileDialog()) return;
        if (_fileService.Open(_dialogService.FilePath!))
        {
            Picture = _fileService.OpenedImage!;
            OriginalPicture = Picture.Clone();
            PictureBytes = Tools.GetPixelBytes(Picture);
            Title = _fileService.ImageName;
            Tools.ResizeAndCenterWindow(Application.Current.MainWindow);
        }
        else
            DialogService.ShowError("Приложение не поддерживает картинки больше 1600x900");
    }

    #endregion

    #region SaveImageCommand

    public Command? SaveImageCommand { get; }

    private bool SaveImageCommand_CanExecute(object? parameter) => Picture is not null;

    private void SaveImageCommand_OnExecuted(object? parameter)
    {
        if (!_dialogService.SaveFileDialog()) return;
        _fileService.Save(_dialogService.FilePath!, Picture!);
    }

    #endregion
    
    #region ReturnOriginalImageCommand

    public Command? ReturnOriginalImageCommand { get; }

    private bool ReturnOriginalImageCommand_CanExecute(object? parameter) => OriginalPicture is not null;

    private void ReturnOriginalImageCommand_OnExecuted(object? parameter)
    {
        Picture = OriginalPicture;
        PictureBytes = Tools.GetPixelBytes(OriginalPicture!);
    }

    #endregion

    #region MouseMoveCommand

    public Command? MouseMoveCommand { get; }

    private bool MouseMoveCommand_CanExecute(object? parameter) =>
        Picture is not null && ((MouseEventArgs) parameter!).Source is Image;

    private void MouseMoveCommand_OnExecuted(object? parameter)
    {
        var e = (MouseEventArgs) parameter!;
        var position = e.GetPosition((Image) e.Source);
        position.X = Math.Round(Math.Min(position.X, Picture!.PixelWidth - 1), 0);
        position.Y = Math.Round(Math.Min(position.Y, Picture!.PixelHeight - 1), 0);
        PictureMousePosition = position;
    }

    #endregion

    #endregion
}