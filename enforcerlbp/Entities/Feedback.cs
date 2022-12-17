using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Feedback
    {
        public Guid ID { get; set; }
        public string FeedbackMessage { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public int PainResponse { get; set; }
        public string OtherComplaint { get; set; }
        public ICollection<FeedbackReply> FeedbackReplies { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

    }

}
