using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using RatherThis.Domain.Entities;

namespace RatherThis.Models
{
    public class NewQuestionViewModel
    {
        public string Gender { get; set; }
        [Required(ErrorMessage="Please enter the first answer option.")]
        [StringLength(140, ErrorMessage="First answer option exceeds 140 characters.")]
        public string Option1 { get; set; }
        [Required(ErrorMessage = "Please enter the second answer option.")]
        [StringLength(140, ErrorMessage = "Second answer option exceeds 140 characters.")]
        public string Option2 { get; set; }
    }

    public class QuestionDisplayViewModel
    {
        public DateTime Date { get; set; }
        public string OptionText1 { get; set; }
        public string OptionText2 { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public int OptionId1 { get; set; }
        public int OptionId2 { get; set; }
        public int QuestionId { get; set; }
        public bool IsLoggedIn { get; set; }
        public string QuestionUserGender { get; set; }
    }

    public class AnswerDisplayViewModel
    {
        public DateTime Date { get; set; }
        public string OptionText1 { get; set; }
        public string OptionText2 { get; set; }
        public int OptionVotes1 { get; set; }
        public int OptionVotes2 { get; set; }
        public int OptionId1 { get; set; }
        public int OptionId2 { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public int TotalVotes { get; set; }
        public int QuestionId { get; set; }
        public int AnsweredOptionId { get; set; }
        public string QuestionUserGender { get; set; }
        public CommentListViewModel CommentModel { get; set; }
    }

    public class CommentFormViewModel
    {
        [Required]
        [StringLength(500)]
        public string Comment { get; set; }
        public int QuestionId { get; set; }
        public int AnsweredOptionId { get; set; }
    }

    public class CommentListViewModel
    {
        public List<Comment> Comments { get; set; }
        public int OptionId1 { get; set; }
        public int OptionId2 { get; set; }
    }
}