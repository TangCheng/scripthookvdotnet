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
    const float PI = 3.14159f;
    struct ImuData
    {
        public ulong timestamp;

        // Batch - 1
        public ushort time_ms;                 // milliseconds into the minute in GPS time. (0~59999)

        public double acc_x;                   // acceleration X in vehicle body frame. unit: m/s^2
        public double acc_y;                   // acceleration Y in vehicle body frame. unit: m/s^2
        public double acc_z;                   // acceleration Z in vehicle body frame. unit: m/s^2

        public double ang_x;                   // angular rate X in vehicle body frame. unit: rad/s
        public double ang_y;                   // angular rate Y in vehicle body frame. unit: rad/s
        public double ang_z;                   // angular rate Z in vehicle body frame. unit: rad/s

        public byte nav_status;                // navigation status

        // Batch - 2
        public double latitude;                // latitude of the IMU. unit: rad
        public double longitude;               // longitude of the IMU. unit: rad
        public float altitude;                 // altitude of the IMU. unit: m

        public double vel_n;                   // north velocity. unit: m/s
        public double vel_e;                   // east velocity. unit: m/s
        public double vel_d;                   // down velocity. unit: m/s

        public double heading;                 // heading. unit: rad (-PI~PI)
        public double pitch;                   // pitch. unit: rad (-PI/2~PI/2)
        public double roll;                    // roll. unit: rad (-PI~PI)

        // Batch - 3
        public byte channel;                   // determine what information is sent
        // Channel 0
        public uint time_mm;                   // time in minutes since GPS began (midnight 06/01/1980). invalid when value < 1000
        public byte satellites;                // number of GPS satellites tracked by the primary GPS receiver. invalid when value = 255
        public byte pos_mode;                  // position mode of primary GPS. invalid when value = 255
                                               //byte vel_mode;                // velocity mode of primary GPS. invalid when value = 255
                                               //byte orien_mode;              // orientation mode of dual antenna systems. invalid when value = 255
                                               // Channel 3
        public ushort n_pos_accuracy;          // north postion accuracy. valid when age < 150. unit : mm
        public ushort e_pos_accuracy;          // east postion accuracy. valid when age < 150. unit : mm
        public ushort d_pos_accuracy;          // down postion accuracy. valid when age < 150. unit : mm
        public byte age3;                      // age
        // Channel 4
        public ushort n_vel_accuracy;          // valid when age < 150. unit : mm/s
        public ushort e_vel_accuracy;          // valid when age < 150. unit : mm/s
        public ushort d_vel_accuracy;          // valid when age < 150. unit : mm/s
        public byte age4;                      // age
        // Channel 5
        public ushort heading_accuracy;        // valid when age < 150. unit : 1e-5 rad
        public ushort pitch_accuracy;          // valid when age < 150. unit : 1e-5 rad
        public ushort roll_accuracy;           // valid when age < 150. unit : 1e-5 rad
        public byte age5;                      // age
        // Channel 6
        public short gyro_bias_x;              // valid when age < 150. unit : 5e-6 rad
        public short gyro_bias_y;              // valid when age < 150. unit : 5e-6 rad
        public short gyro_bias_z;              // valid when age < 150. unit : 5e-6 rad
        public byte age6;                      // age
        // Channel 7
        public short acc_bias_x;               // valid when age < 150. unit : 0.1 mm/s^2
        public short acc_bias_y;               // valid when age < 150. unit : 0.1 mm/s^2
        public short acc_bias_z;               // valid when age < 150. unit : 0.1 mm/s^2
        public byte age7;                      // age
    };

    const int PANEL_WIDTH = 900;
    const int PANEL_HEIGHT = 20;
    Color backColor = Color.FromArgb(100, 255, 255, 255);
    Color textColor = Color.Black; // just change this to whatever color you want

    List<UIContainer> containers = new List<UIContainer>();
    List<UIText> texts = new List<UIText>();
    int container_cnt = 5;

    ImuData imu_data;

    public GPS()
    {
        UI.Notify("Loaded GPS.cs");

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
        Player player = Game.Player;
        if (player != null && player.CanControlCharacter && player.IsAlive && player.Character != null)
        {
            // get coords
            Vector3 pos = player.Character.Position;
            float heading = player.Character.Heading;
            DateTime date = World.CurrentDate;
            float speed = 0.0f;
            Vector3 velocity = player.Character.Velocity;
            Vector3 rotation_velocity = player.Character.RotationVelocity;
            Vehicle v = Game.Player.Character.CurrentVehicle;
            if (v != null)
            {
                speed = v.Speed;
                speed *= 3.6f;
                velocity = v.Velocity;
                rotation_velocity = v.RotationVelocity;
            }

            imu_data.timestamp = (ulong)date.Ticks;
            imu_data.time_ms = (ushort)date.Millisecond;

            imu_data.ang_x = rotation_velocity.X;
            imu_data.ang_y = rotation_velocity.Y;
            imu_data.ang_z = rotation_velocity.Z;

            imu_data.latitude = pos.X;
            imu_data.longitude = pos.Y;
            imu_data.altitude = pos.Z;

            imu_data.acc_x = (Math.Abs(velocity.X) - Math.Abs(imu_data.vel_e)) / 1.0f;
            imu_data.acc_y = (Math.Abs(velocity.Y) - Math.Abs(imu_data.vel_n)) / 1.0f;
            imu_data.acc_z = (Math.Abs(velocity.Z) - Math.Abs(imu_data.vel_d)) / 1.0f;

            imu_data.vel_e = velocity.X;
            imu_data.vel_n = velocity.Y;
            imu_data.vel_d = velocity.Z;

            imu_data.heading = PI - (player.Character.Heading * PI / 180);
            imu_data.pitch = player.Character.Pitch * PI / 180;
            imu_data.roll = player.Character.Roll * PI / 180;

            texts[0].Caption = String.Format("{0}    x:{1}    y:{2}    z:{3}    velocity:{4}", 
                date.ToString(), 
                pos.X.ToString("0.000"),
                pos.Y.ToString("0.000"), 
                pos.Z.ToString("0.000"), 
                speed.ToString("0.00 km/h"));
            texts[1].Caption = String.Format("Heading:{0}    Pitch:{1}    Roll:{2}",
                imu_data.heading.ToString("0.00"),
                imu_data.pitch.ToString("0.00"),
                imu_data.roll.ToString("0.00"));
            texts[2].Caption = String.Format("vel_e:{0}    vel_n:{1}    vel_d:{2}    speed:{3}",
                imu_data.vel_e.ToString("0.00"),
                imu_data.vel_n.ToString("0.00"),
                imu_data.vel_d.ToString("0.00"),
                (velocity.Length() * 3.6f).ToString("0.00"));
            texts[3].Caption = String.Format("ang_x:{0}    ang_y:{1}    ang_z:{2}",
                imu_data.ang_x.ToString("0.00"),
                imu_data.ang_y.ToString("0.00"),
                imu_data.ang_z.ToString("0.00"));
            texts[4].Caption = String.Format("acc_x:{0}    acc_y:{1}    acc_z:{2}",
                imu_data.acc_x.ToString("0.00"),
                imu_data.acc_y.ToString("0.00"),
                imu_data.acc_z.ToString("0.00"));
            // draw
            //foreach (UIContainer container in containers)
            //{
            //    container.Draw();
            //}
        }
    }
}