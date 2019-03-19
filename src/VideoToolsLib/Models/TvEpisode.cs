
namespace VideoTools
{
    using System;

    public class TvEpisode
    {
        public int SeasonNumber { get; set; }               // <season></season>                                                            TVSeasonNum
        public int EpisodeNumber { get; set; }              // <episode></episode>                                                          TVEpisode
        public string ShowName { get; set; }                // <showtitle></showtitle>          Title                                       TVShowName
        public string SafeShowName { get; set; }            //
        public string OriginalTitle { get; set; }           // <originaltitle></originaltitle>  Title                                       title
        public string Title { get; set; }                   // <title></title>                  Title                                       title
        public string Description { get; set; }             // <plot></plot>                    WM/SubTitleDescription                      description
        public TimeSpan Duration { get; set; }              //                                  Duration
        public string Channel { get; set; }                 // <studio></studio>                WM/MediaStationCallSign                     TVNetwork
        public string Genre { get; set; }                   // <genre></genre>                  WM/Genre                                    genre
        public string Credits { get; set; }                 // <credits></credits>              WM/MediaCredits                             comment
        public DateTime AiredTime { get; set; }             // <aired></aired>                  WM/WMRVEncodeTime                           year (UTC)
        public DateTime OriginalAirDate { get; set; }       // <premiered></premiered>          WM/MediaOriginalBroadcastDateTime
        public string ThumbnailFile { get; set; }           // <thumb></thumb>

        public override string ToString()
        {
            return (new { ShowName, S = SeasonNumber, Ep = EpisodeNumber, Title = $"\"{Title}\"", Summary = $"\"{Description}\"", Channel, AiredTime, Duration }).ToString();
        }
    }
}
