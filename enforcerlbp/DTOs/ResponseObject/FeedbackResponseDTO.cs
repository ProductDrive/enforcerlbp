using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.ResponseObject
{
    public class FeedbackResponseDTO
    {
        public Guid ID { get; set; }
        public Guid PatientId { get; set; }
        public string FeedbackMessage { get; set; }
        public Guid ExercisePrescriptionId { get; set; }
        public int PainResponse { get; set; }
        public string OtherComplaint { get; set; }
        public List<FeedbackReplyDTO> FeedbackReplies { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }


    public class FeedbackReplyDTO
    {
        public Guid Id { get; set; }
        public Guid FeedbackId { get; set; }
        public string ExerciseName { get; set; }
        public int OrderId { get; set; }
        public string Message { get; set; }
        public string OwnersName { get; set; }
        public Guid OwnerId { get; set; }
    }
}
