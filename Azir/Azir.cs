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

        public static List<Obj_AI_Minion> AzirSoldiers;

        public static List<Obj_AI_Minion> ActiveSoldiers { get { return AzirSoldiers; } }

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

            AzirSoldiers = new List<Obj_AI_Minion>();

            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("<font color='#fffff'>[00:00] [Assembly Name Here]</font> <font color='#1abc9c'>Activated</font>");
            Game.PrintChat("<font color='#fffff'>[00:01] Remember to <font color='#1abc9c'>Upvote</font> in the Assembly Database!</font>");
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
        }

        private static bool IsActive(string menuItem)
        {
            return MenuConfig.config.Item(menuItem).GetValue<KeyBind>().Active;
        }

        private static int GetValue(string menuItem)
        {
            return MenuConfig.config.Item(menuItem).GetValue<Slider>().Value;
        }

        static List<Obj_AI_Minion> Soldiers()
        {
            return AzirSoldiers.Where(soldier => !soldier.IsDead).ToList();
        }

        static Obj_AI_Minion getCloseSoldier(Vector3 pos)
        {
            return AzirSoldiers.Where(soldier => !soldier.IsDead).OrderBy(soldier => soldier.Distance(pos, true) - 
            ((soldier.IsMoving)?500:0)).FirstOrDefault();
        }
        /*
        static void Combo()
        {
            var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);
            var extendTarget = TargetSelector.GetTarget(Spells.Q.Range + 400, TargetSelector.DamageType.Magical);

                if (IsActive("Combo.Q.Use") && Spells.Q.IsReady() && target.IsValidTarget(Spells.Q.Range))
                {
                    foreach (var soldier in SoldierManager.ActiveSoldiers)
                    {
                        if (Player.ServerPosition.Distance(target.ServerPosition) < Spells.Q.Range)
                        {
                            Spells.Q.UpdateSourcePosition(soldier.Position, Player.ServerPosition);

                            var prediction = Spells.Q.GetPrediction(target);

                            if (prediction.Hitchance >= HitChance.High)
                            {
                                var position = prediction.CastPosition;

                                Spells.Q.Cast(position);
                                return;
                            }
                        }
                    }
                }

                if (IsActive("Combo.W.Use") && Spells.W.IsReady())
                    Spells.W.Cast(Player.Position.To2D().Extend(target.Position.To2D(), 450));

                if (IsActive("Combo.R.Use") && Spells.R.IsReady() && Spells.R.IsInRange(target))
                {
                    Spells.R.Cast(target);
                }
        }
        */
        static void Combo()
        {
            var target = TargetSelector.GetSelectedTarget();
            var extendTarget = TargetSelector.GetTarget(Spells.Q.Range + 400, TargetSelector.DamageType.Magical);

            if ((IsActive("Combo.W.Use")) && Spells.W.IsReady())
                Spells.W.Cast(target);

            if (IsActive("Combo.Q.Use") && Spells.Q.IsReady() && target.IsValidTarget(Spells.Q.Range))
            {
                if (Player.ServerPosition.Distance(target.ServerPosition) < Spells.Q.Range)
                {
                    var prediction = Spells.Q.GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.High)
                    {
                        Spells.Q.Cast(target);
                    }
                }
            }

            if (IsActive("Combo.R.Use") && Spells.R.IsReady() && Spells.R.IsInRange(target))
                Spells.R.Cast(target);
        }

        static void AllInCombo()
        {
            var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);
            var extendTarget = TargetSelector.GetTarget(Spells.Q.Range + 400, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                if (IsActive("Combo.All_In") && Spells.Q.IsReady() && Spells.W.IsReady() && Spells.E.IsReady() && Spells.R.IsReady() && target.IsValidTarget(Spells.Q.Range))
                {
                    foreach (var soldier in SoldierManager.ActiveSoldiers)
                    {
                        if (Player.ServerPosition.Distance(target.ServerPosition) < Spells.Q.Range)
                        {
                            Spells.W.Cast(Player.Position.To2D().Extend(target.Position.To2D(), 450));
                            Spells.Q.UpdateSourcePosition(soldier.Position, Player.ServerPosition);
                            Spells.Q.Cast(target);
                            Spells.E.Cast(Game.CursorPos);
                            Spells.R.Cast(target);
                            Spells.W.Cast(target);
                        }
                    }
                }
            }
        }

        static void EliteAzirCombo(Vector3 pos)
        {
            var extended = Player.ServerPosition.To2D().Extend(pos.To2D(), 1800f);
            var extendBack = Player.ServerPosition.To2D().Extend(pos.To2D(), -700f);
            var summonSoldier = Game.CursorPos + 100f;
            // var closest = getCloseSoldier(pos);
            // var playerPos = Player.ServerPosition;
            // var target = TargetSelector.GetTarget(Spells.W.Range + 100, TargetSelector.DamageType.Magical);

            Spells.W.Cast(summonSoldier);
            Utility.DelayAction.Add(0, () => Spells.E.Cast(Game.CursorPos));
            Utility.DelayAction.Add(34, () => Spells.R.Cast(extendBack));
            Utility.DelayAction.Add(425, () => Spells.Q.Cast(extendBack));

            MenuConfig.config.Item("EliteCombo").SetValue(new KeyBind('G', KeyBindType.Press, false));
        }

        static void Harass()
        {
            var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);

            if (Player.ManaPercent < (GetValue("Harass.Mana"))) // MenuConfig.config.SubMenu("Harass").Item("Harass.Mana").GetValue<Slider>().Value)
                return;

            if (target != null)
                return;

            if (IsActive("Harass.Q.Use") && Spells.Q.IsReady() && Player.Mana > (GetValue("Harass.Mana")))
            {
                var qTarget = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical);

                if (qTarget != null)
                {
                    foreach (var soldier in SoldierManager.AzirSoldiers)
                    {
                        Spells.Q.UpdateSourcePosition(soldier.Position, Player.ServerPosition);
                        Spells.Q.Cast(qTarget);
                    }
                }
            }

            if (IsActive("Harass.W.Use") & Spells.W.IsReady() && Player.Mana > (GetValue("Harass.Mana")))
                Spells.W.Cast(Player.Position.To2D().Extend(target.Position.To2D(), 450));
        }

        public static void Laneclear()
        {
            if (Player.Mana > (GetValue("Lane.Mana")))
                return;

            if (IsActive("Lane.Use.Q") && Spells.Q.IsReady() && Player.Mana > (GetValue("Lane.Mana")))
            {
                MinionManager.FarmLocation farm = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(Spells.Q.Range + 100).Select(player => 
                    player.ServerPosition.To2D()).ToList(), SoldierManager.SoldierAttackRange, Spells.Q.Range + 100);

                if (farm.MinionsHit >= MenuConfig.config.SubMenu("Lane").Item("Lane.Min.Minions").GetValue<Slider>().Value)
                    Spells.Q.Cast(farm.Position);

            }

            if (IsActive("Lane.Use.W") && Spells.W.IsReady() && Spells.W.Instance.Ammo > 0 && Player.Mana > (GetValue("Lane.Mana")))
            {
                var minions = MinionManager.GetMinions(Spells.W.Range + SoldierManager.SoldierAttackRange / 2f);

                if (minions.Count > 1)
                {
                    var summon = MinionManager.GetBestCircularFarmLocation(minions.Select(player => player.ServerPosition.To2D()).ToList(),
                        SoldierManager.SoldierAttackRange, Spells.W.Range);

                    if (summon.MinionsHit > 2)
                        Spells.W.Cast(summon.Position);
                }
            }
        }

        static void Killsteal()
        {
            if (!Spells.Q.IsReady() || (SoldierManager.ActiveSoldiers.Count == 0 && !Spells.W.IsReady()))
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Spells.Q.Range + 100) && 
                !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(p => ComboDamage(p)))
            {
                if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                {
                    if (SoldierManager.ActiveSoldiers.Count == 0)
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
            if (!IsActive("DisableDraw"))
            {
                if (IsActive("Draw.Q"))
                    Render.Circle.DrawCircle(Player.Position, Spells.Q.Range, MenuConfig.config.SubMenu("Drawing").Item("Draw.Q").GetValue<Circle>().Color);
                if (IsActive("Draw.W"))
                    Render.Circle.DrawCircle(Player.Position, Spells.Q.Range, MenuConfig.config.SubMenu("Drawing").Item("Draw.W").GetValue<Circle>().Color);
                if (IsActive("Draw.E"))
                    Render.Circle.DrawCircle(Player.Position, Spells.Q.Range, MenuConfig.config.SubMenu("Drawing").Item("Draw.E").GetValue<Circle>().Color);
                if (IsActive("Draw.R"))
                    Render.Circle.DrawCircle(Player.Position, Spells.Q.Range, MenuConfig.config.SubMenu("Drawing").Item("Draw.R").GetValue<Circle>().Color);
            } 
        }
    }
}