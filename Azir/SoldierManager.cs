using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Azir
{
    public static class SoldierManager
    {

        public static List<GameObject> AzirSoldiers;
        public static int SoldierAttackRange = 315;

        public static bool NextSoldier;
        public static int LastSoldier;

        public static bool Attacking { get; private set; }
        public static int LastSoldierSpawn { get { return LastSoldier; } }
        public static List<GameObject> ActiveSoldiers { get { return AzirSoldiers; } }

        public static void Initialise()
        {
            AzirSoldiers = new List<GameObject>();

            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        public static float GetAADamage(Obj_AI_Base target)
        {
            return (float)Azir.Player.GetSpellDamage(target, SpellSlot.W) * ActiveSoldiers.Count(player => 
                player.Position.Distance(target.Position) < SoldierAttackRange);
        }

        public static bool InAARange(Obj_AI_Base target)
        {
            foreach (var soldier in AzirSoldiers)
            {
                if (Vector2.DistanceSquared(target.Position.To2D(), soldier.Position.To2D()) <= SoldierAttackRange * SoldierAttackRange)
                    return true;
            }
            return false;
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (NextSoldier && sender.Name == "AzirSoldier")
            {
                AzirSoldiers.Add(sender);
                NextSoldier = false;
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name.ToLower() == "azirbasicattacksoldier")
                {
                    Attacking = true;
                    Utility.DelayAction.Add((int)(Azir.Player.AttackCastDelay * 1000), () => Attacking = false);
                }
                else if (args.SData.Name == Azir.Player.GetSpell(SpellSlot.W).SData.Name)
                {
                    NextSoldier = true;
                    LastSoldier = Utils.TickCount;
                }
            }
        }
    }
}
