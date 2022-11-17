using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Infrastructure.EmailServices
{
    public class EmailSenderCommand : IRequest<bool>
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public List<Stream> AttachmentInCode { get; set; }
        public string Filename { get; set; }

        public bool Ispersonalized { get; set; }
        public List<ContactsModel> Contacts { get; set; }
        public string[] GroupedContacts { get; set; }
        public string ToContacts { get; set; }
        public string ToOthers { get; set; }
        public string ISOCode { get; set; }
        public string Category { get; set; }

        public string EmailAddress { get; set; }
        public string EmailDisplayName { get; set; }

    }

    public class ContactsModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string status { get; set; }
        public string otherInfo { get; set; }
    }
}
