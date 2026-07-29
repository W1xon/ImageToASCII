(function () {
  function getParticleCount() {
    const width = window.innerWidth;
    const isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
    const isMobileUA = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    const cores = navigator.hardwareConcurrency || 4;

    if (isTouch || isMobileUA) {
      return width < 480 ? 25 : 40;
    }
    if (cores <= 2) return 50;
    if (width >= 2560) return 120;
    if (width >= 1440) return 90;
    return 70;
  }

  function initAsciiBg() {
    const container = document.getElementById('asciiContainer');
    if (!container) return;

    container.innerHTML = '';

    const chars = [
      '.', ':', '-', '=', '+', '*', '#', '%', '@', '$',
      ',', ';', '~', '!', 'i', '>', '|', '\\', '/',
      '1', '(', ')', '[', ']', '{', '}', '?',
      'X', '0', 'O', 'Q', '8', '&', '·', '•', '≡',
      '░', '▒', '▓', '█', '`', '^', '"', "'"
    ];

    const colors = [
      'var(--accent)',
      '#4ecdc4',
      '#ffe66d',
      '#a8e6cf',
      '#ff8b94'
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
      const color = colors[Math.floor(Math.random() * colors.length)];
      const startX = (Math.random() * 120) - 10;
      const startY = (Math.random() * 120) - 10;
      const driftRange = 15 + depth * 55;
      const endX = startX + (Math.random() - 0.5) * driftRange;
      const endY = startY + (Math.random() - 0.5) * driftRange;
      const speedFactor = Math.pow(Math.random(), 2.2);
      const duration = (4 + speedFactor * 30) * 1000;
      const delay = Math.random() * -40000;
      const fontSize = Math.floor(10 + depth * 28);
      const minOpacity = 0.06 + depth * 0.06;
      const maxOpacity = 0.3 + depth * 0.5;
      const easing = easings[Math.floor(Math.random() * easings.length)];

      span.style.fontSize = `${fontSize}px`;
      span.style.color = color;
      span.style.opacity = minOpacity;

      if (depth > 0.5) {
        const glowRadius = Math.floor((depth - 0.5) * 18);
        span.style.textShadow = `0 0 ${glowRadius}px ${color}, 0 0 ${glowRadius * 2}px ${color}`;
      }

      if (depth < 0.25) {
        span.style.filter = `blur(${(0.25 - depth) * 1.2}px)`;
      }

      span.animate([
        {
          transform: `translate3d(${startX}vw, ${startY}vh, 0)`,
          opacity: minOpacity
        },
        { opacity: maxOpacity, offset: 0.5 },
        {
          transform: `translate3d(${endX}vw, ${endY}vh, 0)`,
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