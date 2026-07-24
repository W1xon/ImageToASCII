(function () {
    function getParticleCount() {
        const width = window.innerWidth;
        const isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
        const isMobileUA = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        const cores = navigator.hardwareConcurrency || 4;

        // 1. Мобилки и тач-устройства (экономя ресурсы/аккумулятор)
        if (isTouch || isMobileUA) {
            return width < 480 ? 40 : 65;
        }

        // 2. Слабые ПК / Ноуты (мало ядер)
        if (cores <= 2) {
            return 80;
        }

        if (width >= 2560) return 220; // 2K / 4K моники
        if (width >= 1440) return 170; // 1440p
        return 120;                    // Стандартный десктоп
    }

    function initAsciiBg() {
        const container = document.getElementById('asciiContainer');
        if (!container) return;
        container.innerHTML = '';

        const chars = [
            '.', ':', '-', '=', '+', '*', '#', '%', '@', '`', ',', ';', '~',
            'i', '>', 'r', 'c', 'v', 'u', 'n', 'z', 'X', '0', 'O', 'Q', 'm',
            'W', '&', '8', 'B', '·', '•', 'M', '!', '|',
            'H', 't', 'o', 'a', 'b', 'p', 'w', 'k', 'N', '≡', 'x', '?', 'S'
        ];

        const count = getParticleCount();

        const easings = [
            'cubic-bezier(0.25, 1, 0.5, 1)',
            'cubic-bezier(0.5, 0, 0.75, 0)',
            'cubic-bezier(0.12, 0.8, 0.32, 1)',
            'cubic-bezier(0.37, 0, 0.63, 1)'
        ];

        for (let i = 0; i < count; i++) {
            const span = document.createElement('span');
            span.className = 'ascii-particle';
            span.textContent = chars[Math.floor(Math.random() * chars.length)];

            const depth = Math.random();

            const startX = (Math.random() * 120) - 10;
            const startY = (Math.random() * 120) - 10;

            const driftRange = 15 + depth * 55;
            const endX = startX + (Math.random() - 0.5) * driftRange;
            const endY = startY + (Math.random() - 0.5) * driftRange;

            const speedFactor = Math.pow(Math.random(), 2.2);
            const duration = (3 + speedFactor * 35) * 1000;
            const delay = Math.random() * -45000;

            const fontSize = Math.floor(8 + depth * 32);
            const minOpacity = 0.04;
            const maxOpacity = 0.12 + depth * 0.65;

            const startZ = (depth - 0.5) * 330;
            const endZ = startZ + (Math.random() - 0.5) * 80;

            const easing = easings[Math.floor(Math.random() * easings.length)];

            span.style.fontSize = `${fontSize}px`;
            span.style.filter = depth > 0.8 ? `blur(${(depth - 0.8) * 4}px)` : 'none';

            span.animate([
                {
                    transform: `translate3d(${startX}vw, ${startY}vh, ${startZ}px)`,
                    opacity: minOpacity
                },
                { opacity: maxOpacity, offset: 0.5 },
                {
                    transform: `translate3d(${endX}vw, ${endY}vh, ${endZ}px)`,
                    opacity: minOpacity
                }
            ], {
                duration: duration,
                delay: delay,
                iterations: Infinity,
                easing: easing
            });

            container.appendChild(span);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAsciiBg);
    } else {
        initAsciiBg();
    }
})();