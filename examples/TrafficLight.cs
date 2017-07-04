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

public class TrafficLight : Script
{

    // Maximum distance an entity can be from the camera 
    float maxAnnotationRange = 70f;

    int trafficLightState = 0;
    int lastControlledTrafficeLight = 0;

    public TrafficLight()
    {
        UI.Notify("Loaded TrafficLight.cs");

        // attach time methods 
        Tick += OnTick;
    }

    void OnTick(object sender, EventArgs e)
    {
        Prop[] props = World.GetNearbyProps(GameplayCamera.Position, maxAnnotationRange);

        foreach (Prop b in props)
        {
            if (b.IsOnScreen && !b.IsOccluded && b.GetHashCode() != lastControlledTrafficeLight)
            {
                switch ((uint)b.Model.Hash)
                {
                    case 0x3E2B73A4: // prop_traffic_01a
                    case 0x336E5E2A: // prop_traffic_01b
                    case 0xD8EBA922: // prop_traffic_01d
                    //case 0xD4729F50: // prop_traffic_02a
                    //case 0x272244B2: // prop_traffic_02b
                    case 0x33986EAE: // prop_traffic_03a
                    //case 0x2323CDC5: // prop_traffic_03b
                        Vector3 propFront = Vector3.Cross(b.UpVector, b.RightVector);
                        Vector3 propPos = b.Position;
                        Vector3 camPos = GameplayCamera.Position;
                        Vector3 camDir = GameplayCamera.Direction;

                        float ang = Vector3.Dot((propPos - camPos), camDir);
                        float faceToCam = Vector3.Dot(propFront, camDir);

                        if ((ang > 0 && ang < 15) && faceToCam > 0)
                        {
                            Random r = new Random();
                            trafficLightState = r.Next(0, 3);
                            b.SetTrafficLight(trafficLightState);
                            lastControlledTrafficeLight = b.GetHashCode();
                            string[] color = { "green", "red", "yellow" };
                            UI.Notify("set traffic light to " + color[trafficLightState]);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}