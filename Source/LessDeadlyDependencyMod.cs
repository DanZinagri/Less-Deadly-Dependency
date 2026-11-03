using UnityEngine;
using Verse;

namespace LessDeadlyDependency
{
    public class LessDeadlyDependencySettings : ModSettings
    {
        public const float DefaultConsciousnessCap = 0.65f;
        public const float DefaultMovingCap = 0.7f;
        public const float DefaultManipulationCap = 0.5f;
        public const float DefaultSightCap = 0.9f;
        public const float DefaultHearingCap = 0.9f;

        // Consciousness under 0.3 means unconscious (PawnCapacitiesHandler.CanBeAwake), moving under 0.15 means
        // unable to walk (Moving.minForCapable); the chronic stage must never do either on its own.
        public const float MinConsciousnessCap = 0.3f;
        public const float MinMovingCap = 0.15f;

        public float consciousnessCap = DefaultConsciousnessCap;
        public float movingCap = DefaultMovingCap;
        public float manipulationCap = DefaultManipulationCap;
        public float sightCap = DefaultSightCap;
        public float hearingCap = DefaultHearingCap;
        public bool includeHussarLuciferium;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref consciousnessCap, "consciousnessCap", DefaultConsciousnessCap);
            Scribe_Values.Look(ref movingCap, "movingCap", DefaultMovingCap);
            Scribe_Values.Look(ref manipulationCap, "manipulationCap", DefaultManipulationCap);
            Scribe_Values.Look(ref sightCap, "sightCap", DefaultSightCap);
            Scribe_Values.Look(ref hearingCap, "hearingCap", DefaultHearingCap);
            Scribe_Values.Look(ref includeHussarLuciferium, "includeHussarLuciferium", false);
            consciousnessCap = Mathf.Clamp(consciousnessCap, MinConsciousnessCap, 1f);
            movingCap = Mathf.Clamp(movingCap, MinMovingCap, 1f);
            manipulationCap = Mathf.Clamp01(manipulationCap);
            sightCap = Mathf.Clamp01(sightCap);
            hearingCap = Mathf.Clamp01(hearingCap);
        }

        public void Reset()
        {
            consciousnessCap = DefaultConsciousnessCap;
            movingCap = DefaultMovingCap;
            manipulationCap = DefaultManipulationCap;
            sightCap = DefaultSightCap;
            hearingCap = DefaultHearingCap;
            includeHussarLuciferium = false;
        }
    }

    public class LessDeadlyDependencyMod : Mod
    {
        public const string HussarPackageId = "vanillaracesexpanded.hussar";

        public static LessDeadlyDependencySettings settings;

        public LessDeadlyDependencyMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<LessDeadlyDependencySettings>();
        }

        public static bool HussarActive => ModLister.GetActiveModWithIdentifier(HussarPackageId, ignorePostfix: true) != null;

        public static bool HussarIncluded => settings.includeHussarLuciferium && HussarActive;

        public override string SettingsCategory()
        {
            return "Less Deadly Dependency";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            GUI.color = Color.yellow;
            listing.Label("LDD_RequiresRestart".Translate());
            GUI.color = Color.white;
            listing.Gap();
            listing.Label("LDD_ChronicIntro".Translate());
            listing.Gap();

            listing.Label("LDD_ConsciousnessCap".Translate(settings.consciousnessCap.ToStringPercent()), tooltip: "LDD_ConsciousnessCapTip".Translate());
            settings.consciousnessCap = Step(listing.Slider(settings.consciousnessCap, LessDeadlyDependencySettings.MinConsciousnessCap, 1f));
            listing.Label("LDD_MovingCap".Translate(settings.movingCap.ToStringPercent()), tooltip: "LDD_MovingCapTip".Translate());
            settings.movingCap = Step(listing.Slider(settings.movingCap, LessDeadlyDependencySettings.MinMovingCap, 1f));
            listing.Label("LDD_ManipulationCap".Translate(settings.manipulationCap.ToStringPercent()));
            settings.manipulationCap = Step(listing.Slider(settings.manipulationCap, 0f, 1f));
            listing.Label("LDD_SightCap".Translate(settings.sightCap.ToStringPercent()));
            settings.sightCap = Step(listing.Slider(settings.sightCap, 0f, 1f));
            listing.Label("LDD_HearingCap".Translate(settings.hearingCap.ToStringPercent()));
            settings.hearingCap = Step(listing.Slider(settings.hearingCap, 0f, 1f));
            listing.Gap();

            listing.CheckboxLabeled("LDD_IncludeHussar".Translate(), ref settings.includeHussarLuciferium, "LDD_IncludeHussarTip".Translate());
            if (!HussarActive)
            {
                GUI.color = Color.gray;
                listing.Label("LDD_HussarNotLoaded".Translate());
                GUI.color = Color.white;
            }
            listing.Gap();

            if (listing.ButtonText("LDD_ResetDefaults".Translate(), widthPct: 0.3f))
            {
                settings.Reset();
            }
            listing.End();
        }

        // Sliders snap to 5% steps; both minimums (30%, 15%) are on that grid.
        private static float Step(float value)
        {
            return Mathf.Round(value * 20f) / 20f;
        }
    }
}
