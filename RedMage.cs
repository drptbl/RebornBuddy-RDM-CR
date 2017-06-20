using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using Kupo.Helpers;
using System.Linq;
using TreeSharp;

// Made with love for FFXIV by drptbl

namespace Kupo.Rotations
{
    internal class RedMage : KupoRoutine
    {
        public override string Name => "Kupo [" + GetType().Name + "]";

        public override ClassJobType[] Class => new[] { ClassJobType.RedMage };

        public override float PullRange => 20;

        [Behavior(BehaviorType.PreCombatBuffs)]
        public Composite CreateBasicPreCombatBuffs()
        {
            return SummonChocobo();
        }

        [Behavior(BehaviorType.Rest)]
        public Composite CreateBasicRest()
        {
            return DefaultRestBehavior(r => Core.Player.CurrentManaPercent);
        }

        private readonly string[] _pullSpells = new[] { "Jolt II", "Jolt" };
        private string _bestPullSpell;
        private uint _level;

        private string BestPullSpell
        {
            get
            {
                if (_level != Core.Player.ClassLevel)
                {
                    _level = Core.Player.ClassLevel;
                    _bestPullSpell = null;
                }

                if (string.IsNullOrEmpty(_bestPullSpell))
                {
                    foreach (var spell in _pullSpells.Where(ActionManager.HasSpell))
                    {
                        _bestPullSpell = spell;
                        break;
                    }
                }

                return _bestPullSpell;
            }
        }

        [Behavior(BehaviorType.Pull)]
        public Composite CreateBasicPull()
        {
            return new PrioritySelector(ctx => Core.Player.CurrentTarget as BattleCharacter,
    new Decorator(ctx => ctx != null, new PrioritySelector(
                    EnsureTarget,
            CommonBehaviors.MoveToLos(ctx => ctx as GameObject),
            CommonBehaviors.MoveAndStop(ctx => ((GameObject) ctx).Location, PullRange, true, "Moving to unit"),
    Spell.PullApply(r => BestPullSpell)
    )));
        }

        [Behavior(BehaviorType.CombatBuffs)]
        public static Composite CreateBasicCombatBuffs()
        {
            return new PrioritySelector(
                Spell.Apply("Acceleration", r => Core.Player),
                Spell.Apply("Swiftcast", r => Core.Player),
                Spell.Apply("Embolden", r => Core.Player)
                );
        }

        [Behavior(BehaviorType.Heal)]
        public static Composite CreateBasicHeal()
        {
            return new PrioritySelector(
                Spell.Cast("Vercure", r => Core.Player.CurrentHealthPercent <= 40 && Core.Player.ClassLevel >= 54, r => Core.Player)
                );
        }

        [Behavior(BehaviorType.Combat)]
        public Composite CreateBasicCombat()
        {
            return new PrioritySelector(ctx => Core.Player.CurrentTarget as BattleCharacter,
                new Decorator(ctx => ctx != null,
                    new PrioritySelector(
                        CommonBehaviors.MoveToLos(ctx => ctx as GameObject),
                        CommonBehaviors.MoveAndStop(ctx => ((GameObject) ctx).Location, PullRange, true, "Moving to unit"),

                        Spell.Apply("Manafication", r => ActionResourceManager.RedMage.BlackMana >= 40 && ActionResourceManager.RedMage.WhiteMana >= 40 && Core.Player.ClassLevel >= 60, r => Core.Player),

                        Spell.Cast("Verfire", r => (Core.Player.HasAura("Swiftcast") || Core.Player.HasAura("Dualcast")) && Core.Player.HasAura("Verfire Ready") && ActionResourceManager.RedMage.BlackMana <= ActionResourceManager.RedMage.WhiteMana),
                        Spell.Cast("Verstone", r => (Core.Player.HasAura("Swiftcast") || Core.Player.HasAura("Dualcast")) && Core.Player.HasAura("Verstone Ready") && ActionResourceManager.RedMage.WhiteMana <= ActionResourceManager.RedMage.BlackMana),

                        Spell.Cast("Verthunder", r => Core.Player.HasAura("Swiftcast") || Core.Player.HasAura("Dualcast") && ActionResourceManager.RedMage.BlackMana <= ActionResourceManager.RedMage.WhiteMana),
                        Spell.Cast("Veraero", r => Core.Player.HasAura("Swiftcast") || Core.Player.HasAura("Dualcast") && ActionResourceManager.RedMage.WhiteMana <= ActionResourceManager.RedMage.BlackMana),

                        Spell.Cast("Verfire", r => Core.Player.HasAura("Verfire Ready") && ActionResourceManager.RedMage.BlackMana <= ActionResourceManager.RedMage.WhiteMana),
                        Spell.Cast("Verstone", r => Core.Player.HasAura("Verstone Ready") && ActionResourceManager.RedMage.WhiteMana <= ActionResourceManager.RedMage.BlackMana),

                        Spell.Cast("Fleche", r => !Core.Player.HasAura("Swiftcast") && !Core.Player.HasAura("Dualcast")),
                        Spell.Cast("Contre Sixte", r => !Core.Player.HasAura("Swiftcast") && !Core.Player.HasAura("Dualcast") && Core.Player.ClassLevel >= 56),

                        Spell.Cast("Impact", r => Core.Player.ClassLevel >= 67 && Core.Player.HasAura("Impactful")),
                        Spell.Cast("Jolt", r => Core.Player.ClassLevel < 62),
                        Spell.Cast("Jolt II", r => Core.Player.ClassLevel >= 62)
                        )));
        }
    }
}
