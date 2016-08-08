using System;
using LeagueSharp;
using LeagueSharp.Common;
using vSupport_Series.Champions;
using vSupport_Series.Core.Activator;
using vSupport_Series.Core.Plugins;

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
                    new Alistar();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Blitzcrank":
                    new Blitzcrank();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "FiddleSticks":
                    new Fiddlesticks();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Janna":
                    new Janna();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Karma":
                    new Karma();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Leona":
                    new Leona();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Lux":
                    new Lux();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Morgana":
                    new Morgana();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Nami":
                    new Nami();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Nautilus":
                    new Nautilus();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Poppy":
                    new Poppy();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Sona":
                    new Sona();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Soraka":
                    new Soraka();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Syndra":
                    new Syndra();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Taric":
                    new Taric();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Thresh":
                    new Thresh();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
                case "Trundle":
                    new Trundle();
                    new ActivatorBase();
                    VersionCheck.UpdateCheck();
                    break;
            }
            
        }
    }
}
