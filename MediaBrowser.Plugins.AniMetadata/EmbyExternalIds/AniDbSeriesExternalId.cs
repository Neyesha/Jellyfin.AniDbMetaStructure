﻿using Jellyfin.AniDbMetaStructure.Process.Sources;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.AniDbMetaStructure.JellyfinExternalIds
{
    public class AniDbSeriesExternalId : IExternalId
    {
        public bool Supports(IHasProviderIds item)
        {
            return item is Series;
        }

        public string ProviderName => SourceNames.AniDb;

        public string Key => SourceNames.AniDb;

        public string UrlFormatString => "http://anidb.net/perl-bin/animedb.pl?show=anime&aid={0}";

        public ExternalIdMediaType? Type => ExternalIdMediaType.Series
            ;
    }
}