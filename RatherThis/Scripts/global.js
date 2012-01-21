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

function toggleNewQuestion() {
    var newQuestion = $("#new-question");
    if (newQuestion) $(newQuestion).slideToggle();
}

function activateRegistration() {
    var registration = $("#registration");
    if(registration){
        $(registration).slideDown().effect("highlight", {}, 2000);
    }
}

function activateLogin() {
    var login = $("#login");
    if (login) {
        $(login).effect("highlight", {}, 2000);
    }
}