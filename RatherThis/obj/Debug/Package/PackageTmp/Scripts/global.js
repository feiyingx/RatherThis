$(function () {
    initOptionValidation()
});

(function ($) {
    $.validator.unobtrusive.adapters.addBool("mandatory", "required");
} (jQuery));

function initInField() {
    $("label.infield").inFieldLabels();
}

function loadComments(containerId, loaderId, questionId, listSize) {
    $.get("/Question/CommentList?qid=" + questionId + "&listSize=" + listSize, function (result) {
        var container = $("#" + containerId);
        if (container) $(container).html(result);
    });
}

function toggleRegistration() {
    var registration = $("#registration");
    if (registration) $(registration).slideToggle();
}

function toggleLogin() {
    var login = $("#login");
    if (login) $(login).slideToggle();
}

function toggleNewQuestion() {
    var newQuestion = $("#new-question");
    if (newQuestion) $(newQuestion).slideToggle();
}

function activateRegistration() {
    var registration = $("#registration");
    if(registration){
        $(registration).slideDown().effect("highlight", { color: "#ffe400" }, 1000);
    }
}

function activateLogin() {
    var login = $("#login");
    if (login) {
        $(login).slideDown().effect("highlight", { color: "#ffe400"}, 1000);
    }
}

function toggleComments(trigger) {
    $(trigger).siblings(".comment-container").slideToggle();
}

function toggleUserInfo() {
    var userInfo = $("#user-info");
    if (userInfo) {
        $(userInfo).slideToggle();
    }
}

function reinitAddThis() {
    if (window.addthis) {

        window.addthis.ost = 0;
        window.addthis.ready();
    }
}

var testtest;
function validateAnswer(questionContainerId) {
    var container = $("#" + questionContainerId);
    if (container){
        if ($("input[type='radio']:checked", container).length == 1) {
            return true;
        } else {
            if (event.preventDefault) {
                event.preventDefault();
            } else {
                event.returnValue = false;
            }
            $(".validation-summary-errors", container).show();
            return false;
        }
    }

    if (event.preventDefault) {
        event.preventDefault();
    } else {
        event.returnValue = false;
    }
    return false;
}

function loadWhileAnswer(actionButtonToLoad, actionButtonToHide, inactiveButtonToDisable, inactiveButtonToHide) {
    $("#" + actionButtonToHide).hide();
    $("#" + inactiveButtonToHide).hide();

    $("#" + actionButtonToLoad).show();
    $("#" + inactiveButtonToDisable).show();
}

function loadWhileNewQuestion() {
    $("#new-question-submit").hide();
    $("#new-question-submit-loading").show();
}
//this function makes sure when the user selects an answer, then we hide the validation summary for the question
function initOptionValidation() {
    $(".option-container input[type='radio']").bind('click', function (e) {
        $(this).parent().siblings(".validation-summary-errors").hide();
    });
}