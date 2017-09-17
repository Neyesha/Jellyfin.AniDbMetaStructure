﻿using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal class FindSeriesRequest : GetRequest<FindSeriesRequest.Response>
    {
        public FindSeriesRequest(string seriesName) : base($"search/series?name={seriesName}")
        {
        }

        public class Response
        {
            public Response(IEnumerable<MatchingSeries> data)
            {
                MatchingSeries = data ?? new List<MatchingSeries>();
            }

            public IEnumerable<MatchingSeries> MatchingSeries { get; }
        }

        public class MatchingSeries
        {
            public MatchingSeries(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }
    }
}