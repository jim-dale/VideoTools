
namespace VideoTools
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    public static partial class TvEpisodeExtensions
    {
        public static string GetConversionOutputFileName(this TvEpisode item)
        {
            var result = default(string);

            if (item.EpisodeNumber != Helpers.InvalidEpisodeNumber
                && item.SeasonNumber != Helpers.InvalidSeasonNumber)
            {
                result = $"{item.ShowName} s{item.SeasonNumber}e{item.EpisodeNumber}";
            }
            else
            {
                string aired = item.AiredTime.ToString("yyyy-MM-dd HH:mm");
                result = $"{item.ShowName} {aired}";
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
            episode.Add(new XElement("title", item.Title));
            episode.Add(new XElement("plot", item.Description));
            episode.Add(new XElement("aired", item.AiredTime));
            episode.Add(new XElement("genre", item.Genre));
            episode.Add(new XElement("studio", item.Channel));
            episode.Add(new XElement("credits", item.Credits));
            if (item.OriginalAirDate != DateTime.MinValue)
            {
                episode.Add(new XElement("premiered", item.OriginalAirDate));
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
    }
}
