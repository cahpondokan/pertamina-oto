(() => {
  const picker = document.querySelector('#theme');
  if (picker) {
    const media = matchMedia('(prefers-color-scheme: dark)');
    let choice = 'system';
    try { const stored = localStorage.getItem('oto.theme'); if (['system', 'light', 'dark'].includes(stored)) choice = stored; } catch {}
    const apply = () => { document.documentElement.dataset.theme = choice === 'system' ? (media.matches ? 'dark' : 'light') : choice; };
    picker.value = choice; apply(); picker.closest('.theme-control').hidden = false;
    picker.addEventListener('change', () => { choice = picker.value; apply(); try { localStorage.setItem('oto.theme', choice); } catch {} });
    media.addEventListener('change', apply);
  }
  const toast = document.querySelector('.download-toast');
  if (!toast) return;
  let timer;
  const dismiss = () => { toast.hidden = true; clearTimeout(timer); };
  toast.querySelector('button').addEventListener('click', dismiss);
  document.querySelectorAll('a[data-download]').forEach(link => link.addEventListener('click', event => {
    if (event.defaultPrevented || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    toast.hidden = false;
    toast.querySelector('[role=status]').textContent = 'Permintaan unduh dikirim. Periksa unduhan browser.';
    clearTimeout(timer); timer = setTimeout(dismiss, 10000);
  }));
})();
