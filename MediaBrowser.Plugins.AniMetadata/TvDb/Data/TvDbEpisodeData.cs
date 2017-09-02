﻿using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    public class TvDbEpisodeData
    {
        public TvDbEpisodeData(int id, string episodeName, Maybe<long> absoluteNumber, int airedEpisodeNumber,
            int airedSeason, int lastUpdated)
        {
            Id = id;
            EpisodeName = episodeName;
            AbsoluteNumber = absoluteNumber;
            AiredEpisodeNumber = airedEpisodeNumber;
            AiredSeason = airedSeason;
            LastUpdated = lastUpdated;
        }

        public int Id { get; }

        public string EpisodeName { get; }

        public Maybe<long> AbsoluteNumber { get; }

        public int AiredEpisodeNumber { get; }

        public int AiredSeason { get; }

        public int LastUpdated { get; }
    }
}