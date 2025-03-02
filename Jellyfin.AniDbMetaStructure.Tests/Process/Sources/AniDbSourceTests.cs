﻿using FluentAssertions;
using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Tests.Process.Sources
{
    [TestFixture]
    public class AniDbSourceTests
    {
        [SetUp]
        public virtual void Setup()
        {
            this.aniDbClient = Substitute.For<IAniDbClient>();
            this.configuration = Substitute.For<ITitlePreferenceConfiguration>();
            this.titleSelector = Substitute.For<IAniDbTitleSelector>();

            this.configuration.TitlePreference.Returns(TitleType.Localized);
            this.loaders = new List<IJellyfinSourceDataLoader>();

            this.aniDbSource = new AniDbSource(this.aniDbClient, this.configuration, this.titleSelector, this.loaders);
        }

        private IAniDbClient aniDbClient;
        private ITitlePreferenceConfiguration configuration;
        private IAniDbTitleSelector titleSelector;
        private AniDbSource aniDbSource;
        private IList<IJellyfinSourceDataLoader> loaders;

        private JellyfinItemData JellyfinItemData(string name, int? parentAniDbSeriesId)
        {
            var parentIds = new List<JellyfinItemId>();

            if (parentAniDbSeriesId.HasValue)
            {
                parentIds.Add(new JellyfinItemId(MediaItemTypes.Series, SourceNames.AniDb, parentAniDbSeriesId.Value));
            }

            return new JellyfinItemData(MediaItemTypes.Series,
                new ItemIdentifier(Option<int>.None, Option<int>.None, name),
                null, "en", parentIds);
        }

        [Test]
        public void Name_ReturnsAniDbSourceName()
        {
            this.aniDbSource.Name.Should().BeSameAs(SourceNames.AniDb);
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetJellyfinSourceDataLoader_MatchingLoader_ReturnsLoader(IMediaItemType mediaItemType)
        {
            var loader = Substitute.For<IJellyfinSourceDataLoader>();
            loader.SourceName.Returns(SourceNames.AniDb);
            loader.CanLoadFrom(mediaItemType).Returns(true);

            this.loaders.Add(loader);

            var result = this.aniDbSource.GetJellyfinSourceDataLoader(mediaItemType);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(loader));
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetJellyfinSourceDataLoader_NoMatchingLoader_ReturnsFailed(IMediaItemType mediaItemType)
        {
            var sourceMismatch = Substitute.For<IJellyfinSourceDataLoader>();
            sourceMismatch.SourceName.Returns(SourceNames.TvDb);
            sourceMismatch.CanLoadFrom(mediaItemType).Returns(true);

            var cannotLoad = Substitute.For<IJellyfinSourceDataLoader>();
            cannotLoad.SourceName.Returns(SourceNames.AniDb);
            cannotLoad.CanLoadFrom(mediaItemType).Returns(false);

            this.loaders.Add(sourceMismatch);
            this.loaders.Add(cannotLoad);

            var result = this.aniDbSource.GetJellyfinSourceDataLoader(mediaItemType);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No Jellyfin source data loader for this source and media item type"));
        }

        [Test]
        public async Task GetSeriesData_NoAniDbIdOnParent_ReturnsFailed()
        {
            var jellyfinItemData = JellyfinItemData("Name", null);

            var result = await this.aniDbSource.GetSeriesData(jellyfinItemData, new ProcessResultContext(string.Empty, string.Empty, null));

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No AniDb Id found on parent series"));
        }

        [Test]
        public async Task GetSeriesData_NoSeriesLoaded_ReturnsFailed()
        {
            var jellyfinItemData = JellyfinItemData("Name", 56);

            this.aniDbClient.GetSeriesAsync(56).Returns(Option<AniDbSeriesData>.None);

            var result = await this.aniDbSource.GetSeriesData(jellyfinItemData, new ProcessResultContext(string.Empty, string.Empty, null));

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with AniDb Id '56'"));
        }

        [Test]
        public async Task GetSeriesData_ReturnsSeries()
        {
            var jellyfinItemData = JellyfinItemData("Name", 56);

            var seriesData = new AniDbSeriesData();

            this.aniDbClient.GetSeriesAsync(56).Returns(Option<AniDbSeriesData>.Some(seriesData));

            var result = await this.aniDbSource.GetSeriesData(jellyfinItemData, new ProcessResultContext(string.Empty, string.Empty, null));

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(seriesData));
        }

        [Test]
        public void SelectTitle_TitleSelected_ReturnsTitle()
        {
            var titles = new ItemTitleData[] { };

            this.titleSelector.SelectTitle(titles, TitleType.Localized, "en")
                .Returns(Option<ItemTitleData>.Some(new ItemTitleData
                {
                    Title = "TitleName"
                }));

            var result = this.aniDbSource.SelectTitle(titles, "en", new ProcessResultContext(string.Empty, string.Empty, null));

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().Be("TitleName"));
        }

        [Test]
        public void SelectTitle_NoTitleSelected_ReturnsFailed()
        {
            var titles = new ItemTitleData[] { };

            this.titleSelector.SelectTitle(titles, TitleType.Localized, "en")
                .Returns(Option<ItemTitleData>.None);

            var result = this.aniDbSource.SelectTitle(titles, "en", new ProcessResultContext(string.Empty, string.Empty, null));

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find a title"));
        }
    }
}