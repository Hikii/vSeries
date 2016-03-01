using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

// THANKS TO KORTATU 

namespace Azir
{
    class Walker : Orbwalking.Orbwalker
    {
        private const int SoldierAARange = 250;

        public static List<GameObject> ActiveSoldiers { get { return SoldierManager.AzirSoldiers; } }

        public Walker (Menu attachToMenu) : base (attachToMenu)
        {

        }

        private static float GetDamageValue(Obj_AI_Base target, bool soldierAttack)
        {
            var d = soldierAttack;
            return target.Health;
        }

        public override bool InAutoAttackRange(AttackableUnit target)
        {
            return CustomInAARange(target) != 0;
        }

        public int CustomInAARange(AttackableUnit target)
        {
            if (Orbwalking.InAutoAttackRange(target))
                return 1;

            if (!target.IsValidTarget())
                return 0;

            if (!(target is Obj_AI_Base))
                return 0;

            var soldierAARange = SoldierAARange + 65 + target.BoundingRadius;
            soldierAARange *= SoldierAARange;
            foreach (var soldier in ActiveSoldiers)
            {
                
            }

            return 0;
        }

        public override AttackableUnit GetTarget()
        {
            AttackableUnit result;
            if (ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || ActiveMode == Orbwalking.OrbwalkingMode.Mixed || ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(
                    m => m.IsValidTarget() && m.Health < 3 * ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod))
                {
                    var r = CustomInAARange(minion);
                    if (r != 0)
                    {
                        var t = (int)(ObjectManager.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2;
                        var predHealth = HealthPrediction.GetHealthPrediction(minion, t, 0);
                        var damage = (r == 1) ? ObjectManager.Player.GetAutoAttackDamage(minion, true) : ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W);
                        if (minion.Team != GameObjectTeam.Neutral && MinionManager.IsMinion(minion, true))
                        {
                            if (predHealth > 0 && predHealth <= damage)
                                return minion;
                        }
                    }
                }
            }

            if (ActiveMode != Orbwalking.OrbwalkingMode.LastHit)
            {
                var possibleTargets = new Dictionary<Obj_AI_Base, float>();
                var aaTarget = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);

                if (aaTarget.IsValidTarget())
                    possibleTargets.Add(aaTarget, GetDamageValue(aaTarget, false));

                foreach (var soldier in ActiveSoldiers)
                {
                    var soldierTarget = TargetSelector.GetTarget(SoldierAARange + 65 + 65, TargetSelector.DamageType.Magical, true, null, soldier.Position);
                    if (soldierTarget.IsValidTarget())
                    {
                        if (possibleTargets.ContainsKey(soldierTarget))
                            possibleTargets[soldierTarget] *= 1.25f;
                        else
                            possibleTargets.Add(soldierTarget, GetDamageValue(soldierTarget, true));
                    }
                }

                if (possibleTargets.Count > 0)
                    return possibleTargets.MinOrDefault(p => p.Value).Key;
            }

            if (ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsValidTarget() && Orbwalking.InAutoAttackRange(t)))
                {
                    return turret;
                }

                foreach (var inhib in ObjectManager.Get<Obj_BarracksDampener>().Where(i => i.IsValidTarget() && Orbwalking.InAutoAttackRange(i)))
                {
                    return inhib;
                }

                foreach (var nexus in ObjectManager.Get<Obj_HQ>().Where(n => n.IsValidTarget() & Orbwalking.InAutoAttackRange(n)))
                {
                    return nexus;
                }
            }

            if (ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                result = ObjectManager.Get<Obj_AI_Minion>().Where(
                    mob => mob.IsValidTarget() && Orbwalking.InAutoAttackRange(mob) && mob.Team == GameObjectTeam.Neutral).MaxOrDefault(mob => mob.MaxHealth);

                if (result != null)
                    return result;
            }

            if (ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                return (ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && InAutoAttackRange(minion))).MaxOrDefault(m => CustomInAARange(m) * m.Health);
            }

            return null;
        }
    }
}
