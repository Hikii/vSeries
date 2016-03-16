using System;
using LeagueSharp;
using LeagueSharp.Common;
using vSupport_Series.Champions;

namespace vSupport_Series
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Alistar":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Alistar();
                    break;
                case "Blitzcrank":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Blitzcrank();
                    break;
                case "FiddleSticks":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Fiddlesticks();
                    break;
                case "Janna":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Janna();
                    break;
                case "Leona":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Leona();
                    break;
                case "Lux":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Lux();
                    break;
                case "Morgana":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Morgana();
                    break;
                case "Nami":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Nami();
                    break;
                case "Nautilus":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Nautilus();
                    break;
                case "Sona":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Sona();
                    break;
                case "Soraka":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Soraka();
                    break;
                case "Syndra":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Syndra();
                    break;
                case "Taric":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Taric();
                    break;
                case "Thresh":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Thresh();
                    break;
                case "Trundle":
                    // ReSharper disable once ObjectCreationAsStatement
                    new Trundle();
                    break;
            }
            
        }
    }
}
