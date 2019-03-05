
namespace VideoTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using TvDbSharper;
    using TvDbSharper.Dto;

    public class TheTVDBClient
    {
        public const int InvalidSeriesId = -1;

        private TvDbClient _client;
        private string _episodesCacheFolder;

        public void SetEpisodesCacheFolder(string path)
        {
            _episodesCacheFolder = Environment.ExpandEnvironmentVariables(path);
        }

        private async Task InitialiseAsync()
        {
            if (_client is null)
            {
                _client = new TvDbClient();

                var credentials = Environment.GetEnvironmentVariable("THETVDB_CREDENTIALS");
                var parts = credentials.Split('!');
                if (parts.Length == 3)
                {
                    await _client.Authentication.AuthenticateAsync(parts[0], parts[1], parts[2]);
                }
            }
        }

        public async Task<int> GetSeriesIdByNameAsync(string title)
        {
            int result = InvalidSeriesId;

            await InitialiseAsync();

            var items = await _client.Search.SearchSeriesByNameAsync(title);
            if (items.Data.Any())
            {
                var series = items.Data.First();

                result = series.Id;
            }

            return result;
        }

        public async Task<List<EpisodeRecord>> GetAllSeasonEpisodes(string title, int id, int seriesNumber)
        {
            var result = default(List<EpisodeRecord>);

            var fileName = $"{title} s{seriesNumber}.json";
            var path = Path.Combine(_episodesCacheFolder, fileName);

            if (File.Exists(path))
            {
                result = LoadEpisodesFromFile(path);
            }
            else
            {
                result = await GetAllSeasonEpisodes(id, seriesNumber);

                SaveEpisodesToFile(result, path);
            }

            return result;
        }

        private async Task<List<EpisodeRecord>> GetAllSeasonEpisodes(int id, int season)
        {
            var result = new List<EpisodeRecord>();

            int page = 1;
            int lastPage = 1;

            do
            {
                var response = await _client.Series.GetEpisodesAsync(id, page, new EpisodeQuery { AiredSeason = season });

                if (page == 1 && response.Links?.Last != null)
                {
                    lastPage = response.Links.Last.Value;
                }
                if (response.Data.Any())
                {
                    result.AddRange(response.Data);
                }
                ++page;

            } while (page <= lastPage);

            return result;
        }

        private void SaveEpisodesToFile(IList<EpisodeRecord> items, string path)
        {
            var contents = JsonConvert.SerializeObject(items);
            File.WriteAllText(path, contents);
        }

        private List<EpisodeRecord> LoadEpisodesFromFile(string path)
        {
            var contents = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<EpisodeRecord>>(contents);
        }
    }
}
