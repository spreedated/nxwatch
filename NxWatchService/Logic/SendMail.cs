using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using NxBrewWindowsServiceReporter.Models;
using Scraper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NxBrewWindowsServiceReporter.Logic
{
    public class SendMail
    {
        private readonly string emailJSONFile = Path.Combine(Environment.CurrentDirectory, "email.json");
        private MailCredentials mailCredentials = null;
        private readonly Queue<SwitchGame> switchGames = null;

        #region Constructor
        public SendMail()
        {
            this.LoadMailCredentials();
            this.switchGames = new();
        }
        #endregion

        public void Add(SwitchGame s)
        {
            this.switchGames.Enqueue(s);
        }

        public void AddRange(IEnumerable<SwitchGame> s)
        {
            foreach (SwitchGame item in s)
            {
                this.switchGames.Enqueue(item);
            }
        }

        public async Task Send()
        {
            if (this.switchGames.Count <= 0)
            {
                return;
            }

            using (SmtpClient smtpClient = new())
            {
                smtpClient.Connect(this.mailCredentials.Host, this.mailCredentials.Port, this.mailCredentials.SSL);
                smtpClient.Authenticate(this.mailCredentials.Username, this.mailCredentials.Password);

                using (MimeMessage m = new())
                {
                    m.From.Add(new MailboxAddress(this.mailCredentials.Address, this.mailCredentials.Address));
                    m.To.Add(new MailboxAddress("markus.wackermann@gmail.com", "markus.wackermann@gmail.com"));
                    m.Subject = $"[{Assembly.GetExecutingAssembly().GetName().Name}] New game{(this.switchGames.Count == 1 ? "" : "s")}";
                    m.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                    {
                        Text = this.RenderMailbodyFromTemplate()
                    };

                    await smtpClient.SendAsync(m);
                }

                smtpClient.Disconnect(true);
            }
        }

        internal string RenderMailbodyFromTemplate()
        {
            string v = GetEmbeddedHtml("ETemplate1");
            StringBuilder s = new();

            while (this.switchGames.Count > 0)
            {
                SwitchGame sw = this.switchGames.Dequeue();
                s.Append(GetEmbeddedHtml("ETemplate1_Content").Replace("###NAME###", sw.Name).Replace("###LINK###", sw.Link).Replace("###PUBLISHEDON###", sw.NxDate.ToString("f")));
            }

            v = v.Replace("###CONTENT###", s.ToString());

            return v;
        }

        public bool DoesEmailJSONExist()
        {
            return File.Exists(this.emailJSONFile);
        }

        public void CreateEmailJSON()
        {
            File.Create(this.emailJSONFile).Close();

            using (FileStream fs = File.Open(this.emailJSONFile, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamWriter w = new(fs))
                {
                    w.Write(JsonConvert.SerializeObject(new MailCredentials(), Formatting.Indented));
                }
            }
        }

        private void LoadMailCredentials()
        {
            using (FileStream fs = File.Open(this.emailJSONFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader r = new(fs))
                {
                    this.mailCredentials = JsonConvert.DeserializeObject<MailCredentials>(r.ReadToEnd());
                }
            }
        }

        public static string GetEmbeddedHtml(string resourceName)
        {
            if (!resourceName.EndsWith(".htm"))
            {
                resourceName += ".htm";
            }

            using (Stream fileStream = typeof(SendMail).Assembly.GetManifestResourceStream($"NxBrewWindowsServiceReporter.EmailTemplates.{resourceName}"))
            {
                if (fileStream == null)
                {
                    return null;
                }

                using (StreamReader sr = new(fileStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
