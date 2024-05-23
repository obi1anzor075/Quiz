using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public partial class HardQuestion
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string ImageUrl { get; set; }
        public string CorrectAnswer { get; set; }
        public string CorrectAnswer2 { get; set; }
    }
}
