using NUnit.Framework;
using RichardSzalay.MockHttp;
using Scraper.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using static UnitTests.HelperFunctions.HelperFunctions;

namespace ScraperLayer
{
    [TestFixture]
    public class ScraperTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        [TestCase(false, 19)]
        [TestCase(true, 18)]
        public void ScrapeSuccessTests(bool filterUpdates, int resultCount)
        {
            MockHttpMessageHandler mockHttp = new();
            mockHttp.When("https://nxbrew.com/").Respond("*/*", GetEmbeddedHtml("nxbrewExample"));

            List<SwitchGame> results = null;
            List<SwitchGame> resultsFromEvent = [];

            using (Scraper.Scraper s = new() { httpClient = mockHttp.ToHttpClient(), FilterUpdates = filterUpdates })
            {
                s.GameRetrieved += (s, e) => resultsFromEvent.Add(e.Game);
                Assert.DoesNotThrowAsync(async () => results = (await s.GetGamesFromPage()).ToList());
            }

            Assert.That(results, Is.Not.Empty);
            Assert.That(results, Has.Count.EqualTo(resultCount));
            Assert.Multiple(() =>
            {
                Assert.That(results.TrueForAll(x => x.Date != default), Is.True);
                Assert.That(results.TrueForAll(x => x.NxDate != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Name != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Categories.Length > 0), Is.True);
            });

            Assert.That(resultsFromEvent.SequenceEqual<SwitchGame>(results), Is.True);
        }

        [Test]
        [TestCase(false, 19)]
        [TestCase(true, 18)]
        public void ScrapeSuccessPagesTests(bool filterUpdates, int resultCount)
        {
            MockHttpMessageHandler mockHttp = new();
            mockHttp.When("https://nxbrew.com/*").Respond("*/*", GetEmbeddedHtml("nxbrewExample"));

            List<SwitchGame> results = null;

            using (Scraper.Scraper s = new() { httpClient = mockHttp.ToHttpClient(), FilterUpdates = filterUpdates })
            {
                Assert.DoesNotThrowAsync(async () => results = (await s.GetGamesFromPage(512)).ToList());
            }

            Assert.That(results, Is.Not.Empty);
            Assert.That(results, Has.Count.EqualTo(resultCount));
            Assert.Multiple(() =>
            {
                Assert.That(results.TrueForAll(x => x.Date != default), Is.True);
                Assert.That(results.TrueForAll(x => x.NxDate != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Name != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Categories.Length > 0), Is.True);
            });
        }

