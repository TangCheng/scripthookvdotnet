using System;
using System.Drawing;
using System.Collections.Generic;
using GTA;
using GTA.Math;

public class Plate : Script
{
    // Maximum distance an entity can be from the camera 
    float maxAnnotationRange = 20f;

    const int PANEL_WIDTH = 900;
    const int PANEL_HEIGHT = 20;
    Color backColor = Color.FromArgb(100, 255, 255, 255);
    Color textColor = Color.Black; // just change this to whatever color you want

    List<UIContainer> containers = new List<UIContainer>();
    List<UIText> texts = new List<UIText>();
    int container_cnt = 1;

    public Plate()
    {
        UI.Notify("Loaded Plate.cs");

        for (int i = 0; i < container_cnt; i++)
        {
            UIContainer container = new UIContainer(new Point(UI.WIDTH / 2 - PANEL_WIDTH / 2, PANEL_HEIGHT * i), new Size(PANEL_WIDTH, PANEL_HEIGHT), backColor);
            UIText text = new UIText("", new Point(PANEL_WIDTH / 2, 0), 0.42f, textColor, GTA.Font.Pricedown, true);
            container.Items.Add(text);
            containers.Add(container);
            texts.Add(text);
        }

        // attach time methods 
        Tick += OnTick;
    }

    void OnTick(object sender, EventArgs e)
    {
        Vehicle[] vehicles = World.GetNearbyVehicles(GameplayCamera.Position, maxAnnotationRange);
        Vector3 camPos = GameplayCamera.Position;
        Vector3 camDir = GameplayCamera.Direction;

        if (vehicles.Length != 0)
        {
            float angleMinimal = 180.0f;
            Vehicle nearest = null;
            foreach (Vehicle v in vehicles)
            {
                if (v != Game.Player.Character.CurrentVehicle)
                {
                    Vector3 vehiclePos = v.Position;
                    float ang = Vector3.Dot((vehiclePos - camPos), camDir);
                    if (ang > 0.0f && ang < angleMinimal)
                    {
                        if (v.IsOnScreen)
                        {
                            nearest = v;
                            angleMinimal = ang;
                        }
                    }
                }
            }
            
            bool left_indicator_status = false;
            bool right_indicator_status = false;
            string plate = "";
            if (nearest != null)
            {
                plate = nearest.NumberPlate;
            }
            texts[0].Caption = String.Format("License plate: {0}, Left Indicator: {1}, Right Indicator: {2}",
                plate,
                left_indicator_status,
                right_indicator_status);
        }
        else
        {
            texts[0].Caption = "No car.";
        }
        // draw
        foreach (UIContainer container in containers)
        {
            container.Draw();
        }
    }
}