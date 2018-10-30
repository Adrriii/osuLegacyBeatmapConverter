
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
                Console.ReadKey();
                return 1;
            }

            Console.Write("Converting '"+beatmap.BeatmapInfo.Metadata.Artist + " - "+ beatmap.BeatmapInfo.Metadata.Title + " ["+ beatmap.BeatmapInfo.Version + "]'\n");

            IBeatmap convertedBeatmap = null;
            ManiaBeatmapConverter maniaBeatmapConverter = null;
            try
            {
                beatmap.BeatmapInfo.Ruleset = new OsuRuleset().RulesetInfo;
                maniaBeatmapConverter = new ManiaBeatmapConverter(beatmap);
                convertedBeatmap = maniaBeatmapConverter.Convert();
            } catch (Exception e)
            { 
                Console.Write("Could not convert input file:\n" + e);
                Console.ReadKey();
                return 1;
            }

            int columns = maniaBeatmapConverter.TargetColumns;
            using (StreamWriter writer = new StreamWriter(args[1]))
            {
                writer.Write("[HitObjects]\n");
                foreach (ManiaHitObject maniaHitObject in convertedBeatmap.HitObjects)
                {
                    int columnPosition = (((maniaHitObject.Column) * (512 / columns)) + (256 / columns));
                    switch (maniaHitObject.GetType().Name)
                    {
                        case "Note":
                            writer.Write(columnPosition + ",192," + maniaHitObject.StartTime+",0:0:0:0\n");
                            break;
                        case "HoldNote":
                            writer.Write(columnPosition + ",192," + maniaHitObject.StartTime+","+(maniaHitObject as HoldNote).EndTime+":0:0:0\n");
                            break;
                    }
                    
                }
            }

            Console.Write("Finished. "+ columns + "k\n");
            Console.ReadKey();

            return 0;
        }
    }
}