using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using vSupport_Series.Core.Plugins;
using Color = System.Drawing.Color;

namespace vSupport_Series.Champions
{
    public class Nami : Helper
    {
        public static Menu Config;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;

        public Nami()
        {
            NamiOnLoad();
        }

        private static void NamiOnLoad()
        {
            Q = new Spell(SpellSlot.Q, 875);
            W = new Spell(SpellSlot.W, 725);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 2750);

            Q.SetSkillshot(1f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 260f, 850f, false, SkillshotType.SkillshotLine);
            Config = new Menu("vSupport Series: " + ObjectManager.Player.ChampionName, "vSupport Series", true);
            {
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));
                var comboMenu = new Menu("Combo Settings", "Combo Settings");
                {
                    comboMenu.AddItem(new MenuItem("nami.q.combo", "Use Q").SetValue(true));
                    comboMenu.AddItem(new MenuItem("nami.w.combo", "Use W").SetValue(true));
                    comboMenu.AddItem(new MenuItem("nami.e.combo", "Use E").SetValue(true));
                    comboMenu.AddItem(new MenuItem("nami.r.combo", "Use R").SetValue(true));
                    var rsettings = new Menu("R Settings", "R Settings");
                    {
                        rsettings.AddItem(new MenuItem("nami.min.enemy.count", "Min. Enemy Count").SetValue(new Slider(3, 1, 5)));
                        comboMenu.AddSubMenu(rsettings);
                    }
                    Config.AddSubMenu(comboMenu);
                }
                var healMenu = new Menu("Heal Settings", "Heal Settings");
                {
                    healMenu.AddItem(new MenuItem("nami.heal.disable", "Disable Heal?").SetValue(false));
                    healMenu.AddItem(new MenuItem("ayrac1", "                  Heal Whitelist"));
                    foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsAlly))
                    {
                        healMenu.AddItem(new MenuItem("heal." + ally.CharData.BaseSkinName, string.Format("Heal: {0}", ally.CharData.BaseSkinName)).SetValue(true));
                        healMenu.AddItem(new MenuItem("heal.percent." + ally.CharData.BaseSkinName, string.Format("Heal: {0} HP Percent ", ally.CharData.BaseSkinName)).SetValue(new Slider(30, 1, 99)));
                    }
                    Config.AddSubMenu(healMenu);
                }
                var wsettings = new Menu("(W) Settings", "(W) Settings");
                {
                    wsettings.AddItem(new MenuItem("ayrac3", "                  (W) Whitelist"));
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsEnemy))
                    {
                        wsettings.AddItem(new MenuItem("wwhite." + enemy.CharData.BaseSkinName, string.Format("(W): {0}", enemy.CharData.BaseSkinName)).SetValue(true));
                    }
                    Config.AddSubMenu(wsettings);
                }
                var esettings = new Menu("(E) Settings", "(E) Settings");
                {
                    esettings.AddItem(new MenuItem("ayrac2", "                  (E) Whitelist"));
                    foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsAlly && !o.IsMe))
                    {
                        esettings.AddItem(new MenuItem("ewhite." + ally.CharData.BaseSkinName, string.Format("(E): {0}", ally.CharData.BaseSkinName)).SetValue(true));
                    }
                    Config.AddSubMenu(esettings);
                }
                var harass = new Menu("Harass Settings", "Harass Settings");
                {
                    harass.AddItem(new MenuItem("nami.q.harass", "Use Q").SetValue(true));
                    harass.AddItem(new MenuItem("nami.w.harass", "Use W").SetValue(true));
                    harass.AddItem(new MenuItem("nami.harass.mana", "Min. Mana Percent").SetValue(new Slider(50, 1, 99)));
                    Config.AddSubMenu(harass);
                }

                var drawing = new Menu("Draw Settings", "Draw Settings");
                {
                    drawing.AddItem(new MenuItem("nami.q.draw", "Q Range").SetValue(new Circle(true, Color.Chartreuse)));
                    drawing.AddItem(new MenuItem("nami.w.draw", "W Range").SetValue(new Circle(true, Color.Yellow)));
                    drawing.AddItem(new MenuItem("nami.e.draw", "E Range").SetValue(new Circle(true, Color.White)));
                    drawing.AddItem(new MenuItem("nami.r.draw", "R Range").SetValue(new Circle(true, Color.SandyBrown)));
                    Config.AddSubMenu(drawing);
                }
                Config.AddItem(new MenuItem("nami.q.hitchance", "Skillshot Hit Chance").SetValue(new StringList(HitchanceNameArray, 2)));
                Config.AddToMainMenu();
            }

            SPrediction.Prediction.Initialize(Config);
            Orbwalking.AfterAttack += NamiAfterAttack;
            Game.OnUpdate += NamiOnUpdate;
            Drawing.OnDraw += NamiOnDraw;
        }

        private static void NamiAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && MenuCheck("nami.e.combo",Config) && E.IsReady())
            {
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && !x.IsMe && x.Distance(ObjectManager.Player.Position) < E.Range && !x.IsDead && !x.IsZombie))
                {
                    if (MenuCheck("ewhite." + ally.ChampionName,Config))
                    {
                        E.Cast(ally);
                    }
                }
            }
        }

        private static void NamiOnUpdate(EventArgs args)
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
            HealManager();
        }

        private static void Combo()
        {
            if (MenuCheck("nami.q.combo",Config) && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsDead && !x.IsZombie && !x.HasBuffOfType(BuffType.SpellShield) && !x.HasBuffOfType(BuffType.SpellImmunity)))
                {
                    Q.SPredictionCast(enemy, SpellHitChance(Config, "nami.q.hitchance"));
                }
            }
            if (MenuCheck("nami.w.combo",Config) && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range) && !x.IsDead && !x.IsZombie))
                {
                    if (MenuCheck("wwhite." + enemy.ChampionName,Config))
                    {
                        W.Cast(enemy);
                    }
                }
            }
            if (MenuCheck("nami.r.combo",Config) && R.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(R.Range) >= SliderCheck("nami.min.enemy.count",Config))
                    {
                        R.CastIfWillHit(enemy, SliderCheck("nami.min.enemy.count",Config));
                    }
                }
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < SliderCheck("nami.harass.mana",Config))
            {
                return;
            }

            if (MenuCheck("nami.q.combo",Config) && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsDead && !x.IsZombie && !x.HasBuffOfType(BuffType.SpellShield) && !x.HasBuffOfType(BuffType.SpellImmunity)))
                {
                    Q.SPredictionCast(enemy, SpellHitChance(Config, "nami.q.hitchance"));
                }
            }
            if (MenuCheck("nami.w.combo", Config) && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range) && !x.IsDead && !x.IsZombie))
                {
                    if (MenuCheck("wwhite." + enemy.ChampionName, Config))
                    {
                        W.Cast(enemy);
                    }
                }
            }

        }

        private static void HealManager()
        {
            if (MenuCheck("nami.heal.disable", Config))
            {
                return;
            }
            if (W.IsReady() && !ObjectManager.Player.IsDead && !ObjectManager.Player.IsZombie)
            {
                foreach (var ally in HeroManager.Allies.Where(x => x.Distance(ObjectManager.Player.Position) < W.Range && !x.IsDead && !x.IsZombie))
                {
                    if (MenuCheck("heal." + ally.ChampionName, Config) && ally.HealthPercent < SliderCheck("heal.percent." + ally.ChampionName, Config))
                    {
                        W.Cast(ally);
                    }
                }
            }
        }

        private static void NamiOnDraw(EventArgs args)
        {
            if (Q.IsReady() && ActiveCheck("nami.q.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, GetColor("nami.q.draw", Config));
            }
            if (W.IsReady() && ActiveCheck("nami.w.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, GetColor("nami.w.draw", Config));
            }
            if (E.IsReady() && ActiveCheck("nami.e.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, GetColor("nami.e.draw", Config));
            }
            if (R.IsReady() && ActiveCheck("nami.r.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, GetColor("nami.r.draw", Config));
            }
        }
    }
}
