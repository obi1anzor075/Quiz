using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string CorrectAnswer { get; set; } = null!;

    public string? Answer1 { get; set; }

    public string? Answer2 { get; set; }

    public string? Answer3 { get; set; }

    public string? Answer4 { get; set; }
}
