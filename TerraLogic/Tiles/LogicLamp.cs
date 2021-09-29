﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TerraLogic.Tiles
{
    class LogicLamp : Tile
    {
        public override string Id => "logicLamp";
        public override string[] PreviewVariants => new string[] { "+", "-", "?" };

        public override string DisplayName => $"Logic Gate Lamp ({State})";

        static Texture2D On, Off, Faulty;

        internal LampState State;

        public override void Draw(Rectangle rect, bool isScreenPos = false)
        {
            TerraLogic.SpriteBatch.Draw(
                (State == LampState.On)? On : (State == LampState.Faulty || State == LampState.FaultyTriggered)? Faulty : Off, 
                isScreenPos ? rect : PanNZoom.WorldToScreen(rect), Color.White);
        }

        public override void PlacedInWorld()
        {
            int scanPos = Pos.Y;
            while (Gui.Logics.TileArray[Pos.X, scanPos] is LogicLamp) scanPos--;
            scanPos++;

            List<bool> lamps = new List<bool>();
            bool foundFaulty = false;
            bool faultyTriggered = false;

            while (Gui.Logics.TileArray[Pos.X, scanPos] is LogicLamp)
            {
                LogicLamp lamp = Gui.Logics.TileArray[Pos.X, scanPos] as LogicLamp;
                switch (lamp.State) 
                {
                    case LampState.Off: lamps.Add(false); break;
                    case LampState.On: lamps.Add(true); break;
                    case LampState.Faulty: foundFaulty = true; break;
                    case LampState.FaultyTriggered: foundFaulty = true; faultyTriggered = true; break;
                }
                scanPos++;
            }

            if (Gui.Logics.TileArray[Pos.X, scanPos] is LogicGate lg) lg.LampStateChanged(lamps.ToArray(), foundFaulty, faultyTriggered);
        }

        public override void LoadContent(ContentManager content)
        {
            Off = content.Load<Texture2D>("Tiles/LogicLampOff");
            On = content.Load<Texture2D>("Tiles/LogicLampOn");
            Faulty = content.Load<Texture2D>("Tiles/LogicLampFaulty");
        }

        public override void WireSignal(int wire, Point origin)
        {
            switch (State) 
            {
                case LampState.On: State = LampState.Off; break;
                case LampState.Off: State = LampState.On; break;
                case LampState.Faulty: State = LampState.FaultyTriggered; break;
            }
        }

        internal override Tile CreateTile(string data, bool preview)
        {
            return new LogicLamp() { State = (data == "+") ? LampState.On : (data == "?") ? LampState.Faulty : LampState.Off };
        }

        public override void BeforeDestroy()
        {
            int scanPos = Pos.Y+1;

            List<bool> lamps = new List<bool>();
            bool foundFaulty = false;
            bool faultyTriggered = false;

            while (Gui.Logics.TileArray[Pos.X, scanPos] is LogicLamp)
            {
                LogicLamp lamp = Gui.Logics.TileArray[Pos.X, scanPos] as LogicLamp;
                switch (lamp.State)
                {
                    case LampState.Off: lamps.Add(false); break;
                    case LampState.On: lamps.Add(true); break;
                    case LampState.Faulty: foundFaulty = true; break;
                    case LampState.FaultyTriggered: foundFaulty = true; faultyTriggered = true; break;
                }
                scanPos++;
            }

            if (Gui.Logics.TileArray[Pos.X, scanPos] is LogicGate lg) lg.LampStateChanged(lamps.ToArray(), foundFaulty, faultyTriggered);

        }

        internal override string GetData()
        {
            return (State == LampState.On) ? "+" : (State == LampState.Faulty) ? "?" : "-";
        }

        internal enum LampState { On, Off, Faulty, FaultyTriggered }
    }
}