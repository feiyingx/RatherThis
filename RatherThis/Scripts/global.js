$(function () {
    
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
        var loader = $("#" + loaderId);
        if (loader) $(loader).hide();
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