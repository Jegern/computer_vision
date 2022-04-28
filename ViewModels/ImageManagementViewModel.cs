using System.Windows;
using System.Windows.Media.Imaging;
using Laboratory_work_1.Commands.Base;
using Laboratory_work_1.ViewModels.Base;
using Laboratory_work_1.ViewModels.Store;

namespace Laboratory_work_1.ViewModels;

public class ImageManagementViewModel : ViewModel
{
    #region Fields

    private readonly ViewModelStore? _store;
    private BitmapSource? _picture;
    private byte[]? _pictureBytes;
    private Visibility _imageManagementVisibility = Visibility.Collapsed;

    private int _intensivity = 127;
    private int _channelCounter;
    private bool _redChecked;
    private bool _greenChecked;
    private bool _blueChecked;
    private bool _horizontalChecked;
    private bool _verticalChecked;
    private bool _horizontalVerticalChecked;
    private bool _diagonalChecked;


    private BitmapSource? Picture
    {
        get => _picture;
        set
        {
            if (!Set(ref _picture, value)) return;
            _store?.TriggerPictureEvent(_picture);
            if (_picture is not null)
                _pictureBytes = _pictureBytes is null
                    ? Tools.GetPixelBytes(_picture)
                    : Tools.GetPixelBytes(_picture, _pictureBytes);
        }
    }

    public Visibility ImageManagementVisibility
    {
        get => _imageManagementVisibility;
        set => Set(ref _imageManagementVisibility, value);
    }

    public int Intensivity
    {
        get => _intensivity;
        set => Set(ref _intensivity, value);
    }

    private int ChannelCounter
    {
        get => _channelCounter;
        set => Set(ref _channelCounter, value);
    }

    public bool RedChecked
    {
        get => _redChecked;
        set
        {
            if (ChannelCounter == 2 && value) return;
            Set(ref _redChecked, value);
            ChannelCounter += value ? 1 : -1;
        }
    }

    public bool GreenChecked
    {
        get => _greenChecked;
        set
        {
            if (ChannelCounter == 2 && value) return;
            Set(ref _greenChecked, value);
            ChannelCounter += value ? 1 : -1;
        }
    }

    public bool BlueChecked
    {
        get => _blueChecked;
        set
        {
            if (ChannelCounter == 2 && value) return;
            Set(ref _blueChecked, value);
            ChannelCounter += value ? 1 : -1;
        }
    }

    public bool HorizontalChecked
    {
        get => _horizontalChecked;
        set => Set(ref _horizontalChecked, value);
    }

    public bool VerticalChecked
    {
        get => _verticalChecked;
        set => Set(ref _verticalChecked, value);
    }

    public bool HorizontalVerticalChecked
    {
        get => _horizontalVerticalChecked;
        set => Set(ref _horizontalVerticalChecked, value);
    }

    public bool DiagonalChecked
    {
        get => _diagonalChecked;
        set => Set(ref _diagonalChecked, value);
    }

    private bool[,] HorizontalVerticalMask { get; } =
    {
        {false, true, false},
        {true, true, true},
        {false, true, false}
    };

    private bool[,] DiagonalMask { get; } =
    {
        {true, false, true},
        {false, true, false},
        {true, false, true}
    };

    private bool[,] AllMask { get; } =
    {
        {true, true, true},
        {true, true, true},
        {true, true, true}
    };

    #endregion

    /// <summary>
    /// Default constructor for code suggestions
    /// </summary>
    public ImageManagementViewModel()
    {
    }

    public ImageManagementViewModel(ViewModelStore? store)
    {
        if (store is null) return;

        store.PictureChanged += Picture_OnChanged;
        _store = store;

        ImageManagementCommand = new Command(ImageManagementCommand_OnExecuted, ImageManagementCommand_CanExecute);
        ValueChangedCommand = new Command(ValueChangedCommand_OnExecuted, ValueChangedCommand_CanExecute);
        BleachCommand = new Command(BleachCommand_OnExecuted, BleachCommand_CanExecute);
        NegativeCommand = new Command(NegativeCommand_OnExecuted, NegativeCommand_CanExecute);
        SwapCommand = new Command(SwapCommand_OnExecuted, SwapCommand_CanExecute);
        SymmetryCommand = new Command(SymmetryCommand_OnExecuted, SymmetryCommand_CanExecute);
        VanishCommand = new Command(VanishCommand_OnExecuted, VanishCommand_CanExecute);
    }

    #region Event Subscription

    private void Picture_OnChanged(BitmapSource? picture)
    {
        Picture = picture;
    }

    #endregion

    #region Commands

    #region ImageManagementCommand

    public Command? ImageManagementCommand { get; }

    private bool ImageManagementCommand_CanExecute(object? parameter) => Picture is not null;

