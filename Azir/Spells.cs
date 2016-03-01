using LeagueSharp;
using LeagueSharp.Common;

namespace Azir
{
    public class Spells
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static SpellSlot Ignite;

        public static void Initialise()
        {
            Q = new Spell(SpellSlot.Q, 1175);
                Q.SetSkillshot(0.0f, 65, 1500, false, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R, 250);

            Ignite = Player.GetSpellSlot("SummonerDot");
        }

        /*
        public static void CastSpell(Spell spell, Obj_AI_Base target, HitChance hitchance)
        {
            if (target.IsValidTarget(spell.Range) && spell.GetPrediction(target).Hitchance >= hitchance)
                spell.Cast(target);
        }
        */
    }
}