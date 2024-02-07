using MailKit.Net.Smtp;
using Mailsend.Models;
using MimeKit;
using Newtonsoft.Json;
using Scraper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mailsend
{
    public class SendMail
    {
        internal MailCredentials mailCredentials = null;
        internal readonly Queue<SwitchGame> switchGames = null;
        internal string receiverEmailAddress = null;
        internal ISmtpClient smtpClient = null;
        public string EmailJSONFile { get; init; } = null;

        #region Constructor
        public SendMail(string filepath, string receiverEmailAddress)
        {
            this.EmailJSONFile = filepath;
            this.receiverEmailAddress = receiverEmailAddress;
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
            
            using (this.smtpClient ??= new SmtpClient())
            {
                smtpClient.Connect(this.mailCredentials.Host, this.mailCredentials.Port, this.mailCredentials.SSL);
                smtpClient.Authenticate(this.mailCredentials.Username, this.mailCredentials.Password);

                MimeMessage msg = this.BuildMimeMessage();

                await smtpClient.SendAsync(msg);

                msg.Dispose();
                smtpClient.Disconnect(true);
            }
        }

        internal MimeMessage BuildMimeMessage()
        {
            MimeMessage m = new();

            m.From.Add(new MailboxAddress(this.mailCredentials.Address, this.mailCredentials.Address));
            m.To.Add(new MailboxAddress(this.receiverEmailAddress, this.receiverEmailAddress));
            m.Subject = $"[{Assembly.GetExecutingAssembly().GetName().Name}] New game{(this.switchGames.Count == 1 ? "" : "s")}";
            m.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = this.RenderMailbodyFromTemplate()
            };

            return m;
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

        private void LoadMailCredentials()
        {
            using (FileStream fs = File.Open(this.EmailJSONFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader r = new(fs))
                {
                    this.mailCredentials = JsonConvert.DeserializeObject<MailCredentials>(r.ReadToEnd());
                }
            }
        }

        public static string GetEmbeddedHtml(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            if (!resourceName.EndsWith(".htm"))
            {
                resourceName += ".htm";
            }

            Assembly a = typeof(SendMail).Assembly;
            string resPath = a.GetManifestResourceNames().FirstOrDefault(x => x.Contains(resourceName, StringComparison.InvariantCultureIgnoreCase));

            if (string.IsNullOrEmpty(resPath))
            {
                return null;
            }

            using (Stream fileStream = a.GetManifestResourceStream(resPath))
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
