using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Azir
{
    class MenuConfig
    {
        public static Menu config;

        public static string menuName = "Assembly Name Here";


        // private static Menu TargetSelectorMenu;
        private static Orbwalking.Orbwalker Orbwalker;

        public static void LoadMenu()
        {
            config = new Menu(menuName, menuName, true);

            /*
            TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            config.AddSubMenu(TargetSelectorMenu);
            */

            Orbwalker = new Orbwalking.Orbwalker(config.SubMenu("Orbwalker"));
            config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));

            var comboMenu = new Menu("Combo", "# Combo"); {
                comboMenu.AddItem(new MenuItem("Combo.Q.Use", "Use Q")).SetValue(true);
                comboMenu.AddItem(new MenuItem("Combo.Q.MaxRng", "Use Q Max Range")).SetValue(true);
                comboMenu.AddItem(new MenuItem("Combo.W.Use", "Use W")).SetValue(true);
                comboMenu.AddItem(new MenuItem("Combo.E.Use", "Use E")).SetValue(false);
                comboMenu.AddItem(new MenuItem("Combo.R.Use", "Use R")).SetValue(true);
                comboMenu.AddItem(new MenuItem("--", "- Extras"));
                comboMenu.AddItem(new MenuItem("Combo.All_In", "All-In")).SetValue(new KeyBind('A', KeyBindType.Press));
                comboMenu.AddItem(new MenuItem("Special", "Special Feature"));
                comboMenu.AddItem(new MenuItem("EliteCombo", "Elite Combo")).SetValue(new KeyBind('G', KeyBindType.Press));
                comboMenu.AddItem(new MenuItem("Dont", "Don't Change Elite Key"));

                config.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass", "# Harass"); {
                harassMenu.AddItem(new MenuItem("Harass.Q.Use", "Use Q")).SetValue(true);
                harassMenu.AddItem(new MenuItem("Harass.W.Use", "Use W")).SetValue(true);
                harassMenu.AddItem(new MenuItem("Harass.Mana", "Minimum Mana %")).SetValue(new Slider(40, 1, 99));

                config.AddSubMenu(harassMenu);
            }

            var laneMenu = new Menu("Lane", "# Lane/Jungle Clear"); {
                laneMenu.AddItem(new MenuItem("Lane.Q.Use", "Use Q")).SetValue(true);
                laneMenu.AddItem(new MenuItem("Lane.W.Use", "Use W")).SetValue(true);
                laneMenu.AddItem(new MenuItem("Lane.Min.Minions", "Minions to Summon Soldier")).SetValue(new Slider(3, 1, 6));
                laneMenu.AddItem(new MenuItem("Lane.Mana", "Minimum Mana %")).SetValue(new Slider(40, 1, 99));

                config.AddSubMenu(laneMenu);
            }

            var lasthitMenu = new Menu("Lasthit", "# Lasthit"); {
                // add stuff

                config.AddSubMenu(lasthitMenu);
            }

            var killstealMenu = new Menu("Killsteal", "# Killsteal"); {
                killstealMenu.AddItem(new MenuItem("Killsteal.Q.Use", "Use Q")).SetValue(true);
            }

            var interruptMenu = new Menu("Interrupt", "# Interrupt"); {
                interruptMenu.AddItem(new MenuItem("Interrupt.R.Use", "Use R")).SetValue(false);

                config.AddSubMenu(interruptMenu);
            }

            var miscMenu = new Menu("Misc", "# Misc"); {
                miscMenu.AddItem(new MenuItem("Escape", "W E Q Escape")).SetValue(new KeyBind('Z', KeyBindType.Press));

                config.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("Drawing", "# Drawings"); {
                drawMenu.AddItem(new MenuItem("DisableAll", "Disable all Drawings")).SetValue(false);
                drawMenu.AddItem(new MenuItem("DrawDamage", "Draw Damage")).SetValue(true);

                drawMenu.AddItem(new MenuItem("Draw.Q", "Draw Q")).SetValue(new Circle(true, Color.Blue));
                drawMenu.AddItem(new MenuItem("Draw.W", "Draw W")).SetValue(new Circle(true, Color.Blue));
                drawMenu.AddItem(new MenuItem("Draw.E", "Draw E")).SetValue(new Circle(true, Color.Blue));
                drawMenu.AddItem(new MenuItem("Draw.R", "Draw R")).SetValue(new Circle(true, Color.Blue));

                config.AddSubMenu(drawMenu);
            }

            config.AddItem(new MenuItem("Developed", "Developed by Veto"));
            config.AddItem(new MenuItem("Bugs", "Report bugs to me!"));

            config.AddToMainMenu();
        }
    }
}
