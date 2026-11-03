using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace LessDeadlyDependency
{
    // Turns each dependency hediff's coma stage into a non-lethal "chronic deficiency" plateau, once at startup.
    // Severity keeps climbing until that stage and then stops (maxSeverity), so the pawn stays there until a dose
    // resets it. Done in code rather than XML because the caps come from the settings and XML caches would bake them in.
    [StaticConstructorOnStartup]
    public static class DependencyRework
    {
        private const string HussarHediff = "VREH_GeneticDrugNeed_Luci";
        private const string HussarGene = "VREH_ChemicalDependency_Luciferium";

        static DependencyRework()
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }
            Rework(HediffDefOf.GeneticDrugNeed);

            if (LessDeadlyDependencyMod.HussarIncluded)
            {
                HediffDef hussarHediff = DefDatabase<HediffDef>.GetNamedSilentFail(HussarHediff);
                if (hussarHediff != null && Rework(hussarHediff))
                {
                    // Its description is a copy of the vanilla one, which the Patches folder already rewrote.
                    hussarHediff.description = HediffDefOf.GeneticDrugNeed.description;
                    GeneDef gene = DefDatabase<GeneDef>.GetNamedSilentFail(HussarGene);
                    if (gene != null)
                    {
                        gene.description = "LDD_HussarLuciferiumGeneDesc".Translate();
                        typeof(GeneDef).GetField("cachedDescription", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(gene, null);
                    }
                }
            }
        }

        private static bool Rework(HediffDef def)
        {
            // The coma stage is the first one that caps consciousness below the waking threshold. It has to be at
            // index 2 or later: Hediff_ChemicalDependency.ShouldSatify (the "go take the drug" job and caravan
            // auto-dosing) reads stages[2], which must survive.
            int index = def.stages?.FindIndex(IsComaStage) ?? -1;
            if (index < 2)
            {
                Log.Warning("[Less Deadly Dependency] " + def.defName + " has no coma stage at index 2 or later; left unchanged.");
                return false;
            }

            LessDeadlyDependencySettings settings = LessDeadlyDependencyMod.settings;
            HediffStage stage = def.stages[index];
            stage.label = "LDD_ChronicDeficiency".Translate();
            stage.untranslatedLabel = stage.label;
            stage.overrideLabel = null;
            stage.lifeThreatening = false;
            stage.capMods = new List<PawnCapacityModifier>
            {
                new PawnCapacityModifier { capacity = PawnCapacityDefOf.Consciousness, setMax = settings.consciousnessCap },
                new PawnCapacityModifier { capacity = PawnCapacityDefOf.Moving, setMax = settings.movingCap },
                new PawnCapacityModifier { capacity = PawnCapacityDefOf.Manipulation, setMax = settings.manipulationCap },
                new PawnCapacityModifier { capacity = PawnCapacityDefOf.Sight, setMax = settings.sightCap },
                new PawnCapacityModifier { capacity = PawnCapacityDefOf.Hearing, setMax = settings.hearingCap }
            };
            // Hussar's version has a further stage that sets consciousness to 0, which kills on its own.
            def.stages.RemoveRange(index + 1, def.stages.Count - index - 1);

            def.maxSeverity = stage.minSeverity;
            def.lethalSeverity = -1f;
            return true;
        }

        private static bool IsComaStage(HediffStage stage)
        {
            return stage.capMods != null && stage.capMods.Any(m => m.capacity == PawnCapacityDefOf.Consciousness && m.setMax < LessDeadlyDependencySettings.MinConsciousnessCap);
        }
    }
}
