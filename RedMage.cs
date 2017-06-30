using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using Kupo.Helpers;
using Kupo.Settings;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using TreeSharp;

// Combat routine made with love for FFXIV by drptbl

namespace Kupo.Rotations
{
    internal class RedMage : KupoRoutine
    {
        public override string Name => "Kupo [" + GetType().Name + "]";

        public override ClassJobType[] Class => new[] { ClassJobType.RedMage };

        public override float PullRange => 20;

        public override void OnInitialize()
        {
            WindowSettings = new RedMageSettings();
            settings = WindowSettings as RedMageSettings;
        }

        private RedMageSettings settings;

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
                    CommonBehaviors.MoveAndStop(ctx => ((GameObject)ctx).Location, PullRange, true, "Moving to unit"),
                    Spell.PullApply(r => BestPullSpell)
                )));
        }

        [Behavior(BehaviorType.Combat)]
        public Composite CreateBasicCombat()
        {
            return new PrioritySelector(ctx => Core.Player.CurrentTarget as BattleCharacter,
                new Decorator(ctx => ctx != null,
                    new PrioritySelector(
                        CommonBehaviors.MoveToLos(ctx => ctx as GameObject),
                        CommonBehaviors.MoveAndStop(ctx => ((GameObject) ctx).Location, PullRange, true,
                            "Moving to unit"),

                        Spell.Apply("Vercure", r => Core.Player.CurrentHealthPercent <= settings.HealAtHealthPercent && Core.Player.ClassLevel >= 54 && settings.UseHeal, r => Core.Player),

                        Spell.Apply("Acceleration", r => settings.UseAcceleration, r => Core.Player),
                        Spell.Apply("Swiftcast", r => settings.UseSwiftcast, r => Core.Player),
                        Spell.Apply("Embolden", r => settings.UseEmbolden, r => Core.Player),
                        Spell.Apply("Lucid Dreaming", r => Core.Player.CurrentManaPercent <= settings.LucidDreamingManaPercent && settings.UseLucidDreaming, r => Core.Player),

                        Spell.Cast("Scatter", r => settings.UseAoe && EnemiesNearPlayer(24.9f, f => f.TimeToDeath() > 3) >= settings.MinMonstersToAoe),
                        Spell.Cast("Scatter", r => settings.UseAoe && EnemiesNearPlayer(24.9f, f => f.TimeToDeath() > 3) >= settings.MinMonstersToAoe),

                        Spell.Cast("Enchanted Zwerchhau", r => ActionManager.LastSpell.Name == "Riposte" && Core.Target.Distance2D() <= 2.9f),
                        Spell.Cast("Enchanted Redoublement", r => ActionManager.LastSpell.Name == "Zwerchhau" && Core.Target.Distance2D() <= 2.9f),

                        Spell.Cast("Verholy", r => ActionManager.LastSpell.Name == "Enchanted Redoublement" && ActionResourceManager.RedMage.BlackMana >= ActionResourceManager.RedMage.WhiteMana && Core.Player.ClassLevel >= 70),
                        Spell.Cast("Verflare", r => ActionManager.LastSpell.Name == "Enchanted Redoublement" && ActionResourceManager.RedMage.WhiteMana >= ActionResourceManager.RedMage.BlackMana && Core.Player.ClassLevel >= 68),

                        Spell.Cast("Displacement", r => settings.UseDisplacement && (ActionManager.LastSpell.Name == "Enchanted Redoublement" || ActionManager.LastSpell.Name == "Verholy" || ActionManager.LastSpell.Name == "Verflare") && Core.Target.Distance2D() <= 2.9f),

                        Spell.Apply("Manafication", r => ActionResourceManager.RedMage.BlackMana >= 40 && ActionResourceManager.RedMage.WhiteMana >= 40 && Core.Player.ClassLevel >= 60, r => Core.Player),

                        Spell.Cast("Corps-a-corps", r => settings.UseCorpsACorps && ActionResourceManager.RedMage.BlackMana >= 80 && ActionResourceManager.RedMage.WhiteMana >= 80),
                        Spell.Cast("Enchanted Riposte", r => ActionResourceManager.RedMage.BlackMana >= 80 && ActionResourceManager.RedMage.WhiteMana >= 80 && Core.Target.Distance2D() <= 2.9f),

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
