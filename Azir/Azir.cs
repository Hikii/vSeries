using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Azir
{
    class Azir
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;

        internal static void Load(EventArgs args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Azir")
                return;

            Spells.Initialise();
            MenuConfig.LoadMenu();

            List<GameObject> Soldiers = SoldierManager.AzirSoldiers;

            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("[00:00] Azir # By Veto");
            Game.PrintChat("[00:00] Remember to Upvote in the Assembly Database!");
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || args == null)
                return;
            
            switch (MenuConfig.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    break;
            }

            if (IsActive("Combo.All_In"))
                AllInCombo();

            if (IsActive("EliteCombo"))
                EliteAzirCombo(Game.CursorPos);

            if (IsActive("Escape"))
                Escape(Game.CursorPos);

            if (IsActive("DrawDamage"))
                Utility.HpBarDamageIndicator.DamageToUnit += target => ComboDamage(target);
        }

        private static bool IsActive(string menuItem)
        {
            return MenuConfig.config.Item(menuItem).GetValue<KeyBind>().Active;
        }
        static void Combo()
        {
            var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);

            foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Spells.Q.Range - 15)))
            {
                var closestPos = Player.Position.Extend(enemy.Position, SoldierManager.SoldierAttackRange);

                if (MenuConfig.ComboR && Spells.R.IsReady() && Player.ServerPosition.Distance(enemy.ServerPosition) < Spells.R.Range)
                    Spells.R.Cast(enemy);

                if (MenuConfig.ComboW && Spells.W.IsReady())
                    Spells.W.Cast(closestPos);

                if (MenuConfig.ComboQ && Spells.Q.IsReady() && Player.ServerPosition.Distance(enemy.ServerPosition) > SoldierManager.SoldierAttackRange)
                    Spells.Q.Cast(enemy);
            }
        }

        static void AllInCombo()
        {
            var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);

            foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Spells.Q.Range - 15)))
            {
                var closestPos = Player.Position.Extend(enemy.Position, SoldierManager.SoldierAttackRange);

                if (Spells.R.IsReady() && Player.ServerPosition.Distance(enemy.ServerPosition) < Spells.R.Range)
                    Spells.R.Cast(enemy);

                if (Spells.W.IsReady())
                    Spells.W.Cast(closestPos);

                if (Spells.Q.IsReady() && Player.ServerPosition.Distance(enemy.ServerPosition) > SoldierManager.SoldierAttackRange)
                    Spells.Q.Cast(enemy);

                if (Spells.E.IsReady())
                    Spells.E.Cast(Game.CursorPos);
            }
        }

        static void EliteAzirCombo(Vector3 pos)
        {
            var backCast = Player.ServerPosition.To2D().Extend(pos.To2D(), -700);

            Spells.W.Cast(Player.ServerPosition.To2D().Extend(pos.To2D(), Spells.W.Range));
            Spells.E.Cast(Game.CursorPos);

            Utility.DelayAction.Add(
                34,
                () =>
                {
                    Spells.R.Cast(backCast);
                }
            );

            Utility.DelayAction.Add(
                425,
                () =>
                {
                    Spells.Q.Cast(backCast);
                }
            );

            MenuConfig.config.Item("EliteCombo").SetValue(new KeyBind('G', KeyBindType.Press, false));
        }

        static void Harass()
        {
            if (Player.ManaPercent < MenuConfig.HarassMana)
                return; 

            foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Spells.Q.Range)))
            {
                var closestPos = Player.Position.Extend(enemy.Position, SoldierManager.SoldierAttackRange);

                if (MenuConfig.HarassW && Spells.W.IsReady())
                Spells.W.Cast(closestPos);

                if (MenuConfig.HarassQ && Spells.W.IsReady() && Player.ServerPosition.Distance(enemy.ServerPosition) > Spells.W.Range)
                    Spells.Q.Cast(enemy);
            }
        }

        public static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(Player.ServerPosition, Spells.W.Range);

            if (Minions.Count > 0)
            {
                if (MenuConfig.LaneW && Spells.W.Instance.Ammo > 0 && (Minions.Count > 2 || Minions[0].Team == GameObjectTeam.Neutral))
                {
                    var position = Player.ServerPosition.To2D().Extend(Minions[0].Position.To2D(), Spells.W.Range);
                    Spells.W.Cast(position);
                }
            }
        }

        static void Killsteal()
        {
            if (!Spells.Q.IsReady() || (SoldierManager.ActiveSoldiers.Count >= 0 && !Spells.W.IsReady()))
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Spells.Q.Range + 100) && 
                !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(p => ComboDamage(p)))
            {
                if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                {
                    if (SoldierManager.ActiveSoldiers.Count >= 0)
                        Spells.W.Cast(Player.Position.To2D().Extend(target.Position.To2D(), 450));
                    else
                        Spells.Q.Cast(target);
                }
            }
        }
        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High)
                return;

            if (IsActive("Interrupt.R.Use") && Spells.R.IsReady())
            {
                var distance = Player.Distance(sender, true);

                if (distance > Spells.R.RangeSqr)
                {
                    Spells.R.Cast(sender, false, true);
                    return;
                }

                if (distance < Math.Pow(Math.Sqrt(Spells.R.RangeSqr + Math.Pow(Spells.R.Width + sender.BoundingRadius, 2)) , 2))
                {
                    var angle = (float)Math.Atan(Spells.R.Width + sender.BoundingRadius / Spells.R.Range);
                    var position = (sender.ServerPosition.To2D() - Player.ServerPosition.To2D()).Rotated(angle);

                    Spells.R.Cast(position);
                }
            }
        }

        static void Escape(Vector3 pos)
        {
            var extended = Player.ServerPosition.Extend(Game.CursorPos, 1800f);
            
            Spells.W.Cast(Game.CursorPos);
            Spells.E.Cast(Game.CursorPos);
            Spells.Q.Cast(extended);
        }

        static float ComboDamage(Obj_AI_Base target)
        {
            var damage = 0d;

            if (Spells.Q.IsReady())
                damage += Player.GetSpellDamage(target, SpellSlot.Q);

            damage += SoldierManager.ActiveSoldiers.Count * Player.GetSpellDamage(target, SpellSlot.W);

            if (Spells.E.IsReady())
                damage += Player.GetSpellDamage(target, SpellSlot.E);

            if (Spells.R.IsReady())
                damage += Player.GetSpellDamage(target, SpellSlot.R);

            if (Spells.Ignite != SpellSlot.Unknown && Player.Spellbook.GetSpell(Spells.Ignite).State == SpellState.Ready)
                damage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            return (float)damage;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (MenuConfig.DrawQ)
                Render.Circle.DrawCircle(Player.Position, Spells.Q.Range, Color.Blue);
            if (MenuConfig.DrawW)
                Render.Circle.DrawCircle(Player.Position, Spells.W.Range, Color.AliceBlue);
            if (MenuConfig.DrawE)
                Render.Circle.DrawCircle(Player.Position, Spells.E.Range, Color.BlueViolet);
            if (MenuConfig.DrawR)
                Render.Circle.DrawCircle(Player.Position, Spells.R.Range, Color.LightBlue);
        }
    }
}