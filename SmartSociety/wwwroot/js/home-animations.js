document.addEventListener("DOMContentLoaded", function () {

    gsap.registerPlugin(ScrollTrigger);

    /* =========================================
       HERO ANIMATION
    ========================================= */

    const hero = document.querySelector(".hero-card");

    if (hero) {

        const heroTimeline = gsap.timeline({
            defaults: {
                ease: "power3.out"
            }
        });

        heroTimeline
            .from(".hero-card", {
                opacity: 0,
                y: 60,
                duration: 1
            })
            .from(".hero-card .badge", {
                opacity: 0,
                y: 20,
                duration: .5
            }, "-=.5")
            .from(".hero-card h1", {
                opacity: 0,
                y: 40,
                duration: .7
            }, "-=.3")
            .from(".hero-card .lead", {
                opacity: 0,
                y: 25,
                duration: .6
            }, "-=.4")
            .from(".hero-card .btn", {
                opacity: 0,
                y: 20,
                stagger: .15,
                duration: .5
            }, "-=.3")
            .from(".hero-card [data-counter]", {
                opacity: 0,
                y: 20,
                stagger: .15,
                duration: .5
            }, "-=.3");
    }


    /* =========================================
       FEATURE CARDS
    ========================================= */

    gsap.utils.toArray(".feature-card").forEach((card, index) => {

        gsap.from(card, {
            scrollTrigger: {
                trigger: card,
                start: "top 85%",
                toggleActions: "play none none reverse"
            },

            opacity: 0,
            y: 70,
            scale: .94,
            duration: .8,
            delay: index * 0.08,
            ease: "power3.out"
        });

    });


    /* =========================================
       SECTION LABELS
    ========================================= */

    gsap.utils.toArray(".section-label").forEach(label => {

        gsap.from(label, {

            scrollTrigger: {
                trigger: label,
                start: "top 88%"
            },

            opacity: 0,
            x: -40,
            duration: .7,
            ease: "power3.out"
        });

    });


    /* =========================================
       FLOATING HERO CARDS
    ========================================= */

    gsap.utils.toArray(".float-el").forEach((element, index) => {

        gsap.from(element, {
            opacity: 0,
            scale: .7,
            y: 50,
            duration: 1,
            delay: .5 + index * .2,
            ease: "back.out(1.7)"
        });

        gsap.to(element, {
            y: index % 2 === 0 ? -15 : 15,
            duration: 2.5 + index,
            repeat: -1,
            yoyo: true,
            ease: "sine.inOut"
        });

    });


    /* =========================================
       SVG SKYLINE
    ========================================= */

    const skyline = document.querySelector(".hero-card svg");

    if (skyline) {

        gsap.from(skyline, {
            opacity: 0,
            scale: .8,
            duration: 1.5,
            delay: .3,
            ease: "power3.out"
        });

        gsap.to(skyline, {
            y: -12,
            duration: 4,
            repeat: -1,
            yoyo: true,
            ease: "sine.inOut"
        });

    }


    /* =========================================
       CTA ANIMATION
    ========================================= */

    const cta = document.querySelector(".hero-card.text-center");

    if (cta) {

        gsap.from(cta, {

            scrollTrigger: {
                trigger: cta,
                start: "top 80%"
            },

            opacity: 0,
            y: 80,
            scale: .95,
            duration: 1,
            ease: "power3.out"
        });

    }


    /* =========================================
       COUNTERS
    ========================================= */

    gsap.utils.toArray("[data-counter]").forEach(counter => {

        const target = Number(counter.dataset.counter);

        gsap.fromTo(
            counter,
            {
                innerText: 0
            },
            {
                innerText: target,
                duration: 1.5,
                ease: "power2.out",
                snap: {
                    innerText: 1
                },

                scrollTrigger: {
                    trigger: counter,
                    start: "top 90%",
                    once: true
                },

                onUpdate: function () {
                    counter.innerText =
                        Math.round(this.targets()[0].innerText);
                }
            }
        );

    });


    /* =========================================
       REDUCED MOTION
    ========================================= */

    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {

        ScrollTrigger.getAll().forEach(trigger => {
            trigger.kill();
        });

        gsap.set(
            ".feature-card, .section-label, .hero-card",
            {
                clearProps: "all"
            }
        );
    }

});