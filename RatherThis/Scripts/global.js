﻿$(function () {
    initOptionValidation();
    initSortLinks();
    initOptionClick();
    initReasonTextarea();
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

function loadWhileComment(commentContainerId) {
    var container = $("#" + commentContainerId);
    if (container) {
        $(".submit-comment", container).not(":disabled").hide();
        $("input:disabled", container).show();
    }
}

function toggleUnansweredQuestions(sectionUrl, checkbox) {
    //if its true, then it means user wants to see only unanswered questions
    //else it means user wants to see all questions
    if ($(checkbox).is(':checked')) {
        window.location = sectionUrl + "&onlyunanswered=true";
    } else {
        window.location = sectionUrl + "&onlyunanswered=false";
    }
}

function bumpUp(qid, triggerElt) {
    var isDumped = false;

    var netBumpSpan = $(triggerElt).siblings(".bump-count");
    var netBump = parseInt($(netBumpSpan).html());

    //check to see whether it's been bumped up already, if so, then clicking again will 'unbump' it
    //otherwise, mark it as 'on'
    if ($(triggerElt).hasClass("on")) {
        $(triggerElt).removeClass("on");
        netBump -= 1;
        $(netBumpSpan).html(netBump);

        $.get('/question/bump?qid=' + qid + '&direction=reset', function (data) {

        });
    } else {
        isDumped = $(triggerElt).siblings(".on").length > 0;
        $(triggerElt).siblings(".on").removeClass("on");
        $(triggerElt).addClass("on");

        //this means it has been dumped before, so need to increment the total count by 2
        if (isDumped) {
            netBump += 2;
            $(netBumpSpan).html(netBump);
        } else {
            netBump += 1;
            $(netBumpSpan).html(netBump);
        }

        $.get('/question/bump?qid=' + qid + '&direction=up', function (data) {

        });
    }
}

function bumpDown(qid, triggerElt) {
    var isBumped = false;
    var netBumpSpan = $(triggerElt).siblings(".bump-count");
    var netBump = parseInt($(netBumpSpan).html());

    //check to see whether it's been bumped down already, if so, then clicking again will 'undump' it
    //otherwise, mark it as 'on'
    if ($(triggerElt).hasClass("on")) {
        $(triggerElt).removeClass("on");
        netBump += 1;
        $(netBumpSpan).html(netBump);

        $.get('/question/bump?qid=' + qid + '&direction=reset', function (data) {

        });
    } else {
        isBumped = $(triggerElt).siblings(".on").length > 0;
        $(triggerElt).siblings(".on").removeClass("on");
        $(triggerElt).addClass("on");

        //this means it has been dumped before, so need to increment the total count by 2
        if (isBumped) {
            netBump -= 2;
            $(netBumpSpan).html(netBump);
        } else {
            netBump -= 1;
            $(netBumpSpan).html(netBump);
        }

        $.get('/question/bump?qid=' + qid + '&direction=down', function (data) {

        });
    }
}

function initSortLinks() {
    $(".sort-links .current-sort, .sort-links a").bind('mouseover', function (e) {
        $(".sort-links a").show();
    });

    $(" .sort-links .current-sort, .sort-links a").bind('mouseout', function (e) {
        $(".sort-links a").hide();
    });
}

function initOptionClick() {
    $(".option-container").bind('click', function (e) {
        if ($("body").hasClass("ie7")) {
            $(this).siblings(".option-reason").show();
        } else {
            $(this).siblings(".option-reason").slideDown('fast');
        }
    });
}

function initReasonTextarea() {
    //limit char limit to 320
    $(".option-reason-area").bind('keyup', function (e) {
        var charCount = $(this).val().length;
        if (charCount > 320) {
            $(this).val($(this).val().substring(0, 319));
        }
        
        //update char count
        var counter = $(this).siblings(".counter");
        $(".count", counter).html(charCount);
    }).bind('change', function (e) {
        var charCount = $(this).val().length;
        if (charCount > 320) {
            $(this).val($(this).val().substring(0, 319));
        }

        //update char count
        var counter = $(this).siblings(".counter");
        $(".count", counter).html(charCount);
    }).bind('keydown', function (e) {
        var charCount = $(this).val().length;
        if (charCount > 320) {
            $(this).val($(this).val().substring(0, 319));
        }

        //update char count
        var counter = $(this).siblings(".counter");
        $(".count", counter).html(charCount);
    });
}

//this function makes sure when the user selects an answer, then we hide the validation summary for the question
function initOptionValidation() {
    $(".option-container input[type='radio']").bind('click', function (e) {
        $(this).parent().siblings(".validation-summary-errors").hide();
    });
}