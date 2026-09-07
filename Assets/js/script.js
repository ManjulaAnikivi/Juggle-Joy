jQuery(document).ready(function ($) {
    // header menu
    // var menu = $('.menu-menu-1-container');
    var menu = $('.navigation');
    var hamburger = $('.bars');
    $(hamburger).click(function () {
        // $(menu).slideToggle("fast", function () {
        //     $(hamburger).toggleClass('active');
        // });

        $(menu).animate({
            width: "toggle"
        }, "fast");
        $(hamburger).toggleClass('active');
        $('.navigation-backdrop').toggle(10);
        $('body').toggleClass('navigation-active');
    });

    $('.navigation-backdrop').click(function() {
        $(menu).animate({
            width: "toggle"
        }, "fast");
        $(hamburger).toggleClass('active');
        $('.navigation-backdrop').toggle(10);
        $('body').toggleClass('navigation-active');
    })


    // active page
    $(".nav-item a").click(function (e) {
        var link = $(this);
        var item = link.parent("li");
        if (item.hasClass("color-changed")) {
            item.removeClass("color-changed").children("a").removeClass("color-changed");
        } else {
            item.addClass("color-changed").children("a").addClass("color-changed");
        }
    }).each(function () {
            var link = $(this);
            if (link.get(0).href === location.href) {
                link.addClass("color-changed").parents("li").addClass("color-changed");
                return false;
            }
        });


    // accordion
    $(".faq_title").on("click", function (e) {
        e.preventDefault();
        var $this = $(this);
        if (!$this.hasClass("accordion-active")) {
            $(".faq_description").slideUp(400);
            $(".faq_title").removeClass("accordion-active");
            $(".faq_title").removeClass("active");
            $(".faq_title").parent().removeClass("active__row");
        }
        $this.toggleClass("accordion-active");
        $this.parent().toggleClass("active__row");
        $this.next().slideToggle();
    });
    

    // faq page tabs
    $('ul#tabs').find('li:eq(0)').addClass('active');
    $('#content-tab ul').find('> li:eq(0)').nextAll().hide();
    $('#tabs li').on("click", function (event) {
        event.preventDefault();
        $('#tabs li').removeClass();
        $('#content-tab ul > li').hide();
        $(this).addClass('active');
        var tab_index = $('#tabs li').index(this);
        $('#content-tab ul > li:eq(' + tab_index + ')').show();
    });


    // home page video banner //gift subscription video banner
    var body = document.querySelector('body');
    if (body.classList.contains('background-banner-video')) {
        let videoElement = document.querySelector('.banner-main video');
        let homePageBanner = document.querySelector('.home-page-banner');
        let playPauseButtonWrapper = document.querySelector('.play-pause-btn');
        let playPauseButton = document.querySelector('.play-pause');
        let playPauseButtonPlay = document.querySelector('.play-pause .play');
        let playPauseButtonPause = document.querySelector('.play-pause .pause');

        if(window.innerWidth < 768) {
            playPauseButtonWrapper.style.display = "none";
            function hideVideoOnMobile() {
                hideVideo = videoElement.canPlayType("video/mp4/webm");
                if (hideVideo == "") {
                    videoElement.src = "empty";
                } else {
                    videoElement.src = "empty";
                }
                videoElement.load();
            }
            window.onresize = hideVideoOnMobile(); 
        }

        if (homePageBanner.classList.contains('home-page-banner')) {
        playPauseButton.addEventListener('click', () => {
            playPauseButton.classList.toggle('bg-video-playing');
            if (playPauseButton.classList.contains('bg-video-playing')) {
                videoElement.play();
                playPauseButtonPlay.style.display = "none";
                playPauseButtonPause.style.display = "inline-flex";
            }
            else {
                videoElement.pause();
                playPauseButtonPlay.style.display = "inline-flex";
                playPauseButtonPause.style.display = "none";
            }
        });

        videoElement.addEventListener('ended', () => {
            playPauseButton.classList.remove('bg-video-playing');
        });
        }
    }


    // hide/show password
    $(".show_hide_password .fa-eye").on("click", function (e) {
        $(".show_hide_password .fa-eye").hide();
        $(".show_hide_password .fa-eye-slash").show();
        $(".password_field input").attr("type", "text");
    })

    $(".show_hide_password .fa-eye-slash").on("click", function (e) {
        $(".show_hide_password .fa-eye-slash").hide();
        $(".show_hide_password .fa-eye").show();
        $(".password_field input").attr("type", "password");
    })


    var lastScroll = 0;
    var progressBarDetail = $('.blog-reading-progress-bar');
    $(window).scroll(function(){
        var currentScroll = $(this).scrollTop();
        if (currentScroll > 50) {
           $('.header-main').addClass('sticky-header');
           $('header').removeClass('header-back');
           $('.progress-sticky').addClass('progress-active');
        } else {
            $('.header-main').removeClass('sticky-header');
           $('header').addClass('header-back');
           $('.progress-sticky').removeClass('progress-active');
        }
        // if($(this).width() < 768 && currentScroll > 50){
        if(currentScroll > 50){
            if(currentScroll > lastScroll){
                $('.header-main').addClass('float-hide');
            }else{
                $('.header-main').removeClass('float-hide');
            }
            lastScroll = currentScroll <= 0 ? 0 : currentScroll;
        }
        if(progressBarDetail.length){
            detailScrollSet();
        }
    });
    if(progressBarDetail.length){
        detailScrollSet();
        $('.blog-detail-main .scroll-to-top').on('click', function(){
            $('body, html').animate(
                {scrollTop:0}, 500
            )
        });
    }
    function detailScrollSet(){
        var winScroll = document.body.scrollTop || document.documentElement.scrollTop;
        var height = ($('.blog-detail-main').outerHeight() - $('.blog-detail-main .detail-others').outerHeight() - $('.blog-title-wrapper').outerHeight()) - document.documentElement.clientHeight;
        var scrolled = Math.min(Math.floor((winScroll / height) * 100), 100);
        $('.blog-reading-progress-bar').css('width', scrolled+'%');
        let progress = Math.min(Math.floor((winScroll / height) * 283), 283);
        $('.blog-detail-main .progress-circle-bar').css('stroke-dashoffset', 283 - progress);

        if((283 - progress) < 10){
            $('.blog-detail-main .scroll-to-top').css('opacity', '1');
            $('.blog-detail-main circle.progress-background').css('fill', '#00C1AB');
        }else{
            $('.blog-detail-main .scroll-to-top').css('opacity', '0');
            $('.blog-detail-main circle.progress-background').removeAttr('style');
        }

    }
    if(typeof WOW != 'undefined'){
        new WOW().init();
    }

    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
    });

});






 // $(".faq-box p").slideUp("fast");
    // $(".faq-box h5").on("click", function (e) {

    //     if($(this).hasClass('active')){
    //         $(this).removeClass('active');
    //         $(this).next().slideUp('fast');
    //     }
    //     else{
    //         $(this).addClass('active');
    //         $(".faq-box p").slideUp("fast");
    //         $(this).next().slideDown('fast');
    //     }
    // });


    // $(".faq-box p").slideUp("fast");
    // $(".faq-box h5").on("click", function (e) {
    //     if ($(this).parent().hasClass('active-faq')) {
    //         $(this).parent().removeClass('active-faq');
    //         // $(this).removeClass('active');
    //         $(this).next().slideUp('fast');
    //     }
    //     else {
    //         // $(this).addClass('active');
    //         $(this).parent().addClass('active-faq');
    //         $(".faq-box p").slideUp("fast");
    //         $(this).next().slideDown('fast');
    //     }
    // });

    // let video = document.querySelector('.banner-main #bgvid');
    // let btn = document.querySelector('.play-pause').addEventListener('click', button_action);

    // function button_action() {
    //     if (video.paused) {
    //         video.play();
    //         btn.innerHTML = "Puase Video";
    //     } else {
    //         video.pause();
    //         btn.innerHTML = "Play Video"
    //     }
    // };



// let videoElement = document.querySelector('.banner-main video');
// let playPauseButton = document.getElementById('play-pause');

// playPauseButton.addEventListener('click', () => {
// 	playPauseButton.classList.toggle('playing');
// 	if (playPauseButton.classList.contains('playing')) {
// 		videoElement.play();
// 	}
// 	else {
// 		videoElement.pause();
// 	}
// });

// videoElement.addEventListener('ended', () => {
// 	playPauseButton.classList.remove('playing');
// });




