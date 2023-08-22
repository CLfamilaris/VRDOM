using Godot;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using VRDOM;

public partial class GameConnection : Node
{
    [Export]
    private string _websocket_url = "ws://localhost:9002";
    [Export]
    private FootSensor _footSensor;

    private CultureInfo formatLocale = new("en-US");
    private WebSocketPeer _wsPeer = new();
    readonly double lightingGetInterval = 0.0166;
    double lightingGetTimer = 0;

    public void GetLighting()
    {
        if (_wsPeer.GetReadyState() == WebSocketPeer.State.Open)
            _wsPeer.Send(Encoding.UTF8.GetBytes("{\"id\":0,\"module\":\"drs\",\"function\":\"tapeled_get\",\"params\":[]}"));
    }

    public void SetTouch(List<TouchCommand> touchCommands)
    {
        if (touchCommands.Count == 0)
            return;

        if (_wsPeer.GetReadyState() == WebSocketPeer.State.Open)
        {
            string touchCommandString = "{\"id\":1,\"module\":\"drs\",\"function\":\"touch_set\",\"params\":[";
            foreach (var touchCommand in touchCommands)
            {
                // honestly what did i smoke when i wrote this, i should REALLY go back and change this?? maybe?
                touchCommandString += "[" + touchCommand.touchEvent + "," + touchCommand.sensorId + "," + touchCommand.touchPosition.X.ToString("0.0000", formatLocale) + "," + touchCommand.touchPosition.Y.ToString("0.0000", formatLocale) + "," + touchCommand.touchSize.X.ToString("0.0000", formatLocale) + "," + touchCommand.touchSize.Y.ToString("0.0000", formatLocale) + "],";
            }
            touchCommandString = touchCommandString.Remove(touchCommandString.Length - 1);
            touchCommandString += "]}";
            _wsPeer.Send(Encoding.UTF8.GetBytes(touchCommandString));
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Connecting WebSocket...");
        _wsPeer.ConnectToUrl(_websocket_url); //this shit takes so goddamn long to connect, and I CANNOT FATHOM WHY
        _wsPeer.EncodeBufferMaxSize = 16 * 1024 * 1024; //give the buffer sizes a generous increase. it's a lot of data
        _wsPeer.InboundBufferSize = 16 * 1024 * 1024;
        _wsPeer.OutboundBufferSize = 16 * 1024 * 1024;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        lightingGetTimer += delta;
        _wsPeer.Poll();
        var state = _wsPeer.GetReadyState();
        switch (state)
        {
            case WebSocketPeer.State.Open:
                if (lightingGetTimer >= lightingGetInterval)
                {
                    GetLighting();
                    lightingGetTimer = 0;
                }

                for (int i = 0; i < _wsPeer.GetAvailablePacketCount(); i++)
                {
                    string packetText = Encoding.UTF8.GetString(_wsPeer.GetPacket());
                    if (packetText.Contains("id")) //questionable way to check if the message is valid
                    {
                        var dict = Json.ParseString(packetText).AsGodotDictionary();
                        if (dict.ContainsKey("data") && dict["id"].AsInt32() == 0) //Only the lighting data responses will have an ID of 0
                        {
                            int[] ledData = dict["data"].AsGodotArray()[0].AsInt32Array();
                            //The LED data is in the format of [r, g, b, r, g, b, ...]
                            //We need to turn that into an array of Vector3s and then push that to the shader
                            for (int j = 0; j < ledData.Length; j += 3)
                            {
                                _footSensor.gpu_led_data[j / 3] = new Vector3(ledData[j], ledData[j + 1], ledData[j + 2]);
                            }
                        }
                    }
                }
                break;
            case WebSocketPeer.State.Closed:
                GD.Print("WebSocket closed.");
                var code = _wsPeer.GetCloseCode();
                var reason = _wsPeer.GetCloseReason();
                GD.Print("WebSocket closed with code: %d, reason %s. Clean: %s", code, reason, code != -1);

                SetProcess(false);
                break;
        }
    }
}
