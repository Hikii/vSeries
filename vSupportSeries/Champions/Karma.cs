using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using vSupport_Series.Core.Plugins;
using Color = System.Drawing.Color;
using Orbwalking = vSupport_Series.Core.Plugins.Orbwalking;

namespace vSupport_Series.Champions
{
    public class Karma : Helper
    {
        public static Menu Config;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;

        private static Obj_AI_Hero Player = ObjectManager.Player;

        public Karma()
        {
            KarmaOnLoad();
        }

        public static void KarmaOnLoad()
        {
            Q = new Spell(SpellSlot.Q, 950f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R);

            E.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, 2200f);
            E.SetTargetted(0.25f, float.MaxValue);

            Config = new Menu("vSupport Series:  " + ObjectManager.Player.ChampionName, "vSupport Series", true);
            {
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

                var comboMenu = new Menu("Combo Settings", "Combo Settings");
                {
                    comboMenu.AddItem(new MenuItem("karma.q.combo", "Use Q").SetValue(true));
                    comboMenu.AddItem(new MenuItem("karma.w.combo", "Use W").SetValue(true));
                    comboMenu.AddItem(new MenuItem("karma.e.combo", "Use E").SetValue(true));
                    var esettings = new Menu(":: E Settings", ":: E Settings");
                    {
                        esettings.AddItem(new MenuItem("combo.e.ally", "Shield is Ally HP").SetValue(new Slider(40, 1, 99)));
                        esettings.AddItem(new MenuItem("combo.e.self", "Shield is Player HP").SetValue(new Slider(30, 1, 99)));
                    }
                    comboMenu.AddItem(new MenuItem("karma.r.combo", "Use R").SetValue(true));
                    
                    var rsettings = new Menu(":: R Settings", ":: R Settings");
                    {
                        rsettings.AddItem(new MenuItem("combo.r.q", "Empower Q?").SetValue(true));
                        rsettings.AddItem(new MenuItem("combo.r.w", "Empower W?").SetValue(true));
                        rsettings.AddItem(new MenuItem("combo.r.w.health", "Min. Health to Empower W").SetValue(new Slider(40, 1, 99)));
                        rsettings.AddItem(new MenuItem("combo.r.e", "Empower E?").SetValue(true));
                        rsettings.AddItem(new MenuItem("combo.r.e.allies", "Min. Allies in Range to Shield").SetValue(new Slider(3, 1, 5)));
                        
                        comboMenu.AddSubMenu(rsettings);
                    }

                    Config.AddSubMenu(comboMenu);
                }

                var harass = new Menu("Harass Settings", "Harass Settings");
                {
                    harass.AddItem(new MenuItem("karma.q.harass", "Use Q").SetValue(true));
                    harass.AddItem(new MenuItem("karma.rq.harass", "Empower R?").SetValue(true));
                    harass.AddItem(new MenuItem("karma.e.harass", "Use E").SetValue(true));
                    harass.AddItem(new MenuItem("karma.harass.mana", "Min. Mana Percent").SetValue(new Slider(50, 1, 99)));

                    Config.AddSubMenu(harass);
                }

                var misc = new Menu("Miscellaneous", "Miscellaneous");
                {
                    misc.AddItem(new MenuItem("karma.anti.q", "Gapcloser (Q)").SetValue(true));
                    misc.AddItem(new MenuItem("karma.anti.e", "Gapcloser (E)").SetValue(true));

                    Config.AddSubMenu(misc);
                }

                var drawing = new Menu("Draw Settings", "Draw Settings");
                {
                    drawing.AddItem(new MenuItem("karma.q.draw", "Q Range").SetValue(new Circle(true, Color.Chartreuse)));
                    drawing.AddItem(new MenuItem("karma.w.draw", "W Range").SetValue(new Circle(true, Color.Yellow)));
                    drawing.AddItem(new MenuItem("karma.e.draw", "E Range").SetValue(new Circle(true, Color.White)));

                    Config.AddSubMenu(drawing);
                }

                Config.AddItem(new MenuItem("karma.q.hitchance", "Skillshot Hit Chance").SetValue(new StringList(HitchanceNameArray, 2)));
            }

