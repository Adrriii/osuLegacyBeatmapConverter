
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Osu;
using System;
using System.IO;

namespace osuLegacyBeatmapConverter
{
    class Program {
        static int Main (string[] args)
        {
            if(args == null)
            {
                Console.Write("Missing arguments: [inputMap.osu] [outputMap] {forceKeys}\n");
                return 1;
            }

            LegacyBeatmapDecoder legacyBeatmapDecoder = new LegacyBeatmapDecoder();
            IBeatmap beatmap = null;
            

            try
            {
                using (StreamReader sr = new StreamReader(args[0]))
                {
                    try
                    {
                        beatmap = legacyBeatmapDecoder.Decode(sr);
                    }
                    catch (Exception e)
                    {
                        Console.Write("Could not convert input file:\n" + e);
                        return 1;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Could not read input file:\n" + e);
            }

            if (beatmap == null)
            {
                Console.Write("Failed");
                return 1;
            }

            IBeatmap convertedBeatmap = null;
            ManiaBeatmapConverter maniaBeatmapConverter = null;
            try
            {
                beatmap.BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;
                maniaBeatmapConverter = new ManiaBeatmapConverter(beatmap);

                // Third argument is optionnal and can force TargetColumns
                if (args.Length > 2 )
                {
                    maniaBeatmapConverter.TargetColumns = Int32.Parse(args[2]);
                }
                    
                convertedBeatmap = maniaBeatmapConverter.Convert();
            } catch (Exception e)
            { 
                Console.Write("Could not convert input file:\n" + e);
                return 1;
            }

            int columns = maniaBeatmapConverter.TargetColumns;
            using (StreamWriter writer = new StreamWriter(args[1]))
            {
                writer.Write("[Difficulty]\n");
                writer.Write("CircleSize: "+ columns + "\n");

                writer.Write("[HitObjects]\n");
                String current = "";
                foreach (ManiaHitObject maniaHitObject in convertedBeatmap.HitObjects)
                {
                    int columnPosition = (((maniaHitObject.Column) * (512 / columns)) + (256 / columns));
                    switch (maniaHitObject.GetType().Name)
                    {
                        case "Note":
                            current = columnPosition + ",192," + maniaHitObject.StartTime.ToString().Split(",")[0].Split(".")[0] + ",1,0,0:0:0:0:0\n";
                            break;
                        case "HoldNote":
                            current = columnPosition + ",192," + maniaHitObject.StartTime.ToString().Split(",")[0].Split(".")[0] + ",128,0,"+(maniaHitObject as HoldNote).EndTime.ToString().Split(",")[0].Split(".")[0] + ":0:0:0:0\n";
                            break;
                    }
                    writer.Write(current);

                }
            }

            return 0;
        }
    }
}