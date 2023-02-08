#nullable enable
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;


namespace HeightsOfSkyrim
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "HeightOfSkyrimPatch.esp")
                .AddRunnabilityCheck(state => Debug.Assert(state.LoadOrder.ContainsKey(ModKey.FromNameAndExtension("Heights_of_Skyrim.esp")), "\n\nYour Heights_of_Skyrim.esp is not in load order or above Synthesis.esp in LO\n\n"))
                .Run(args);
        }

        private static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            ModKey heights = ModKey.FromNameAndExtension("Heights_of_Skyrim.esp");
            state.LoadOrder.TryGetIfEnabledAndExists(heights, out var heightsMod);
            if (heightsMod is null)
            {
                throw new Exception("Your Heights_of_Skyrim.esp is not in load order or above Synthesis.esp in LO");
            }
            foreach (INpcGetter npc in heightsMod.Npcs)
            {
                INpcGetter winningOverride = npc.AsLink().Resolve(state.LinkCache);
                if (Math.Abs(winningOverride.Height - npc.Height) < 0.00001) continue;
                INpc patchedNpc = state.PatchMod.Npcs.GetOrAddAsOverride(winningOverride);
                patchedNpc.Height = npc.Height;
            }
        }
    }
}
