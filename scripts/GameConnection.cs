using Godot;
using System.Collections.Generic;
using System.Text;
using VRDOM;

public partial class GameConnection : Node
{
    [Export]
    private string _websocket_url = "ws://localhost:9002";
    [Export]
    private FootSensor _footSensor;

    private WebSocketPeer wsPeer = new WebSocketPeer();
    double lightingGetInterval = 0.0166;
    double lightingGetTimer = 0;

    public void getLighting()
    {
        if (wsPeer.GetReadyState() == WebSocketPeer.State.Open)
            wsPeer.Send(Encoding.UTF8.GetBytes("{\"id\":0,\"module\":\"drs\",\"function\":\"tapeled_get\",\"params\":[]}"));
    }

    public void setTouch(List<TouchCommand> touchCommands)
    {
        if (touchCommands.Count == 0)
            return;

        if (wsPeer.GetReadyState() == WebSocketPeer.State.Open)
        {
            string touchCommandString = "{\"id\":1,\"module\":\"drs\",\"function\":\"touch_set\",\"params\":[";
            foreach (var touchCommand in touchCommands)
            {
                touchCommandString += "[" + touchCommand.touchEvent + "," + touchCommand.sensorId + "," + touchCommand.touchPosition.X.ToString("0.0000") + "," + touchCommand.touchPosition.Y.ToString("0.0000") + "," + touchCommand.touchSize.X + "," + touchCommand.touchSize.Y + "],";
            }
            touchCommandString = touchCommandString.Remove(touchCommandString.Length - 1);
            touchCommandString += "]}";
            wsPeer.Send(Encoding.UTF8.GetBytes(touchCommandString));
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Connecting WebSocket...");
        wsPeer.ConnectToUrl(_websocket_url);
        wsPeer.EncodeBufferMaxSize = 16 * 1024 * 1024;
        wsPeer.InboundBufferSize = 16 * 1024 * 1024;
        wsPeer.OutboundBufferSize = 16 * 1024 * 1024;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        lightingGetTimer += delta;
        wsPeer.Poll();
        var state = wsPeer.GetReadyState();
        switch (state)
        {
            case WebSocketPeer.State.Open:
                if (lightingGetTimer >= lightingGetInterval)
                {
                    getLighting();
                    lightingGetTimer = 0;
                }

                for (int i = 0; i < wsPeer.GetAvailablePacketCount(); i++)
                {
                    string packetText = Encoding.UTF8.GetString(wsPeer.GetPacket());
                    if (packetText.Contains("id"))
                    {
                        var dict = Json.ParseString(packetText).AsGodotDictionary();
                        if (dict.ContainsKey("data") && dict["id"].AsInt32() == 0)
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
                var code = wsPeer.GetCloseCode();
                var reason = wsPeer.GetCloseReason();
                GD.Print("WebSocket closed with code: %d, reason %s. Clean: %s", code, reason, code != -1);

                SetProcess(false);
                break;
        }
    }
}
