function loadComments(containerId, loaderId, questionId, listSize) {
    $.get("/Question/CommentList?qid=" + questionId + "&listSize=" + listSize, function (result) {
        var container = $("#" + containerId);
        if (container) $(container).html(result);
        var loader = $("#" + loaderId);
        if (loader) $(loader).hide();
    });
}