using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Feedback
    {
        public Guid ID { get; set; }
        public Guid PatientId { get; set; }
        public Guid ExercisePrescriptionId { get; set; }
        public string FeedbackMessage { get; set; }
        public int PainResponse { get; set; }
        public string OtherComplaint { get; set; }
        //public Guid FeedbackReplyID { get; set; }
        public ICollection<FeedbackReply> FeedbackReplies { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

    }

}
