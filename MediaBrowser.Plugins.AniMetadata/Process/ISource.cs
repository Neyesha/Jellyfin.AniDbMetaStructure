﻿using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    /// <summary>
    ///     A source of metadata
    /// </summary>
    internal interface ISource
    {
        SourceName Name { get; }

        Either<ProcessFailedResult, IJellyfinSourceDataLoader> GetJellyfinSourceDataLoader(IMediaItemType mediaItemType);

        bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType);
    }
}