﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using SharpDX;
using vSupport_Series.Core.Plugins;
using Color = System.Drawing.Color;
using Orbwalking = vSupport_Series.Core.Plugins.Orbwalking;

namespace vSupport_Series.Champions
{
    public class TahmKench : Helper
    {
        public static Menu Config;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;

        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static string Passive = "TahmKenchPDebuffCounter";

        public enum Swallowed
        {
            Ally,
            Enemy,
            Minion,
            None
        }

        public TahmKench()
        {
            TahmKenchOnLoad();
        }
        public static void Spells()
        {
            Q = new Spell(SpellSlot.Q, 800f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 3500 + 1000 * Player.Spellbook.GetSpell(SpellSlot.R).Level);

            Q.SetSkillshot(0.25f, 70f, 1700f, true, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, 2200f);
            E.SetTargetted(0.25f, float.MaxValue);
        }
        public static void TahmKenchOnLoad()
        {
            Spells();

            Config = new Menu("vSupport Series:  " + ObjectManager.Player.ChampionName, "vSupport Series", true);
            {
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

                var comboMenu = new Menu("Combo Settings", "Combo Settings");
                {
                    comboMenu.AddItem(new MenuItem("tahm.q.combo", "Use Q").SetValue(true));
                    comboMenu.AddItem(new MenuItem("tahm.w.combo", "Use W").SetValue(true));
                    comboMenu.AddItem(new MenuItem("tahm.w.minion", "Use W on Minion to Stack").SetValue(true));
                    Config.AddSubMenu(comboMenu);
                }

                var harass = new Menu("Harass Settings", "Harass Settings");
                {
                    harass.AddItem(new MenuItem("tahm.q.harass", "Use Q").SetValue(true));
                    harass.AddItem(new MenuItem("tahm.w.harass", "Use W (uses Minions)").SetValue(true));
                    harass.AddItem(new MenuItem("tahm.harass.mana", "Min. Mana Percent").SetValue(new Slider(50, 1, 99)));

                    Config.AddSubMenu(harass);
                }

                var misc = new Menu("Miscellaneous", "Miscellaneous");
                {
                    misc.AddItem(new MenuItem("tahm.anti.q", "Gapcloser (Q)").SetValue(true));

                    Config.AddSubMenu(misc);
                }

                var drawing = new Menu("Draw Settings", "Draw Settings");
                {
                    drawing.AddItem(new MenuItem("tahm.q.draw", "Q Range").SetValue(new Circle(true, Color.Chartreuse)));
                    drawing.AddItem(new MenuItem("tahm.w.draw", "W Range").SetValue(new Circle(true, Color.Yellow)));
                    drawing.AddItem(new MenuItem("tahm.r.draw", "R Range").SetValue(new Circle(true, Color.SandyBrown)));

                    Config.AddSubMenu(drawing);
                }

                Config.AddItem(new MenuItem("tahm.hitchance", "Skillshot Hit Chance").SetValue(new StringList(HitchanceNameArray, 2)));
                Config.AddItem(new MenuItem("prediction", ":: Choose Prediction").SetValue(new StringList(new[] { "Common", "Sebby", "sPrediction" }, 1)))
                    .ValueChanged += (s, ar) =>
                    {
                        Config.Item("pred.info").Show(ar.GetNewValue<StringList>().SelectedIndex == 2);
                    };
                Config.AddItem(new MenuItem("pred.info", "                 PRESS F5 FOR LOAD SPREDICTION").SetFontStyle(System.Drawing.FontStyle.Bold)).Show(Config.Item("prediction").GetValue<StringList>().SelectedIndex == 0);
                if (Config.Item("prediction").GetValue<StringList>().SelectedIndex == 2)
                {
                    SPrediction.Prediction.Initialize(Config, ":: sPrediction Settings");
                }
            }

            SPrediction.Prediction.Initialize(Config, ":: Prediction Settings");
            Config.AddToMainMenu();
            Game.OnUpdate += TahmKenchOnUpdate;
            Drawing.OnDraw += TahmKenchOnDraw;
            AntiGapcloser.OnEnemyGapcloser += TahmKenchOnEnemyGapcloser;
        }

        private static void TahmKenchOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.IsReady() && MenuCheck("tahm.anti.q", Config) && gapcloser.Sender.IsValidTarget(Q.Range))
            {
                Q.Cast(gapcloser.Sender);
            }
        }

        private static void TahmKenchOnUpdate(EventArgs args)
        {
            Spells();
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        private static void Combo()
        {
            // Switch cases need adding for smarter passive control
        }

        private static void Harass()
        {
            if (Player.ManaPercent < SliderCheck("tahm.harass.mana", Config))
            {
                return;
            }

            if (MenuCheck("tahm.q.harass", Config) && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range)))
                {
                    Q.vCast(enemy, SpellHitChance(Config, "q.hit.chance"), "prediction", Config);
                }
            }

            if (MenuCheck("tahm.w.harass", Config) && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(650)))
                {
                    // Swallowed logic needs adding
                    var minion = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsEnemy && x.Distance(Player, true) < 250).FirstOrDefault();
                    W.CastOnUnit(minion);

                    Utility.DelayAction.Add(
                        100,
                        () =>
                        {
                            W.SPredictionCast(enemy, SpellHitChance(Config, "tahm.hitchance"));
                        }
                    );
                }
            }
        }

        private static void AutoShield()
        {
            // Auto shielding to give infinite health :kappa:
        }

        private static void TahmKenchOnDraw(EventArgs args)
        {
            if (Q.IsReady() && ActiveCheck("tahm.q.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, GetColor("tahm.q.draw", Config));
            }

            if (W.IsReady() && ActiveCheck("tahm.w.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, GetColor("tahm.w.draw", Config));
            }

            if (R.IsReady() && ActiveCheck("tahm.r.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, GetColor("tahm.r.draw", Config));
            }
        }
    }
}
