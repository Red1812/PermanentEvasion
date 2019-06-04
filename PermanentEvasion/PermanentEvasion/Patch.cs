using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;

namespace PermanentEvasion {

    [HarmonyPatch(typeof(AbstractActor), "ResolveAttackSequence")]
    public static class AbstractActor_ResolveAttackSequence {

        static bool Prefix(AbstractActor __instance) {
            return false;
        }

        static void Postfix(AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection) {
            try {

                AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
                if (attackSequence != null && attackSequence.GetAttackDidDamage(__instance.GUID)) {
                    List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance).FindAll((Effect x) => x.EffectData.targetingData.effectTriggerType == EffectTriggerType.OnDamaged);
                    for (int i = 0; i < list.Count; i++) {
                        list[i].OnEffectTakeDamage(attackSequence.attacker, __instance);
                    }
                    if (attackSequence.isMelee) {
                        int value = attackSequence.attacker.StatCollection.GetValue<int>("MeleeHitPushBackPhases");
                        if (value > 0) {
                            for (int j = 0; j < value; j++) {
                                __instance.ForceUnitOnePhaseDown(sourceID, stackItemID, false);
                            }
                        }
                    }
                }
                int evasivePipsCurrent = __instance.EvasivePipsCurrent;
                if (attackSequence.GetAttackDidDamage(__instance.GUID) || Fields.LoosePip) {
                    __instance.ConsumeEvasivePip(true);
                    Fields.LoosePip = false;
                }
                int evasivePipsCurrent2 = __instance.EvasivePipsCurrent;
                if (evasivePipsCurrent2 < evasivePipsCurrent && attackSequence.GetAttackDidDamage(__instance.GUID) && !__instance.IsDead && !__instance.IsFlaggedForDeath) {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "HIT: -1 EVASION", FloatieMessage.MessageNature.Debuff));
                }
                else if (evasivePipsCurrent2 < evasivePipsCurrent && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "-1 EVASION", FloatieMessage.MessageNature.Debuff));
                }
                else if (evasivePipsCurrent2 > 0 && Fields.KeptPip && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "EVASION KEPT", FloatieMessage.MessageNature.Debuff));
                    Fields.KeptPip = false;
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(Mech), "ResolveAttackSequence")]
    public static class Mech_ResolveAttackSequence {
        static void Prefix(Mech __instance) {
            Settings settings = Helper.LoadSettings();
            try {
                bool acepilot = false;
                foreach (Ability ab in __instance.pilot.Abilities) {
                    if(ab.Def.Description.Id == "AbilityDefP8") {
                        acepilot = true;
                    }
                }
                int test = 10;
                int acePipsBonus = 0;
                if (settings.PilotSkillToKeepPips)
                {
                    int perSkillPointToKeepPips = Math.Min(settings.PerSkillPointToKeepPips, 10);
                    int acePilotPointToKeepPips = Math.Min(settings.AcePilotPointToKeepPips, 10);
                    test += __instance.SkillPiloting * perSkillPointToKeepPips;
                    if (acepilot)
                    {
                        test += __instance.SkillPiloting * Math.Max(acePilotPointToKeepPips, perSkillPointToKeepPips) ;
                        acePipsBonus += settings.AcePilotBonusPips;
                    }
                }
                else {
                    test = settings.PercentageToKeepPips;
                    if (acepilot) {
                        test += settings.AcePilotBonusPercentage;
                        acePipsBonus += settings.AcePilotBonusPips;
                    }
                }
                int cap = Math.Min(settings.MaxTotalChanceTokeepPips, 100);
                test = Math.Min(test, cap);
                Fields.KeptPip = UnityEngine.Random.Range(1, 100) < test;
                if (__instance.weightClass == WeightClass.LIGHT && settings.LightKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent) {
                    Fields.LoosePip = true;
                }
                else if (__instance.weightClass == WeightClass.MEDIUM && settings.MediumKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent) {
                    Fields.LoosePip = true;
                }
                else if (__instance.weightClass == WeightClass.HEAVY && settings.HeavyKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent) {
                    Fields.LoosePip = true;
                }
                else if (__instance.weightClass == WeightClass.ASSAULT && settings.AssaultKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent) {
                    Fields.LoosePip = true;
                }
                else if (!Fields.KeptPip) {
                    Fields.LoosePip = true;
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }
}