        [Test]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NoContent)]
        [TestCase(HttpStatusCode.Unauthorized)]
        public void ScrapeFailTests(HttpStatusCode httpStatusCode)
        {
            List<SwitchGame> results = null;

            MockHttpMessageHandler mockFailHttp = new();
            mockFailHttp.When("https://nxbrew.com/").Respond(httpStatusCode);

            using (Scraper.Scraper s = new() { httpClient = mockFailHttp.ToHttpClient() })
            {
                Assert.DoesNotThrowAsync(async () => results = (await s.GetGamesFromPage()).ToList());
            }

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void ScrapeCorruptTests()
        {
            List<SwitchGame> results = null;
            int gamesRetrieved = 0;

            MockHttpMessageHandler mockCorruptHttp = new();
            mockCorruptHttp.When("https://nxbrew.com/").Respond("*/*", GetEmbeddedHtml("nxbrewExampleCorrupt"));

            using (Scraper.Scraper s = new() { httpClient = mockCorruptHttp.ToHttpClient() })
            {
                s.GameRetrieved += (s, e) => gamesRetrieved++;
                Assert.DoesNotThrowAsync(async () => results = (await s.GetGamesFromPage()).ToList());
            }

            Assert.That(results, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                Assert.That(results, Has.Count.EqualTo(6));
                Assert.That(gamesRetrieved, Is.EqualTo(6));
            });
            Assert.Multiple(() =>
            {
                Assert.That(results.TrueForAll(x => x.Date != default), Is.True);
                Assert.That(results.TrueForAll(x => x.NxDate != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Name != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Categories.Length > 0), Is.True);
            });
        }

        [Test]
        public void ScrapeWrongTests()
        {
            List<SwitchGame> results = null;
            int gamesRetrieved = 0;

            MockHttpMessageHandler mockWrongHttp = new();
            mockWrongHttp.When("https://nxbrew.com/").Respond("*/*", GetEmbeddedHtml("nxbrewExampleWrong"));

            using (Scraper.Scraper s = new() { httpClient = mockWrongHttp.ToHttpClient() })
            {
                s.GameRetrieved += (s, e) => gamesRetrieved++;
                Assert.DoesNotThrowAsync(async () => results = (await s.GetGamesFromPage()).ToList());
            }

            Assert.Multiple(() =>
            {
                Assert.That(results, Is.Empty);
                Assert.That(gamesRetrieved, Is.EqualTo(0));
            });
        }

        [Test]
        public void GetPagesSuccessTests()
        {
            MockHttpMessageHandler mockHttp = new();
            mockHttp.When("https://nxbrew.com/").Respond("*/*", GetEmbeddedHtml("nxbrewExample"));

            int pagecount;

            using (Scraper.Scraper s = new() { httpClient = mockHttp.ToHttpClient() })
            {
                Assert.DoesNotThrowAsync(async () => await s.GetPagecount());
                pagecount = s.Pagecount;
            }

            Assert.That(pagecount, Is.Not.Default);
            Assert.That(pagecount, Is.EqualTo(1292));
        }

        [Test]
        public void GetPagesWrongTests()
        {
            MockHttpMessageHandler mockWrongHttp = new();
            mockWrongHttp.When("https://nxbrew.com/").Respond("*/*", GetEmbeddedHtml("nxbrewExampleWrong"));

            int pagecount;

            using (Scraper.Scraper s = new() { httpClient = mockWrongHttp.ToHttpClient() })
            {
                Assert.DoesNotThrowAsync(async () => await s.GetPagecount());
                pagecount = s.Pagecount;
            }

            Assert.That(pagecount, Is.Default);
        }

        [Test]
        [TestCase(false, 0, 2 ,2)]
        [TestCase(true, 0, 2, 2)]
        [TestCase(true, 0, 0, 0)]
        [TestCase(true, 36, 2, 3)]
        [TestCase(false, 38, 2, 3)]
        public void GetFromRangePagesTests(bool filterUpdates, int resultCount, int from, int to)
        {
            MockHttpMessageHandler mockHttp = new();
            mockHttp.When("https://nxbrew.com/*").Respond("*/*", GetEmbeddedHtml("nxbrewExample"));

            List<SwitchGame> results = null;
            int gamesRetrieved = 0;

            using (Scraper.Scraper s = new() { httpClient = mockHttp.ToHttpClient(), FilterUpdates = filterUpdates })
            {
                s.GameRetrieved += (s, e) => gamesRetrieved++;
                Assert.DoesNotThrowAsync(async () => results = (await s.GetGamesFromPages(from, to)).ToList());
            }

            Assert.Multiple(() =>
            {
                Assert.That(results, Has.Count.EqualTo(resultCount));
                Assert.That(gamesRetrieved, Is.EqualTo(resultCount));
            });
            Assert.Multiple(() =>
            {
                Assert.That(results.TrueForAll(x => x.Date != default), Is.True);
                Assert.That(results.TrueForAll(x => x.NxDate != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Name != default), Is.True);
                Assert.That(results.TrueForAll(x => x.Categories.Length > 0), Is.True);
            });
        }

        [Test]
        public void ScrapeTimeoutTests()
        {
            MockHttpMessageHandler mockHttp = new();
            mockHttp.When("https://nxbrew.com/").Respond(async () => { await Task.Delay(1500); return null; });
            HttpClient timedoutMock = mockHttp.ToHttpClient();
            timedoutMock.Timeout = new System.TimeSpan(0, 0, 1);

            List<SwitchGame> results = null;
            bool isCanceled = false;

            using (Scraper.Scraper s = new() { httpClient = timedoutMock })
            {
                s.TaskCanceled += (s,e) => isCanceled = true;
                Assert.DoesNotThrowAsync(async () => results = (await s.GetGamesFromPage()).ToList());
            }

            Assert.Multiple(() =>
            {
                Assert.That(results, Is.Empty);
                Assert.That(isCanceled, Is.True);
            });
        }

        [TearDown]
        public void TearDown()
        {

        }
    }
}
