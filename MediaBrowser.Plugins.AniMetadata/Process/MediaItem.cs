﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class MediaItem : IMediaItem
    {
        private readonly ImmutableDictionary<string, ISourceData> sourceData;

        /// <summary>
        ///     Create a new <see cref="MediaItem" />
        /// </summary>
        /// <param name="JellyfinData">The name of the item as originally provided by Jellyfin</param>
        /// <param name="itemType">The type of the media item</param>
        /// <param name="sourceData">The metadata from the source used to initially identify this media item</param>
        public MediaItem(IJellyfinItemData JellyfinData, IMediaItemType itemType, ISourceData sourceData)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            this.JellyfinData = JellyfinData ?? throw new ArgumentNullException(nameof(JellyfinData));
            this.ItemType = itemType;

            this.sourceData = ImmutableDictionary<string, ISourceData>.Empty.Add(sourceData.Source.Name, sourceData);
        }

        private MediaItem(IJellyfinItemData JellyfinData, IMediaItemType itemType, ImmutableDictionary<string, ISourceData> sourceData)
        {
            this.JellyfinData = JellyfinData;
            this.ItemType = itemType;
            this.sourceData = sourceData;
        }

        public IJellyfinItemData JellyfinData { get; }

        public IMediaItemType ItemType { get; }

        public Either<ProcessFailedResult, IMediaItem> AddData(ISourceData sourceData)
        {
            if (this.sourceData.ContainsKey(sourceData.Source.Name))
            {
                var failedResult = new ProcessFailedResult(
                    sourceData.Source.Name,
                    sourceData.Identifier.Name,
                    this.ItemType,
                    "Cannot add data for a source more than once");

                return Left<ProcessFailedResult, IMediaItem>(failedResult);
            }

            return Right<ProcessFailedResult, IMediaItem>(
                new MediaItem(this.JellyfinData, this.ItemType, this.sourceData.Add(sourceData.Source.Name, sourceData)));
        }

        public IEnumerable<ISourceData> GetAllSourceData()
        {
            return this.sourceData.Values;
        }

        public Option<ISourceData> GetDataFromSource(ISource source)
        {
            if (this.sourceData.TryGetValue(source.Name, out var sourceData) == false &&
                source.ShouldUsePlaceholderSourceData(this.ItemType) &&
                this.sourceData.FirstOrDefault().Value is ISourceData placeholderSourceData)
            {
                sourceData = placeholderSourceData;
            }

            return Option<ISourceData>.Some(sourceData);
        }
    }
}