            SPrediction.Prediction.Initialize(Config, ":: Prediction Settings");
            Config.AddToMainMenu();
            Game.OnUpdate += KarmaOnUpdate;
            Drawing.OnDraw += KarmaOnDraw;
            AntiGapcloser.OnEnemyGapcloser += KarmaOnEnemyGapcloser;
        }

        private static void KarmaOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.IsReady() && MenuCheck("karma.anti.q", Config) && gapcloser.Sender.IsValidTarget(Q.Range))
            {
                Q.Cast(gapcloser.Sender);
            }
            else if (E.IsReady() && MenuCheck("karma.anti.e", Config) && gapcloser.Sender.IsValidTarget(E.Range))
            {
                E.Cast(gapcloser.Sender);
            }
        }

        private static void KarmaOnUpdate(EventArgs args)
        {
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
            if (MenuCheck("karma.e.combo", Config) && E.IsReady())
            {
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && !x.IsMe))
                {
                    if (MenuCheck("combo.r.e", Config) && R.IsReady())
                    {
                        if (Player.CountAlliesInRange(E.Range) > SliderCheck("combo.r.e.allies", Config))
                        {
                            R.Cast();
                            E.CastOnUnit(Player);
                        }
                    }
                    else if (!MenuCheck("combo.r.e", Config))
                    {
                        if (ally.HealthPercent <= SliderCheck("combo.e.ally", Config))
                        {
                            E.CastOnUnit(ally);
                        }
                        
                        if (Player.HealthPercent <= SliderCheck("combo.e.self", Config))
                        {
                            E.CastOnUnit(Player);
                        }
                    }
                }
            }
            else if (MenuCheck("karma.w.combo", Config) && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range)))
                {
                    if (MenuCheck("combo.r.w", Config) && R.IsReady())
                    {
                        if (ObjectManager.Player.HealthPercent <= SliderCheck("combo.w.e.health", Config))
                        {
                            R.Cast();
                            W.CastOnUnit(enemy);
                        }
                    }
                    else if (!MenuCheck("combo.r.w", Config))
                    {
                        W.CastOnUnit(enemy);
                    }
                }
            }
            else if (MenuCheck("karma.q.combo", Config) && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range)))
                {
                    if (MenuCheck("combo.r.q", Config) && R.IsReady())
                    {
                        R.Cast();
                        Q.SPredictionCast(enemy, SpellHitChance(Config, "karma.q.hitchance"));
                    }
                    else if (!MenuCheck("combo.r.q", Config))
                    {
                        Q.SPredictionCast(enemy, SpellHitChance(Config, "karma.q.hitchance"));
                    }
                }
            }
        }

        private static void Harass()
        {
            if (MenuCheck("karma.rq.harass", Config) && Q.IsReady() && R.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range)))
                {
                    R.Cast();
                    Q.SPredictionCast(enemy, SpellHitChance(Config, "karma.q.hitchance"));
                }
            }
            else if (MenuCheck("karma.q.harass", Config) && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range)))
                {
                    Q.SPredictionCast(enemy, SpellHitChance(Config, "karma.q.hitchance"));
                }
            }

            if (MenuCheck("karma.w.harass", Config) && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range)))
                {
                    W.CastOnUnit(enemy);
                }
            }
        }

        private static void KarmaOnDraw(EventArgs args)
        {
            if (Q.IsReady() && ActiveCheck("karma.q.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, GetColor("karma.q.draw", Config));
            }

            if (W.IsReady() && ActiveCheck("karma.w.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, GetColor("karma.w.draw", Config));
            }

            if (E.IsReady() && ActiveCheck("karma.e.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, GetColor("karma.e.draw", Config));
            }
        }
    }
}