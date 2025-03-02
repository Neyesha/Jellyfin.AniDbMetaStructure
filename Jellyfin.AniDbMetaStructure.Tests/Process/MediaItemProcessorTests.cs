﻿using System.Linq;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Tests.Process
{
    [TestFixture]
    public class MediaItemProcessorTests
    {
        [SetUp]
        public virtual void Setup()
        {
            this.PluginConfiguration = Substitute.For<IPluginConfiguration>();

            this.MediaItemBuilder = Substitute.For<IMediaItemBuilder>();
            this.MediaItemBuilder.BuildMediaItem(Arg.Any<IMediaItem>())
                .Returns(x => Right<ProcessFailedResult, IMediaItem>(x.Arg<IMediaItem>()));

            this.MediaItemType = Substitute.For<IMediaItemType<Series>>();
            this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, Arg.Any<IMediaItem>(), Arg.Any<ILogger>())
                .Returns(x => Right<ProcessFailedResult, IMetadataFoundResult<Series>>(new MetadataFoundResult<Series>(
                    x.Arg<IMediaItem>(), new MetadataResult<Series>
                    {
                        Item = new Series()
                    })));

            this.Processor = new MediaItemProcessor(this.PluginConfiguration, this.MediaItemBuilder, new ConsoleLogger());
        }

        internal static class Data
        {
            public static IMediaItem MediaItem()
            {
                return Substitute.For<IMediaItem>();
            }

            public static ItemLookupInfo JellyfinInfo()
            {
                return new ItemLookupInfo
                {
                    IndexNumber = 1,
                    ParentIndexNumber = 2,
                    Name = "name"
                };
            }
        }

        internal IMediaItemType<Series> MediaItemType;
        internal IPluginConfiguration PluginConfiguration;
        internal IMediaItemBuilder MediaItemBuilder;
        internal MediaItemProcessor Processor;

        [TestFixture]
        public class GetResultAsync : MediaItemProcessorTests
        {
            [SetUp]
            public override void Setup()
            {
                base.Setup();

                this.mediaItem = Data.MediaItem();
                this.JellyfinInfo = Data.JellyfinInfo();

                this.MediaItemBuilder.Identify(Arg.Any<JellyfinItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(this.mediaItem));
            }

            private ItemLookupInfo JellyfinInfo;
            private IMediaItem mediaItem;

            [Test]
            public async Task BuildsMediaItem()
            {
                this.MediaItemBuilder.Identify(Arg.Any<JellyfinItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(this.mediaItem));

                var result = await this.Processor.GetResultAsync(this.JellyfinInfo, this.MediaItemType, Enumerable.Empty<JellyfinItemId>());

                result.IsRight.Should().BeTrue();
                await this.MediaItemBuilder.Received(1).BuildMediaItem(this.mediaItem);
            }

            [Test]
            public async Task CreatesResult()
            {
                var JellyfinInfo = Data.JellyfinInfo();
                var mediaItem = Data.MediaItem();
                var builtMediaItem = Data.MediaItem();

                this.MediaItemBuilder.Identify(Arg.Any<JellyfinItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));
                this.MediaItemBuilder.BuildMediaItem(mediaItem)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(builtMediaItem));

                var result = await this.Processor.GetResultAsync(JellyfinInfo, this.MediaItemType, Enumerable.Empty<JellyfinItemId>());

                result.IsRight.Should().BeTrue();
                result.IfRight(r => r.MediaItem.Should().Be(builtMediaItem));
            }

            [Test]
            public async Task IdentifiesItem()
            {
                var JellyfinInfo = Data.JellyfinInfo();
                var mediaItem = Data.MediaItem();

                this.MediaItemBuilder.Identify(Arg.Any<JellyfinItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));

                var result = await this.Processor.GetResultAsync(JellyfinInfo, this.MediaItemType, Enumerable.Empty<JellyfinItemId>());

                result.IsRight.Should().BeTrue();

                await this.MediaItemBuilder.Received(1)
                    .Identify(Arg.Is<JellyfinItemData>(d => d.Identifier.Index == 1 &&
                                                             d.Identifier.ParentIndex == 2 &&
                                                             d.Identifier.Name == "name"), this.MediaItemType);
            }
        }
    }
}