    private void ImageManagementCommand_OnExecuted(object? parameter)
    {
        ImageManagementVisibility = ImageManagementVisibility is Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    #endregion

    #region ValueChangedCommand

    public Command? ValueChangedCommand { get; }

    private bool ValueChangedCommand_CanExecute(object? parameter) => Picture is not null;

    private void ValueChangedCommand_OnExecuted(object? parameter)
    {
        var e = (RoutedPropertyChangedEventArgs<double>) parameter!;
        if (e.OldValue - e.NewValue == 0) return;

        var difference = e.OldValue - e.NewValue;
        var pictureBytes = Tools.GetPixelBytes(Picture!, _pictureBytes);

        for (var i = 0; i < pictureBytes.Length; i += 4)
        {
            if (0 <= pictureBytes[i + 0] + difference && pictureBytes[i + 0] + difference <= 255)
                pictureBytes[i + 0] = (byte)(pictureBytes[i + 0] + difference);
            if (0 <= pictureBytes[i + 1] + difference && pictureBytes[i + 1] + difference <= 255)
                pictureBytes[i + 1] = (byte)(pictureBytes[i + 1] + difference);
            if (0 <= pictureBytes[i + 2] + difference && pictureBytes[i + 2] + difference <= 255)
                pictureBytes[i + 2] = (byte)(pictureBytes[i + 2] + difference);
        }

        Picture = Tools.CreateImage(Picture!, pictureBytes);
    }

    #endregion

    #region BleachCommand

    public Command? BleachCommand { get; }

    private bool BleachCommand_CanExecute(object? parameter) => Picture is not null;

    private void BleachCommand_OnExecuted(object? parameter)
    {
        var pictureBytes = Tools.GetPixelBytes(Picture!, _pictureBytes);
        for (var i = 0; i < pictureBytes.Length; i += 4)
            Tools.SetPixel(
                Tools.GetPixel(pictureBytes, i),
                Tools.GetGrayPixel((byte) Tools.GetPixelIntensivity(pictureBytes, i)));
        Picture = Tools.CreateImage(Picture!, pictureBytes);
    }

    #endregion

    #region NegativeCommand

    public Command? NegativeCommand { get; }

    private bool NegativeCommand_CanExecute(object? parameter) => Picture is not null;

    private void NegativeCommand_OnExecuted(object? parameter)
    {
        var pictureBytes = Tools.GetPixelBytes(Picture!, _pictureBytes);
        for (var i = 0; i < pictureBytes.Length; i += 4)
        {
            pictureBytes[i + 0] = (byte) (255 - pictureBytes[i + 0]);
            pictureBytes[i + 1] = (byte) (255 - pictureBytes[i + 1]);
            pictureBytes[i + 2] = (byte) (255 - pictureBytes[i + 2]);
        }

        Picture = Tools.CreateImage(Picture!, pictureBytes);
    }

    #endregion

    #region SwapCommand

    public Command? SwapCommand { get; }

    private bool SwapCommand_CanExecute(object? parameter) => Picture is not null && ChannelCounter == 2;

    private void SwapCommand_OnExecuted(object? parameter)
    {
        var pictureBytes = Tools.GetPixelBytes(Picture!, _pictureBytes);
        var firstChannel = RedChecked ? 2 : 1;
        var secondChannel = BlueChecked ? 0 : 1;

        for (var i = 0; i < pictureBytes.Length; i += 4)
            Tools.Swap(ref pictureBytes[i + firstChannel], ref pictureBytes[i + secondChannel]);
        Picture = Tools.CreateImage(Picture!, pictureBytes);
    }

    #endregion

    #region SymmetryCommand

    public Command? SymmetryCommand { get; }

    private bool SymmetryCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        (HorizontalChecked || VerticalChecked);

    private void SymmetryCommand_OnExecuted(object? parameter)
    {
        var pictureBytes = Tools.GetPixelBytes(Picture!, _pictureBytes);
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;

        if (HorizontalChecked)
            for (var i = 0; i < height / 2; i++)
            for (var j = 0; j < width; j++)
                Tools.SwapPixels(
                    Tools.GetPixel(pictureBytes, i * width * 4 + j * 4),
                    Tools.GetPixel(pictureBytes, (height - 1 - i) * width * 4 + j * 4));

        if (VerticalChecked)
            for (var i = 0; i < height; i++)
            for (var j = 0; j < width / 2; j++)
                Tools.SwapPixels(
                    Tools.GetPixel(pictureBytes, i * width * 4 + j * 4),
                    Tools.GetPixel(pictureBytes, i * width * 4 + (width - j - 1) * 4));

        Picture = Tools.CreateImage(Picture!, pictureBytes);
    }

    #endregion

    #region VanishCommand

    public Command? VanishCommand { get; }

    private bool VanishCommand_CanExecute(object? parameter) =>
        Picture is not null &&
        (HorizontalVerticalChecked || DiagonalChecked);

    private void VanishCommand_OnExecuted(object? parameter)
    {
        var pictureBytes = Tools.GetPixelBytes(Picture!, _pictureBytes);
        var originalPictureBytes = (byte[]) pictureBytes.Clone();
        var width = Picture!.PixelWidth;
        var height = Picture!.PixelHeight;
        var mask = HorizontalVerticalChecked switch
        {
            true when DiagonalChecked => AllMask,
            true => HorizontalVerticalMask,
            false => DiagonalMask
        };

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sum = 0d;
            var counter = 0;
            for (var i = -1; i <= 1; i++)
            {
                if (y + i < 0 | y + i >= height) continue;
                for (var j = -1; j <= 1; j++)
                {
                    if (j + x < 0 | j + x >= width) continue;
                    if (!mask[i + 1, j + 1]) continue;
                    var windowPixelIndex = (y + i) * width * 4 + (x + j) * 4;
                    var windowPixel = Tools.GetPixelIntensivity(originalPictureBytes, windowPixelIndex);
                    sum += windowPixel;
                    counter++;
                }
            }

            Tools.SetPixel(
                Tools.GetPixel(pictureBytes, y * width * 4 + x * 4),
                Tools.GetGrayPixel((byte) (sum / counter)));
        }

        Picture = Tools.CreateImage(Picture!, pictureBytes);
    }

    #endregion

    #endregion
}