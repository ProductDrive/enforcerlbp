using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class FeedbackReply
    {
        public Guid Id { get; set; }
        public int OrderId { get; set; }
        public Guid FeedbackId { get; set; }
        public string Feedback { get; set; }
        public string Message { get; set; }
        public Guid OwnerId { get; set; }

    }

   
}
