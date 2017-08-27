﻿using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.Tests.TestHelpers;
using MediaBrowser.Plugins.Anime.TvDb;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests.IntegrationTests
{
    [TestFixture]
    [Explicit]
    internal class TvDbTokenIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
        }

        private ILogManager _logManager;

        [Test]
        public async Task GetToken_ExistingToken_DoesNotRequestNewToken()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager);

            var token = new TvDbToken(tvDbConnection, Secrets.Instance.TvDbApiKey, _logManager);

            var token1 = await token.GetTokenAsync();

            var token2 = await token.GetTokenAsync();

            token2.HasValue.Should().BeTrue();
            token2.Value.Should().Be(token1.Value);
        }

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager);

            var token = new TvDbToken(tvDbConnection, "NotValid", _logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.HasValue.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_NoExistingToken_GetsNewToken()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager);

            var token = new TvDbToken(tvDbConnection, Secrets.Instance.TvDbApiKey, _logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.HasValue.Should().BeTrue();
            returnedToken.Value.Should().NotBeNullOrWhiteSpace();
        }
    }
}