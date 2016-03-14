using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using vSupport_Series.Core.Plugins;
using Color = System.Drawing.Color;

namespace vSupport_Series.Champions
{
    public class Trundle : Helper
    {
        public static Menu Config;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;

        public Trundle()
        {
            TrundleOnLoad();
        }

        public static void TrundleOnLoad()
        {
            Q = new Spell(SpellSlot.Q, 550f);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1000f);

            E.SetSkillshot(0.5f, 188f, 1600f, false, SkillshotType.SkillshotCircle);

            Config = new Menu("vSupport Series: " + ObjectManager.Player.ChampionName, "vSupport Series", true);
            {
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

                var comboMenu = new Menu(":: Combo Settings", ":: Combo Settings");
                {
                    comboMenu.AddItem(new MenuItem("trundle.q.combo", "Use Q").SetValue(true));
                    comboMenu.AddItem(new MenuItem("trundle.w.combo", "Use Q").SetValue(true));
                    comboMenu.AddItem(new MenuItem("trundle.e.combo", "Use E").SetValue(true));
                    comboMenu.AddItem(new MenuItem("trundle.r.combo", "Use R").SetValue(true));
                    Config.AddSubMenu(comboMenu);
                }

                var whitelist = new Menu(":: (R) Whitelist", ":: (R) Whitelist");
                {
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        whitelist.AddItem(new MenuItem("trundle.q." + enemy.ChampionName, "(Q): " + enemy.ChampionName).SetValue(true));
                    }
                    Config.AddSubMenu(whitelist);
                }

                var harassMenu = new Menu(":: Harass Settings", ":: Harass Settings");
                {
                    harassMenu.AddItem(new MenuItem("trundle.q.harass", "Use Q").SetValue(true));
                    harassMenu.AddItem(new MenuItem("trundle.w.harass", "Use W").SetValue(true));
                    harassMenu.AddItem(new MenuItem("trundle.e.harass", "Use E").SetValue(true));
                    harassMenu.AddItem(new MenuItem("trundle.harass.mana", "Min. Mana")).SetValue(new Slider(50,1,99));
                    Config.AddSubMenu(harassMenu);
                }

                var drawing = new Menu("Draw Settings", "Draw Settings");
                {
                    drawing.AddItem(new MenuItem("trundle.q.draw", "Q Range").SetValue(new Circle(true, Color.Chartreuse)));
                    drawing.AddItem(new MenuItem("trundle.w.draw", "W Range").SetValue(new Circle(true, Color.Yellow)));
                    drawing.AddItem(new MenuItem("trundle.e.draw", "E Range").SetValue(new Circle(true, Color.White)));
                    drawing.AddItem(new MenuItem("trundle.r.draw", "R Range").SetValue(new Circle(true, Color.SandyBrown)));
                    Config.AddSubMenu(drawing);
                } 
                Config.AddItem(new MenuItem("trundle.pillar.block", "Use (E) for Blitzcrank Q").SetValue(true));
            }
            Config.AddToMainMenu();
            Game.OnUpdate += TrundleOnUpdate;
            Drawing.OnDraw += TrundleOnDraw;
            Obj_AI_Base.OnProcessSpellCast += TrundleOnProcessSpellCast;
        }

        private static void TrundleOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && args.End.Distance(ObjectManager.Player.Position) < 150 && args.SData.Name == "RocketGrab"
                && sender.CharData.BaseSkinName == "Blitzcrank")
            {
                E.Cast(ObjectManager.Player.Position.Extend(args.End, 100));
            }
        }

        private static void TrundleOnUpdate(EventArgs args)
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
            if (Q.IsReady() && MenuCheck("trundle.q.combo", Config))
            {
                // ReSharper disable once UnusedVariable
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Q.Range)))
                {
                    Q.Cast();
                }
            }

            if (W.IsReady() && MenuCheck("trundle.w.combo", Config))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(W.Range)))
                {
                    W.Cast(enemy);
                }
            }

            if (E.IsReady() && MenuCheck("trundle.e.combo",Config))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(E.Range)))
                {
                    var pred = E.GetPrediction(enemy);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        E.Cast(PillarPos(enemy));
                    }
                }
            }

            if (R.IsReady() && MenuCheck("trundle.r.combo", Config))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && MenuCheck("trundle.q." + x.ChampionName, Config)))
                {
                    R.Cast(enemy);
                }
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent <= SliderCheck("trundle.harass.mana",Config))
            {
                return;
            }
            if (Q.IsReady() && MenuCheck("trundle.q.harass", Config))
            {
                // ReSharper disable once UnusedVariable
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range)))
                {
                    Q.Cast();
                }
            }

            if (W.IsReady() && MenuCheck("trundle.w.harass", Config))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range)))
                {
                    W.Cast(enemy);
                }
            }

            if (E.IsReady() && MenuCheck("trundle.e.harass", Config))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range)))
                {
                    E.Cast(PillarPos(enemy));
                }
            }
        }

        private static Vector3 PillarPos(Obj_AI_Hero enemy)
        {
            return enemy.Position.To2D().Extend(ObjectManager.Player.Position.To2D(), -E.Width / 2).To3D();
        }

        private static void TrundleOnDraw(EventArgs args)
        {
            if (Q.IsReady() && ActiveCheck("trundle.q.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, GetColor("trundle.q.draw", Config));
            }
            if (W.IsReady() && ActiveCheck("trundle.w.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, GetColor("trundle.w.draw", Config));
            }
            if (E.IsReady() && ActiveCheck("trundle.e.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, GetColor("trundle.e.draw", Config));
            }
            if (R.IsReady() && ActiveCheck("trundle.r.draw", Config))
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, GetColor("trundle.r.draw", Config));
            }
            foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(1000)))
            {
                Render.Circle.DrawCircle(PillarPos(enemy),50,Color.Gold);
            }
        }
    }
}
