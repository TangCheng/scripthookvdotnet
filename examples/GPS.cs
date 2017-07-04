using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class GPS : Script
{
    const int PANEL_WIDTH = 900;
    const int PANEL_HEIGHT = 20;
    Color backColor = Color.FromArgb(100, 255, 255, 255);
    Color textColor = Color.Black; // just change this to whatever color you want

    UIContainer container;
    UIText text;

    string coordStr = "";
    string nameStr = "";
    bool enteringText = false;
    UIText nameText;
    const int controlIndex = 1;

    public GPS()
    {
        UI.Notify("Loaded GPS.cs");

        container = new UIContainer(new Point(UI.WIDTH / 2 - PANEL_WIDTH / 2, 0), new Size(PANEL_WIDTH, PANEL_HEIGHT), backColor);
        text = new UIText("", new Point(PANEL_WIDTH / 2, 0), 0.42f, textColor, GTA.Font.Pricedown, true);
        container.Items.Add(text);

        // attach time methods 
        Tick += OnTick;
    }

    void OnTick(object sender, EventArgs e)
    {
        Player player = Game.Player;
        if (player != null && player.CanControlCharacter && player.IsAlive && player.Character != null)
        {
            // get coords
            Vector3 pos = player.Character.Position;
            float heading = player.Character.Heading;
            DateTime date = World.CurrentDate;
            float speed = 0.0f;
            Vehicle v = Game.Player.Character.CurrentVehicle;
            if (v != null)
            {
                speed = v.Speed;
                speed *= 3.6f;
            }

            text.Caption = String.Format("{0}    x:{1}    y:{2}    z:{3}    angle:{4}    velocity:{5}", 
                date.ToString(), 
                pos.X.ToString("0.000"),
                pos.Y.ToString("0.000"), 
                pos.Z.ToString("0.000"), 
                heading.ToString("0.000"), 
                speed.ToString("0.00 km/h"));
            // draw
            container.Draw();
        }
    }
}