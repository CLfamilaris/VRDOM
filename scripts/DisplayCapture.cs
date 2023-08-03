using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ScreenCapture.NET;

public partial class DisplayCapture : CsgMesh3D
{
    private IScreenCapture _screenCapture;
    private CaptureZone _capZone;
    private ImageTexture _texture;
    private Image _image;
    private Thread _capThread;
    private CancellationTokenSource cancel = new CancellationTokenSource();

    public override void _Ready()
    {
        // Create a screen-capture service
        IScreenCaptureService screenCaptureService = new DX11ScreenCaptureService();

        // Get all available graphics cards
        IEnumerable<GraphicsCard> graphicsCards = screenCaptureService.GetGraphicsCards();

        // Get the displays from the graphics card(s) you are interested in
        IEnumerable<Display> displays = screenCaptureService.GetDisplays(graphicsCards.First());

        // Create a screen-capture for all screens you want to capture
        _screenCapture = screenCaptureService.GetScreenCapture(displays.First());

        // Register the regions you want to capture on the screen
        // Capture the whole screen
        _capZone = _screenCapture.RegisterCaptureZone(0, 0, _screenCapture.Display.Width, _screenCapture.Display.Height);

        _image = Image.Create(_capZone.Width, _capZone.Height, false, Image.Format.Rgba8);
        _image.Fill(Color.Color8(0, 0, 0, 255));
        _texture = ImageTexture.CreateFromImage(_image);

        _capThread = new(new ThreadStart(RefreshCaptureThread));
        _capThread.Start();
    }

    void RefreshCaptureThread()
    {
        while (!cancel.IsCancellationRequested)
        {
            _screenCapture.CaptureScreen();
            lock (_capZone.Buffer)
            {
                _image.SetData(_capZone.Width, _capZone.Height, false, Image.Format.Rgba8, _capZone.Buffer);
                _texture.CallDeferred(ImageTexture.MethodName.Update, _image);
                Thread.Sleep(16);
            }
        }
    }

    public override void _ExitTree()
    {
        cancel.Cancel();
        _screenCapture.Dispose();
    }

    public override void _Process(double delta)
    {
        (Material as ShaderMaterial).SetShaderParameter("texture_albedo", _texture);
    }
}