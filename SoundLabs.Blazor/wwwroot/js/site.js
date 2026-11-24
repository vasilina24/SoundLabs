// Плавное появление блоков при прокрутке и состояние шапки.
// Blazor рисует страницу динамически, поэтому за новыми элементами следит MutationObserver.
(function () {
  var root = document.documentElement;
  root.classList.add('js');

  var reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  var io = 'IntersectionObserver' in window
    ? new IntersectionObserver(function (entries) {
        entries.forEach(function (e) {
          if (e.isIntersecting) {
            e.target.classList.add('in');
            io.unobserve(e.target);
          }
        });
      }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' })
    : null;

  function scan() {
    document.querySelectorAll('.reveal:not([data-seen])').forEach(function (el) {
      el.setAttribute('data-seen', '1');
      if (io && !reduce) io.observe(el); else el.classList.add('in');
    });
  }

  new MutationObserver(scan).observe(root, { childList: true, subtree: true });
  scan();

  function onScroll() { document.body.classList.toggle('scrolled', window.scrollY > 16); }
  window.addEventListener('scroll', onScroll, { passive: true });
  onScroll();
})();

window.soundlabsDownload = (filename, contentType, base64) => {
    const a = document.createElement('a');
    a.href = `data:${contentType};base64,${base64}`;
    a.download = filename;
    document.body.appendChild(a); a.click(); a.remove();
};
