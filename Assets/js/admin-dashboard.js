
$(document).ready(function () {

    //submenu open/close
    $(".avatar-logo a").click(function () {
        $(".submenu").toggleClass("submenu-visible");
    });
    // $(".submenu .popup-cross-btn button").click(function () {
    //     $(".submenu").removeClass("submenu-visible");
    // });
    $(document).click(function (event) {
        if (!$(event.target).closest(".submenu,.avatar-logo a").length) {
            $("body").find(".submenu").removeClass("submenu-visible");
        }
    });

    //admin header search bar
    $(".search-box-mobile-btn button").click(function () {
        $(".header-search-box").slideToggle("fast");
    });

    // admin handler accordion
    $(".admin-page-sidebar .handler").on("click", function (e) {
        e.preventDefault();
        var $this = $(this);
        if (!$this.hasClass("toggle-active")) {
            $(".admin-page-sidebar .listing").slideUp(400);
            $(".admin-page-sidebar .handler").removeClass("toggle-active");
            $(".admin-page-sidebar .handler").removeClass("active");
            $(".admin-page-sidebar .handler").parent().removeClass("active__row");
        }
        $this.toggleClass("toggle-active");
        $this.parent().toggleClass("active__row");
        $this.next().slideToggle();
    });

    //admin main tabs (--popup-tabs--)
    $('.popup-tab-title').click(function () {
        $(".popup-tabs-section").removeClass('tab-active');
        $(".popup-tabs-section[data-id='" + $(this).attr('data-id') + "']").addClass("tab-active");
        $(".popup-tab-title").removeClass('active-tab');
        $(this).parent().find(".popup-tab-title").addClass('active-tab');

        if ($(".popup-tabs-section[data-id='message-handler']").hasClass("tab-active")) {
            $(".popup-task-button-wrapper.task-button-wrapper").addClass("show-btn");
            $(".popup-task-button-wrapper.task-button-wrapper").removeClass("hide-btn");
        }
        else {
            $(".popup-task-button-wrapper.task-button-wrapper").removeClass("show-btn");
            $(".popup-task-button-wrapper.task-button-wrapper").addClass("hide-btn");
        }
    });

    // admin view mini task accordion
    $(".view-mini-task-main .mini-task-box-toggle").on("click", function (e) {
        e.preventDefault();
        var $this = $(this);
        if (!$this.hasClass("toggle-active")) {
            $(".view-mini-task-main .mini-task-box-details").slideUp(400);
            $(".view-mini-task-main .mini-task-box-toggle").removeClass("toggle-active");
            $(".view-mini-task-main .mini-task-box-toggle").removeClass("active");
            $(".view-mini-task-main .mini-task-box-toggle").parent().removeClass("active__row");
        }
        $this.toggleClass("toggle-active");
        $this.parent().toggleClass("active__row");
        $this.next().next().slideToggle();
    });


   //  admin view Recommendation accordion
        $(".popup-task-review-wrapper .task-review-btn").on("click", function(e) {
            e.preventDefault();
            var $that = $(this);
            if (!$that.hasClass("toggle-active")) {
                $(".popup-task-review-wrapper .task-review-box-details").slideUp(400);
                $(".popup-task-review-wrapper .task-review-btn").removeClass("toggle-active");
                $(".popup-task-review-wrapper .task-review-btn").removeClass("active");
                $(".popup-task-review-wrapper .task-review-btn").parent().removeClass("active__row");
            }
            $that.toggleClass("toggle-active");
            $that.parent().toggleClass("active__row");
            $that.next().next().slideToggle();
        });


    // admin task journal tabs
    $('.task-journal-tab-title').click(function () {
        $(".task-journal-tab-content").removeClass('tab-active');
        $(".task-journal-tab-content[data-id='" + $(this).attr('data-id') + "']").addClass("tab-active");
        $(".task-journal-tab-title").removeClass('active-tab');
        $(this).parent().find(".task-journal-tab-title").addClass('active-tab');
    });




    //popup open and close
    let dashboardBody = $(".body-dashboard-page");
    $(".add_new_task").click(function () {
        dashboardBody.addClass("new-task-modal-active");
    });
    $(".task-btn").click(function () {
        dashboardBody.addClass("task-modal-active");
    });
    $(".popup-cross-btn button").click(function () {
        dashboardBody.removeClass("task-modal-active");
        dashboardBody.removeClass("new-task-modal-active");
    });
    $(document).on("click", function (ev) {
        let target = $(ev.target);
        if ((target.hasClass('dashboard-popup-background'))) {
            dashboardBody.removeClass("task-modal-active");
            dashboardBody.removeClass("new-task-modal-active");
            dashboardBody.removeClass("feedback-popup-active");
            dashboardBody.removeClass("social-share-popup-active");
        }
    });

    //add new task quantity
    var buttonPlus = $(".qty-btn-plus");
    var buttonMinus = $(".qty-btn-minus");
    var incrementPlus = buttonPlus.click(function () {
        var $n = $(this).parent(".qty-container").find(".input-qty");
        $n.val(Number($n.val()) + 1);
    });
    var incrementMinus = buttonMinus.click(function () {
        var $n = $(this).parent(".qty-container").find(".input-qty");
        var amount = Number($n.val());
        if (amount > 0) {
            $n.val(amount - 1);
        }
    });


    $('.new-task-input-box select').customSelectBox().change(function () {
        // Do something with `$(this).val()` !!
    });

});


const track = document.querySelector('.new-task-content-slides-main');
const slides = document.querySelectorAll('.new-task-content-slide');
const dots = document.querySelectorAll('.slide-dot');
const buttonWrapper = document.querySelector('.new-task-draft-next-wrapper');
const submitButton = document.querySelector('.new-task-submit-wrapper');

let i;
// const reset = () => dots.forEach((dot) => dot.classList.remove('active-dot'));
const reset = () => {
    dots.forEach((dot) => dot.classList.remove('active-dot'));
    slides.forEach((slide) => slide.classList.remove('slide-active'));
};


function slideTo(n) {
    // track.style.transform = `translateX(-${n * slides[0].offsetWidth}px)`;
    reset();
    slides[n].classList.add('slide-active');
    dots[n].classList.add('active-dot');
}

function activateArrows(direction) {
    direction === 'right' ? i++ : i--;
    if (i < 0) i = 0;
    if (i > slides.length - 1) i = slides.length - 1;
    slideTo(i);
}

function activateDots(e) {
    i = e.target.dataset.index;
    slideTo(i);
}

function activate(e) {
    e.target.matches('.slide-dot') && activateDots(e);
    e.target.matches('.new-task-next-button') && activateArrows('right');
    e.target.matches('.new-task-back-button') && activateArrows();
    if (i == dots.length - 1) {
        buttonWrapper.classList.add("hide-btn-wrapper");
        submitButton.classList.add("show-btn-wrapper");
    }
    else {
        buttonWrapper.classList.remove("hide-btn-wrapper");
        submitButton.classList.remove("show-btn-wrapper");
    }
}

function init(n) {
    i = n;
    slideTo(n);
}

document.addEventListener('click', activate, false);
window.addEventListener('load', init(0), false);
