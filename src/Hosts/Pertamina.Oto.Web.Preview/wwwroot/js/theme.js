try { document.documentElement.dataset.theme = localStorage.getItem('oto.web.theme') || 'light'; } catch { document.documentElement.dataset.theme = 'light'; }
