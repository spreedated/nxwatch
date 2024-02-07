using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mailsend;
using static UnitTests.HelperFunctions.HelperFunctions;
using System.IO;
using MailKit.Net.Smtp;
using MimeKit;
using Moq;
using System.Net.Mail;
using HtmlAgilityPack;
using System.Threading;
using MailKit;

namespace MailsendLayer
{
    internal class SendMailTests
    {
        private string testfilepath = null;
        private string testfilepath2 = null;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            this.testfilepath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Testfiles", "exampleemail.json"));
            this.testfilepath2 = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Testfiles", "exampleemail2.json"));
        }

        [Test]
        public void GetTemplatesTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SendMail.GetEmbeddedHtml("something"), Is.Null);
                Assert.That(SendMail.GetEmbeddedHtml(null), Is.Null);
                Assert.That(SendMail.GetEmbeddedHtml("ETemplate1"), Is.Not.Null);
                Assert.That(SendMail.GetEmbeddedHtml("etemplate1"), Is.Not.Null);
            });
        }

        [Test]
        public void MailAddTests()
        {
            SendMail s = new(this.testfilepath, "test@none.com");

            Assert.Multiple(() =>
            {
                Assert.That(s.switchGames, Is.Empty);
                s.Add(new());
                Assert.That(s.switchGames, Has.Count.EqualTo(1));
                s.Add(new());
                Assert.That(s.switchGames, Has.Count.EqualTo(2));
                s.Add(new());
                Assert.That(s.switchGames, Has.Count.EqualTo(3));
            });

            s.AddRange([ new(), new(), new(), new() ]);
            Assert.That(s.switchGames, Has.Count.EqualTo(7));

            Assert.That(s.receiverEmailAddress, Is.EqualTo("test@none.com"));
        }

        [Test]
        public void MailCredentialsTests()
        {
            SendMail s = new(this.testfilepath, null);

            Assert.That(s.EmailJSONFile, Is.EqualTo(testfilepath));

            Assert.Multiple(() =>
            {
                Assert.That(s.mailCredentials, Is.Not.Null);
                Assert.That(s.mailCredentials.Address, Is.Null);
                Assert.That(s.mailCredentials.SSL, Is.False);
                Assert.That(s.mailCredentials.Username, Is.Null);
                Assert.That(s.mailCredentials.Host, Is.Null);
                Assert.That(s.mailCredentials.Password, Is.Null);
                Assert.That(s.mailCredentials.Port, Is.EqualTo(0));
            });

            s = new(this.testfilepath2, null);

            Assert.Multiple(() =>
            {
                Assert.That(s.mailCredentials, Is.Not.Null);
                Assert.That(s.mailCredentials.Address, Is.EqualTo("foobar_p"));
                Assert.That(s.mailCredentials.SSL, Is.True);
                Assert.That(s.mailCredentials.Username, Is.EqualTo("foobar"));
                Assert.That(s.mailCredentials.Host, Is.EqualTo("smtp.gmail.com"));
                Assert.That(s.mailCredentials.Password, Is.EqualTo("pass"));
                Assert.That(s.mailCredentials.Port, Is.EqualTo(587));
            });
        }

        [Test]
        public void RenderHtmlBodyTests()
        {
            SendMail s = new(this.testfilepath, null);

            s.switchGames.Enqueue(new()
            {
                Categories = [ "Action", "Adventure" ],
                Date = DateTime.Now,
                Link = "https://www.nintendo.com/games/detail/animal-crossing-new-horizons-switch/",
                Name = "Animal Crossing: New Horizons",
                NxDate = DateTime.Now
            });

            s.switchGames.Enqueue(new()
            {
                Categories = ["Adventure"],
                Date = DateTime.Now.AddDays(-2),
                Link = "https://store.nintendo.de/de/mario-vs-donkey-kong-70010000072192",
                Name = "Mario vs. Donkey Kong",
                NxDate = DateTime.Now.AddDays(-2)
            });

            string res = s.RenderMailbodyFromTemplate();

            Assert.Multiple(() =>
            {
                Assert.That(res, Is.Not.Null);
                Assert.That(res, Is.Not.Empty);
                Assert.That(res, Does.Contain("Animal Crossing: New Horizons"));
                Assert.That(res, Does.Contain("Mario vs. Donkey Kong"));
                Assert.That(res, Does.Contain("https://www.nintendo.com/games/detail/animal-crossing-new-horizons-switch/"));
                Assert.That(res, Does.Contain("https://store.nintendo.de/de/mario-vs-donkey-kong-70010000072192"));
                Assert.That(res, Does.Contain(DateTime.Now.ToString("f")));
                Assert.That(res, Does.Contain(DateTime.Now.AddDays(-2).ToString("f")));
                Assert.That(res, Does.Not.Contain("###"));
            });
        }

        [Test]
        public void BuildMimeMethodTests()
        {
            SendMail s = new(this.testfilepath2, "some@one.com");
            MimeMessage mm = s.BuildMimeMessage();

            Assert.That(mm, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(mm.From.ToString(), Is.EqualTo("\"foobar_p\" <foobar_p>"));
                Assert.That(mm.To.ToString(), Is.EqualTo("\"some@one.com\" <some@one.com>"));
                Assert.That(mm.Subject.ToLower().Contains("game"), Is.True);
                Assert.That(mm.HtmlBody, Is.Not.Null);
            });



            Assert.DoesNotThrow(mm.Dispose);
        }
    }
}
