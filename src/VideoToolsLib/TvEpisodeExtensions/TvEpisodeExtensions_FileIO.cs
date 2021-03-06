﻿
namespace VideoTools
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    public static partial class TvEpisodeExtensions
    {
        public static string GetOutputFileName(this TvEpisode item)
        {
            var result = item.SafeShowName;

            if (item.EpisodeNumber != Helpers.InvalidEpisodeNumber
                && item.SeasonNumber != Helpers.InvalidSeasonNumber)
            {
                result = $"{item.SafeShowName} s{item.SeasonNumber:D2}e{item.EpisodeNumber:D2}";
            }
            else
            {
                if (item.AiredTime != DateTime.MinValue)
                {
                    string aired = item.AiredTime.ToString("yyyy-MM-dd HH:mm");
                    result = $"{item.SafeShowName} {aired}";
                }
            }

            return Helpers.MakeFileNameSafe(result, '-');
        }

        public static void SaveNfoFile(this TvEpisode item, string path)
        {
            var episode = new XElement("episodedetails");

            if (item.SeasonNumber != Helpers.InvalidSeasonNumber && item.EpisodeNumber != Helpers.InvalidEpisodeNumber)
            {
                episode.Add(
                    new XElement("season", item.SeasonNumber),
                    new XElement("episode", item.EpisodeNumber)
                    );
            }
            if (String.IsNullOrWhiteSpace(item.ShowName) == false)
            {
                episode.Add(new XElement("showtitle", item.ShowName));
            }
            episode.Add(new XElement("originaltitle", item.OriginalTitle));
            episode.Add(new XElement("title", item.Title));
            if (String.IsNullOrWhiteSpace(item.Description) == false)
            {
                episode.Add(new XElement("plot", item.Description));
            }
            if (item.AiredTime != DateTime.MinValue)
            {
                episode.Add(new XElement("aired", item.AiredTime));
            }
            if (String.IsNullOrWhiteSpace(item.Genre) == false)
            {
                episode.Add(new XElement("genre", item.Genre));
            }
            if (String.IsNullOrWhiteSpace(item.Channel) == false)
            {
                episode.Add(new XElement("studio", item.Channel));
            }
            if (String.IsNullOrWhiteSpace(item.Credits) == false)
            {
                episode.Add(new XElement("credits", item.Credits));
            }
            if (item.OriginalAirDate != DateTime.MinValue)
            {
                episode.Add(new XElement("premiered", item.OriginalAirDate.Date));
            }

            if (String.IsNullOrWhiteSpace(item.ThumbnailFile) == false)
            {
                var thumb = Path.GetFileName(item.ThumbnailFile);
                episode.Add(new XElement("thumb", thumb));
            }

            var xml = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    episode
                );

            xml.Save(path);
        }

        public static void SaveShowNfoFile(this TvEpisode item, string path)
        {
            var show = new XElement("tvshow");

            show.Add(new XElement("title", item.ShowName));
            show.Add(new XElement("uniqueid",
                new XAttribute("type", "unknown"),
                new XAttribute("default", "true")));

            var xml = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    show
                );

            xml.Save(path);

            if (String.IsNullOrWhiteSpace(item.KodiUrl) == false)
            {
                File.AppendAllText(path, Environment.NewLine + item.KodiUrl);
            }
        }
    }
}
