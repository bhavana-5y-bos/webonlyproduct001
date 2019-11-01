using BOS.Email.Client.ClientModels;
using System;
using System.Collections.Generic;

namespace BOS.StarterCode.Models.BOSModels
{
    public class Email : IEmail
    {
        public Guid Id { get; set; }

        public string Subject { get; set; }

        public string Text { get; set; }

        public string Html { get; set; }

        public List<To> To { get; set; }

        public List<Cc> Cc { get; set; }

        public List<Bcc> Bcc { get; set; }

        public List<Attachment> Attachments { get; set; }

        public From From { get; set; }

        public ReplyTo ReplyTo { get; set; }

        public List<Substitution> Substitutions { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public Guid TemplateId { get; set; }

        public Template Template { get; set; }

        public Guid ServiceProviderId { get; set; }

        public ServiceProvider ServiceProvider { get; set; }

        public bool Deleted { get; set; }

        public IDictionary<string, object> Extensions { get; set; }

        public List<EmailStatus> Response { get; set; }
    }
}
