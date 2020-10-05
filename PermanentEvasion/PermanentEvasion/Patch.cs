using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PermanentEvasion
{

    [HarmonyPatch(typeof(AbstractActor), "ResolveAttackSequence")]
    public static class AbstractActor_ResolveAttackSequence
    {

        static bool Prefix(AbstractActor __instance)
        {
            return false;
        }

        static void Postfix(AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection)
        {
            try
            {

                AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
                if (attackSequence != null && attackSequence.GetAttackDidDamage(__instance.GUID))
                {
                    List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance).FindAll((Effect x) => x.EffectData.targetingData.effectTriggerType == EffectTriggerType.OnDamaged);
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].OnEffectTakeDamage(attackSequence.attacker, __instance);
                    }
                    if (attackSequence.isMelee)
                    {
                        int value = attackSequence.attacker.StatCollection.GetValue<int>("MeleeHitPushBackPhases");
                        if (value > 0)
                        {
                            for (int j = 0; j < value; j++)
                            {
                                __instance.ForceUnitOnePhaseDown(sourceID, stackItemID, false);
                            }
                        }
                    }
                }
                int evasivePipsCurrent = __instance.EvasivePipsCurrent;
                var settings = PermanentEvasion.Settings;
                float totalDamageReceived = 1;
                if (attackSequence.GetAttackDidDamage(__instance.GUID))
                {
                    totalDamageReceived += attackSequence.GetArmorDamageDealt(__instance.GUID) + attackSequence.GetStructureDamageDealt(__instance.GUID);
                    if ((totalDamageReceived > settings.MinDamageForEvasionStrip) && settings.AllowHitStrip)
                    {
                        __instance.ConsumeEvasivePip(true);
                        Fields.LoosePip = false;
                    }
                    else if (Fields.LoosePip)
                    {
                        __instance.ConsumeEvasivePip(true);
                        Fields.LoosePip = false;
                    }
                }
                else if (Fields.LoosePip)
                {
                    __instance.ConsumeEvasivePip(true);
                    Fields.LoosePip = false;
                }
                int evasivePipsCurrent2 = __instance.EvasivePipsCurrent;
                if (evasivePipsCurrent2 < evasivePipsCurrent && (totalDamageReceived > settings.MinDamageForEvasionStrip) && settings.AllowHitStrip && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "HIT: -1 EVASION", FloatieMessage.MessageNature.Debuff));
                }
                else if (evasivePipsCurrent2 < evasivePipsCurrent && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "-1 EVASION", FloatieMessage.MessageNature.Debuff));
                }
                else if (evasivePipsCurrent2 > 0 && Fields.KeptPip && !__instance.IsDead && !__instance.IsFlaggedForDeath)
                {
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "EVASION KEPT", FloatieMessage.MessageNature.Buff));
                    Fields.KeptPip = false;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }

    [HarmonyPatch(typeof(Mech), "ResolveAttackSequence")]
    public static class Mech_ResolveAttackSequence
    {
        static void Prefix(Mech __instance)
        {
            var settings = PermanentEvasion.Settings;
            try
            {
                bool acepilot = false;
                foreach (Ability ab in __instance.pilot.Abilities)
                {
                    if (ab.Def.Description.Id == "AbilityDefP8")
                    {
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
                        test += __instance.SkillPiloting * Math.Max(acePilotPointToKeepPips, perSkillPointToKeepPips);
                        acePipsBonus += settings.AcePilotBonusPips;
                    }
                }
                else
                {
                    test = settings.PercentageToKeepPips;
                    if (acepilot)
                    {
                        test += settings.AcePilotBonusPercentage;
                        acePipsBonus += settings.AcePilotBonusPips;
                    }
                }
                if (settings.LinkedToAcePilot)
                {
                    if (__instance.HasMovedThisRound && __instance.JumpedLastRound && acepilot)
                    {
                        acePipsBonus += settings.JumpBonusPip;
                    }
                }
                else if (__instance.HasMovedThisRound && __instance.JumpedLastRound)
                {
                    acePipsBonus += settings.JumpBonusPip;
                }
                int cap = Math.Min(settings.MaxTotalChanceTokeepPips, 100);
                test = Math.Min(test, cap);
                var random = new Random();
                Fields.KeptPip = random.Next(1, 100) < test;
                if (!settings.UseMovement)
                {
                    if (__instance.weightClass == WeightClass.LIGHT && settings.LightKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.weightClass == WeightClass.MEDIUM && settings.MediumKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.weightClass == WeightClass.HEAVY && settings.HeavyKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.weightClass == WeightClass.ASSAULT && settings.AssaultKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (!Fields.KeptPip)
                    {
                        Fields.LoosePip = true;
                    }
                }
                else
                {
                    if (__instance.MaxWalkDistance == 210f && settings.Movement210KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 190f && settings.Movement190KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 165f && settings.Movement165KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 140f && settings.Movement140KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 120f && settings.Movement120KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 95f && settings.Movement95KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (!Fields.KeptPip)
                    {
                        Fields.LoosePip = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "ResolveAttackSequence")]
    public static class Vehicle_ResolveAttackSequence
    {
        static void Prefix(Vehicle __instance)
        {
            var settings = PermanentEvasion.Settings;
            try
            {
                bool acepilot = false;
                foreach (Ability ab in __instance.pilot.Abilities)
                {
                    if (ab.Def.Description.Id == "AbilityDefP8")
                    {
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
                        test += __instance.SkillPiloting * Math.Max(acePilotPointToKeepPips, perSkillPointToKeepPips);
                        acePipsBonus += settings.AcePilotBonusPips;
                    }
                }
                else
                {
                    test = settings.PercentageToKeepPips;
                    if (acepilot)
                    {
                        test += settings.AcePilotBonusPercentage;
                        acePipsBonus += settings.AcePilotBonusPips;
                    }
                }
                int cap = Math.Min(settings.MaxTotalChanceTokeepPips, 100);
                test = Math.Min(test, cap);
                var random = new Random();
                Fields.KeptPip = random.Next(1, 100) < test;
                if (!settings.UseMovement)
                {
                    if (__instance.weightClass == WeightClass.LIGHT && settings.LightKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.weightClass == WeightClass.MEDIUM && settings.MediumKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.weightClass == WeightClass.HEAVY && settings.HeavyKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.weightClass == WeightClass.ASSAULT && settings.AssaultKeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (!Fields.KeptPip)
                    {
                        Fields.LoosePip = true;
                    }
                }
                else
                {
                    if (__instance.MaxWalkDistance == 210f && settings.Movement210KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 190f && settings.Movement190KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 165f && settings.Movement165KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 140f && settings.Movement140KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 120f && settings.Movement120KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (__instance.MaxWalkDistance == 95f && settings.Movement95KeepPipsCount + acePipsBonus < __instance.EvasivePipsCurrent)
                    {
                        Fields.LoosePip = true;
                    }
                    else if (!Fields.KeptPip)
                    {
                        Fields.LoosePip = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
