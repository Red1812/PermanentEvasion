This mod is based on Morphyum's PermanentEvasion (PE).
Orginal mod can be find here:
https://github.com/Morphyum/PermanentEvasion

I liked the idea of donZappo's Semi-PermanentEvasion (SPE) regarding the limitation on permanent evasion pip so I added it to Morphyum's mod.
You can find donZappo's mod here:
https://github.com/donZappo/Semi_Permanent_Evasion

Any restrictions applying to their mod apply to mine.

All options are in settings.json.
Here the things that changed compared to PE:
- When a Mech is hit it will loose a pip. I modified the message to display HIT: -1 EVASION instead of the usual -1 EVASION. Can be deactivated, change AllowHitStrip to false. Default:
		"AllowHitStrip": true
- Like in SPE, you can set how many permanent pips each weight category can have. By default they are as follow:
        "LightKeepPipsCount" : 5,
        "MediumKeepPipsCount" : 4,
        "HeavyKeepPipsCount" : 3,
        "AssaultKeepPipsCount" : 2,
- You can configure the chance (in percent) of a permaent pip to not be strip on a miss. Ace pilot can give a bonus. It will display -1 EVASION when that happen like in the non modded game. Default value are:		
        "PercentageToKeepPips" : 50,
        "AcePilotBonusPercentage" : 20,
- AcePilot can also give a bonus permanent pip. Default value is:
        "AcePilotBonusPips" : 1,
- If you don't like the flat percentage for the chance of loosing a permanent pip I made a different version based on Piloting skill. The formula is the following "Pilot skill" x "PerSkillPointToKeepPips" or "Pilot skill" x "AcePilotPointToKeepPips" if the pilot has AcePilot. Default value are:
		"PerSkillPointToKeepPips" : 7,
		"AcePilotPointToKeepPips" : 8,
- To use that feature just put "PilotSkillToKeepPips" : false, to true.
- You can set the max chance to keep your permanent evasion pip so there is always a chance to loose your permanent pip. Default value is:
		"MaxTotalChanceTokeepPips" : 90
- If you succeed to keep to your permanent pip you will see "EVASION KEPT"
- You can set a minimum amout of damage received for stripping a pip after getting hit. That should mitigate the missile volley effect:
		"MinDamageForEvasionStrip": 25
- Added back donZappo Semi-Permanent Evasion pips retain system based on Mech speed. To enable it put "UseMovement" to true. The default settings are the following:

Move 210 - 5 Permanent Evasion
Move 190 - 4 Permanent Evasion
Move 165 - 4 Permanent Evasion
Move 140 - 4 Permanent Evasion
Move 120 - 3 Permanent Evasion
Move 95 - 2 Permanent Evasion

- You can now give a bonus permanent pip for jumping, can optionnaly be linked to AcePilot. Default value is:
		"JumpBonusPip": 1
		"LinkedToAcePilot": true

I hope this is clear enough.
