using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using RatherThis.Domain.Entities;

namespace RatherThis.Models
{
    public class QuestionIndexViewModel
    {
        public Dictionary<object, string> ResultViewModels { get; set; }
        public int CurrentPage { get; set; }
        public string Gender { get; set; }
        public string Sort { get; set; }
        public int TotalPages { get; set; }
        public int CurrentCategoryId { get; set; }
        public bool IsOnlyUnanswered { get; set; }
    }

    public class NewQuestionViewModel
    {
        public string Gender { get; set; }
        [Required(ErrorMessage="Please enter the first answer option.")]
        [StringLength(140, ErrorMessage="First answer option exceeds 140 characters.")]
        public string Option1 { get; set; }
        [Required(ErrorMessage = "Please enter the second answer option.")]
        [StringLength(140, ErrorMessage = "Second answer option exceeds 140 characters.")]
        public string Option2 { get; set; }
        [Required(ErrorMessage = "Please select a category.")]
        public int Category { get; set; }
    }

    public class EditQuestionViewModel
    {
        public string Gender { get; set; }
        [Required(ErrorMessage = "Please enter the first answer option.")]
        [StringLength(140, ErrorMessage = "First answer option exceeds 140 characters.")]
        public string Option1 { get; set; }
        [Required(ErrorMessage = "Please enter the second answer option.")]
        [StringLength(140, ErrorMessage = "Second answer option exceeds 140 characters.")]
        public string Option2 { get; set; }
        [Required(ErrorMessage = "Please select a category.")]
        public int Category { get; set; }

        [Required(ErrorMessage = "Please enter a question id.")]
        public int QuestionID { get; set; }
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
        public string QuestionUsername { get; set; }
        public string QuestionCategory { get; set; }
        public int QuestionCategoryId { get; set; }
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
        public string QuestionUsername { get; set; }
        public string QuestionCategory { get; set; }
        public int QuestionCategoryId { get; set; }
        public CommentListViewModel CommentModel { get; set; }
        public int NumComments { get; set; }

        private int _commentListSize = 9; //default size
        public int CommentListSize
        {
            get { return _commentListSize; }
            set { _commentListSize = value; }
        }

        private bool _showAllComment = false;
        public bool IsShowAllComment
        {
            get { return _showAllComment; }
            set { _showAllComment = value; }
        }
    }

    public class QuestionDetailViewModel
    {
        public Question Question { get; set; }
        public String OptionText1 { get; set; }
        public String OptionText2 { get; set; }
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