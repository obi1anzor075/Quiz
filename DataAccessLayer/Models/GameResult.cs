using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public partial class GameResult
    {
        public int GameResultId { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CorrectAnswers { get; set; }
        public DateTime GameDate { get; set; }
    }
}
