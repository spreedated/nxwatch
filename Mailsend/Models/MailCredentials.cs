using Newtonsoft.Json;

namespace Mailsend.Models
{
    public record MailCredentials
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("ssl")]
        public bool SSL { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
