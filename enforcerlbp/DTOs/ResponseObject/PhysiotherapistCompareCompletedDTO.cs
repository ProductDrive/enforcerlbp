
using System;
using System.Collections.Generic;
using System.Text;

namespace DTOs.ResponseObject
{
    public class PhysiotherapistCompareCompletedDTO
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Addressline { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string DOB { get; set; }
        public int Age { get; set; }
        public string AgeCheck  => Age == 0 ? "" : Age.ToString();
        public int Experience { get; set; }
        public string ExpCheck => Experience == 0 ? "" : Experience.ToString();
        public double Ratings { get; set; }
        public string RatingsCheck => Ratings < 1  ? "" : Ratings.ToString();
        public string RatingData { get; set; }
        public string About { get; set; }
        public string Speciality { get; set; }
        public string Gender { get; set; }
    }
}
