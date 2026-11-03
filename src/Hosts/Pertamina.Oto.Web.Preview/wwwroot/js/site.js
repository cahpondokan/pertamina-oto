(() => {
  'use strict';
  const clone = value => JSON.parse(JSON.stringify(value));
  const empty = () => ({ sources: [], items: [], nextSource: 0 });
  const sourceId = index => index < 26 ? String.fromCharCode(65 + index) : 'A' + String.fromCharCode(65 + index - 26);
  const demo = () => ({
    nextSource: 3,
    sources: [
      { id: 'A', name: 'Laporan operasional.pdf', size: 245760, pages: 2, example: true, color: 0 },
      { id: 'B', name: 'Checklist inspeksi.pdf', size: 184320, pages: 2, example: true, color: 1 },
      { id: 'C', name: 'Berita acara.pdf', size: 98304, pages: 1, example: true, color: 2 }
    ],
    items: ['A1', 'A2', 'B1', 'B2', 'C1'].map(id => ({ id, source: id[0], originalPage: Number(id[1]), rotation: 0 }))
  });
  function validate(value) {
    if (!value || !Array.isArray(value.sources) || !Array.isArray(value.items) || value.sources.length > 50 || value.items.length > 100) return empty();
    const sources = value.sources.filter(s => s && /^[A-Z]{1,2}$/.test(s.id) && typeof s.name === 'string' && s.name.length <= 200 && Number.isFinite(s.size) && s.size >= 0)
      .map(s => ({ id: s.id, name: s.name, size: s.size, pages: s.example === true && Number.isInteger(s.pages) && s.pages > 0 && s.pages <= 100 ? s.pages : null, example: s.example === true, color: Number.isInteger(s.color) && s.color >= 0 && s.color < 5 ? s.color : 0 }));
    if (sources.length !== value.sources.length || new Set(sources.map(s => s.id)).size !== sources.length) return empty();
    const items = value.items.filter(i => i && typeof i.id === 'string' && /^[A-Za-z0-9-]{1,80}$/.test(i.id) && sources.some(s => s.id === i.source))
      .map(i => ({ id: i.id, source: i.source, originalPage: sources.find(s => s.id === i.source).example && Number.isInteger(i.originalPage) && i.originalPage > 0 ? i.originalPage : null, rotation: [0, 90, 180, 270].includes(i.rotation) ? i.rotation : 0 }));
    if (items.length !== value.items.length || new Set(items.map(i => i.id)).size !== items.length || sources.some(s => !items.some(i => i.source === s.id))) return empty();
    const minimumNext = sources.reduce((max, s) => Math.max(max, s.id.length === 1 ? s.id.charCodeAt(0) - 64 : s.id.charCodeAt(1) - 64 + 26), 0);
    return { sources, items, nextSource: Math.min(50, Math.max(minimumNext, Number.isInteger(value.nextSource) ? value.nextSource : 0)) };
  }
  function visible(state, mode) {
    return mode === 'merge' ? state.sources.map(s => state.items.find(i => i.source === s.id)).filter(Boolean) : state.items;
  }
  function move(state, id, delta, mode) {
    const result = clone(state);
    const sequence = visible(result, mode);
    const from = sequence.findIndex(i => i.id === id);
    const to = from + delta;
    if (from < 0 || to < 0 || to >= sequence.length) return result;
    if (mode === 'merge') {
      const [source] = result.sources.splice(from, 1);
      result.sources.splice(to, 0, source);
      result.items = result.sources.flatMap(s => result.items.filter(i => i.source === s.id));
    } else {
      const [item] = result.items.splice(from, 1);
      result.items.splice(to, 0, item);
    }
    return result;
  }
  function removeSource(state, id) {
    const result = clone(state);
    result.sources = result.sources.filter(s => s.id !== id);
    result.items = result.items.filter(i => i.source !== id);
    return result;
  }
  const Model = { clone, empty, demo, validate, visible, move, removeSource, sourceId };
  if (typeof module !== 'undefined' && module.exports) module.exports = Model;
  if (typeof document === 'undefined') return;

  const $ = selector => document.querySelector(selector);
  const $$ = selector => Array.from(document.querySelectorAll(selector));
  function element(tag, className, text) {
    const node = document.createElement(tag);
    if (className) node.className = className;
    if (text !== undefined) node.textContent = text;
    return node;
  }
  function icon(name) {
    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    const use = document.createElementNS(svg.namespaceURI, 'use');
    use.setAttribute('href', '#i-' + name);
    svg.setAttribute('aria-hidden', 'true'); svg.append(use); return svg;
  }
  function getStorage(storage, key, fallback) {
    try { return JSON.parse(storage.getItem(key)) ?? fallback; } catch { return fallback; }
  }
  function setStorage(storage, key, value) {
    try { storage.setItem(key, JSON.stringify(value)); } catch { /* In-memory use still works. */ }
  }
  let toastTimer;
  function toast(message) {
    clearTimeout(toastTimer); $('#toast').textContent = message; $('#toast').hidden = false;
    toastTimer = setTimeout(() => { $('#toast').hidden = true; }, 4500);
  }
  function info(title, message) {
    $('#info-title').textContent = title; $('#info-description').textContent = message; $('#info-dialog').showModal();
  }
  $$('[data-close-dialog]').forEach(button => button.addEventListener('click', () => button.closest('dialog').close()));
  $$('dialog').forEach(dialog => {
    dialog.setAttribute('aria-labelledby', dialog.querySelector('h2').id || (dialog.id + '-heading'));
    if (!dialog.querySelector('h2').id) dialog.querySelector('h2').id = dialog.id + '-heading';
  });
  const catalogOnly = document.body.dataset.catalogOnly === 'true';
  document.addEventListener('click',event=>{const button=event.target.closest('[data-planned]');if(button)info(button.dataset.planned+' — belum tersedia di web',catalogOnly?'Alat ini belum tersedia di browser. Gunakan aplikasi desktop OTO untuk fitur yang didukung. Tidak ada dokumen yang diunggah.':'Fitur ini masih berupa rancangan katalog.');});
  $('#info-download')?.addEventListener('click', () => $('#info-dialog').close());
  document.addEventListener('click',event=>{if(event.target.closest('[data-download]'))toast('Unduhan diminta. Periksa daftar unduhan browser. Aplikasi tidak dipasang otomatis.');});
  $('#theme-toggle').addEventListener('click', () => {
    const theme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
    document.documentElement.dataset.theme = theme;
    try { localStorage.setItem('oto.web.theme', theme); } catch { /* Optional preference only. */ }
    $('#theme-toggle').setAttribute('aria-label', theme === 'dark' ? 'Gunakan tema terang' : 'Gunakan tema gelap');
  });
  const favoriteValue = getStorage(localStorage, 'oto.web.favorites', ['merge', 'organize']);
  const favorites = new Set(Array.isArray(favoriteValue) ? favoriteValue.filter(id => typeof id === 'string').slice(0, 50) : []);
  let category = 'Semua';
  function filterCatalog() {
    if (!$('.catalog')) return;
    const term = ($('#tool-search').value || '').toLocaleLowerCase('id').trim();
    const home = $('.catalog').hasAttribute('data-home-catalog');
    const words = term.split(/\s+/).filter(Boolean);
    let count = 0, unavailableCount = 0;
    $$('.tool-card').forEach(card => {
      const search = card.dataset.search.toLocaleLowerCase('id');
      const show = (category === 'Semua' || card.dataset.category === category) && (home ? words.every(word => search.includes(word)) : search.includes(term)) && ($('.catalog').dataset.favoritesOnly !== 'true' || favorites.has(card.dataset.id));
      card.hidden = !show;
      if (show) { if (home && card.classList.contains('planned')) unavailableCount++; else count++; }
    });
    $('#result-count').textContent = count + ' alat'; $('#no-results').hidden = count > 0;
    if (home) {
      $('#unavailable-count').textContent = unavailableCount + ' alat';
      $('#unavailable-tools').hidden = unavailableCount === 0;
      if (term || category !== 'Semua' || $('.catalog').dataset.favoritesOnly === 'true') $('#unavailable-tools').open = unavailableCount > 0;
      else $('#unavailable-tools').open = false;
      $('#result-announcement').textContent = count + ' alat aktif ditampilkan.' + (unavailableCount ? ' ' + unavailableCount + ' alat belum tersedia di web pada daftar terpisah.' : '');
    }
  }
  function updateFavorites() {
    $$('[data-favorite]').forEach(button => {
      const active = favorites.has(button.dataset.favorite);
      button.setAttribute('aria-pressed', String(active)); button.classList.toggle('is-favorite', active);
      const title = button.closest('.tool-card')?.querySelector('h3')?.textContent || document.querySelector('h1')?.textContent || 'alat';
      const label = active ? 'Hapus ' + title + ' dari favorit' : 'Tambahkan ' + title + ' ke favorit';
      button.setAttribute('aria-label', label); button.title = label;
    });
    $('#favorites-count').textContent = String(favorites.size); filterCatalog();
  }
  document.addEventListener('click', event => {
    const button=event.target.closest('[data-favorite]');if(!button)return;
    const id = button.dataset.favorite;
    if (favorites.has(id)) favorites.delete(id); else favorites.add(id);
    setStorage(localStorage, 'oto.web.favorites', Array.from(favorites)); updateFavorites();
  });
  document.addEventListener('input',event=>{if(event.target.id==='tool-search')filterCatalog();});
  document.addEventListener('keydown', event => {
    if (event.key !== '/' || event.ctrlKey || event.metaKey || event.altKey || event.target.closest('input,textarea,select,[contenteditable]') || document.querySelector('dialog[open]') || !$('[data-home-catalog]')) return;
    event.preventDefault(); $('#tool-search').focus();
  });
  document.addEventListener('click',event=>{const tab=event.target.closest('.category-tab');if(!tab)return;
    category = tab.dataset.category;
    $$('.category-tab').forEach(t => { t.classList.toggle('selected', t === tab); t.setAttribute('aria-pressed', String(t === tab)); }); filterCatalog();
  });
  document.addEventListener('click',event=>{if(event.target.closest('#reset-search')){ $('#tool-search').value = ''; $('.category-tab').click(); $('#tool-search').focus(); }});
  window.OtoSiteRefresh=()=>{category=$('.category-tab.selected')?.dataset.category||'Semua';updateFavorites();};
  updateFavorites();

  const workspace = $('.workspace-layout[data-workspace]');
  if (!workspace) return;
  const mode = workspace.dataset.workspace;
  $$('.drop-hint,.inline-drop').forEach(area => {
    area.classList.add('tooltip-target'); area.dataset.tooltip = 'Ctrl + V'; area.tabIndex = 0;
  });
  const key = 'oto.web.preview.workspace';
  let state = validate(getStorage(sessionStorage, key, empty()));
  let undo = [], redo = [], targetUrl = '';
  const persist = () => setStorage(sessionStorage, key, state);
  function change(next, announcement) {
    undo.push(clone(state)); if (undo.length > 20) undo.shift(); redo = [];
    state = next; persist(); render(); if (announcement) toast(announcement);
  }
  function history(direction) {
    const source = direction === 'undo' ? undo : redo;
    const destination = direction === 'undo' ? redo : undo;
    if (!source.length) return;
    destination.push(clone(state)); state = source.pop(); persist(); render();
    toast(direction === 'undo' ? 'Perubahan diurungkan.' : 'Perubahan diulangi.');
  }
  function actionButton(label, symbol, handler, disabled = false) {
    const button = element('button', 'icon-button'); button.type = 'button'; button.title = label; button.setAttribute('aria-label', label); button.disabled = disabled;
    if (symbol.length === 1) button.textContent = symbol; else button.append(icon(symbol));
    button.addEventListener('click', handler); return button;
  }
  function renderPaper(source, item) {
    const paper = element('div', 'paper-preview rotation-' + item.rotation + (source.example ? '' : ' real-file'));
    if (!source.example) { paper.append(icon('file'), element('strong', '', 'PDF'), element('small', '', 'Pratinjau isi belum tersedia')); return paper; }
    paper.append(element('span', 'paper-brand', 'OTO · DOKUMEN CONTOH'), element('strong', 'paper-title', source.name.replace('.pdf', '')));
    for (let n = 0; n < 3; n++) paper.append(element('i', 'paper-line' + (n === 2 ? ' short' : '')));
    const table = element('div', 'paper-table'); for (let n = 0; n < 18; n++) table.append(element('i')); paper.append(table);
    for (let n = 0; n < 3; n++) paper.append(element('i', 'paper-line' + (n === 2 ? ' short' : '')));
    paper.append(element('span', 'paper-stamp', source.id + ' · Halaman ' + item.originalPage)); return paper;
  }
  function render() {
    const focusedCard = document.activeElement?.closest('.page-card');
    const focusedItem = focusedCard?.dataset.item;
    const focusedAction = focusedCard ? document.activeElement.getAttribute('aria-label') : null;
    const count = state.sources.length, items = visible(state, mode), hasFiles = count > 0;
    $('#empty-workspace').hidden = hasFiles; $('#loaded-workspace').hidden = !hasFiles;
    $('#close-documents').hidden = !hasFiles; $('#sources-section').hidden = !hasFiles;
    const allExample = hasFiles && state.sources.every(s => s.example);
    $('#document-count').textContent = String(items.length); $('#source-count').textContent = count + ' file';
    $('#workspace-summary').textContent = mode === 'organize' && allExample ? items.length + ' halaman contoh dari ' + count + ' file PDF' : count + ' file PDF · ' + (allExample ? 'Dokumen contoh' : 'Hanya metadata, isi belum dibaca');
    $('#example-label').textContent = allExample ? 'DOKUMEN CONTOH' : 'METADATA FILE';
    $('#undo').disabled = !undo.length; $('#redo').disabled = !redo.length;
    const grid = $('#page-grid'); grid.replaceChildren();
    items.forEach((item, index) => {
      const source = state.sources.find(s => s.id === item.source);
      const card = element('article', 'page-card source-' + source.color); card.draggable = true; card.dataset.item = item.id;
      card.setAttribute('aria-label', source.name + ', ' + (item.originalPage ? 'halaman asli ' + item.originalPage : 'isi belum dibaca') + ', posisi ' + (index + 1));
      const head = element('div', 'card-head'); head.append(element('span', 'source-chip', source.id + (mode === 'organize' && item.originalPage ? ' · ' + item.originalPage : '')), element('span', 'page-position', 'Urutan ' + (index + 1)));
      const footer = element('div', 'card-footer'); footer.append(element('strong', '', source.name), element('small', '', source.example ? (mode === 'merge' ? source.pages + ' halaman contoh' : 'Sumber ' + source.id + ' · halaman asli ' + item.originalPage) : 'Jumlah halaman belum dibaca'));
      footer.title = source.name + ' · ' + Math.ceil(source.size / 1024) + ' KB';
      const actions = element('div', 'card-actions');
      actions.append(actionButton('Pindah ke kiri: ' + source.id, '←', () => change(move(state, item.id, -1, mode), 'Urutan diperbarui. Identitas sumber tetap.'), index === 0));
      actions.append(actionButton('Pindah ke kanan: ' + source.id, '→', () => change(move(state, item.id, 1, mode), 'Urutan diperbarui. Identitas sumber tetap.'), index === items.length - 1));
      if (mode === 'organize') {
        actions.append(actionButton('Putar contoh halaman ' + source.id + ' ' + item.originalPage, 'rotate', () => {
          const next = clone(state); const selected = next.items.find(i => i.id === item.id); selected.rotation = (selected.rotation + 90) % 360; change(next);
        }, !source.example));
        actions.append(actionButton('Duplikat contoh halaman ' + source.id + ' ' + item.originalPage, 'copy', () => {
          const next = clone(state); next.items.splice(index + 1, 0, { ...item, id: 'copy-' + Date.now().toString(36) + '-' + Math.random().toString(36).slice(2, 7) }); change(next, 'Salinan contoh ditambahkan; nomor halaman asli tetap.');
        }, !source.example || state.items.length >= 100));
      }
      actions.append(actionButton('Keluarkan ' + source.id + (mode === 'organize' && item.originalPage ? ' halaman ' + item.originalPage : ''), 'close', () => {
        if (mode === 'merge') change(removeSource(state, source.id), 'Dokumen dikeluarkan dari daftar. File asli tidak dihapus.');
        else {
          const next = clone(state); next.items = next.items.filter(i => i.id !== item.id); next.sources = next.sources.filter(s => next.items.some(i => i.source === s.id)); change(next, 'Halaman dikeluarkan dari susunan contoh.');
        }
      }));
      card.append(head, renderPaper(source, item), footer, actions);
      card.addEventListener('dragstart', event => { event.dataTransfer.setData('text/x-oto-card', item.id); event.dataTransfer.effectAllowed = 'move'; card.classList.add('dragging'); });
      card.addEventListener('dragend', () => { card.classList.remove('dragging'); $$('.drop-target').forEach(c => c.classList.remove('drop-target')); });
      card.addEventListener('dragover', event => { if (event.dataTransfer.types.includes('text/x-oto-card')) { event.preventDefault(); card.classList.add('drop-target'); } });
      card.addEventListener('dragleave', () => card.classList.remove('drop-target'));
      card.addEventListener('drop', event => {
        const id = event.dataTransfer.getData('text/x-oto-card'); if (!id) return;
        event.preventDefault(); event.stopPropagation();
        const from = visible(state, mode).findIndex(i => i.id === id);
        if (from >= 0 && from !== index) change(move(state, id, index - from, mode), 'Urutan diperbarui. Identitas sumber tetap.');
      });
      grid.append(card);
    });
    $('#source-list').replaceChildren();
    state.sources.forEach(source => {
      const row = element('div', 'source-row source-' + source.color); const details = element('div');
      details.append(element('strong', '', source.name), element('small', '', source.example ? source.pages + ' halaman asli · contoh' : Math.ceil(source.size / 1024) + ' KB · isi belum dibaca'));
      row.append(element('span', 'source-chip', source.id), details, actionButton('Keluarkan file sumber ' + source.id, 'close', () => change(removeSource(state, source.id), 'File sumber dikeluarkan dari daftar saja.'))); $('#source-list').append(row);
    });
    const ready = mode === 'merge' ? count >= 2 : items.length > 0;
    $('#execute').disabled = !ready; $('#ready-message').classList.toggle('is-ready', ready);
    $('#ready-message span').textContent = ready ? (allExample ? (mode === 'merge' ? count + ' PDF contoh siap disusun.' : items.length + ' halaman contoh siap disusun.') : 'Metadata siap. Mesin PDF belum tersedia.') : (count === 1 && mode === 'merge' ? 'Tambahkan satu PDF lagi.' : 'Tambahkan dokumen untuk melanjutkan.');
    if (focusedItem) {
      const replacement = $$('.page-card').find(card => card.dataset.item === focusedItem);
      const action = replacement && Array.from(replacement.querySelectorAll('button')).find(button => button.getAttribute('aria-label') === focusedAction && !button.disabled);
      if (action) action.focus({ preventScroll: true });
      else if (replacement) { replacement.tabIndex = -1; replacement.focus({ preventScroll: true }); }
      else $('#close-documents').hidden ? $('[data-pick]').focus() : $('#close-documents').focus();
    }
  }
  function loadDemo() {
    if (state.sources.length) { info('Contoh tidak mengganti daftar Anda', 'Tutup dokumen terlebih dahulu untuk memuat contoh. File asli Anda tetap utuh.'); return; }
    change(demo(), 'Contoh dokumen dimuat. Ini bukan dokumen asli.');
  }
  $('#load-demo').addEventListener('click', loadDemo); $('#empty-demo').addEventListener('click', loadDemo);
  $('#undo').addEventListener('click', () => history('undo')); $('#redo').addEventListener('click', () => history('redo'));
  $('#card-size').addEventListener('change', event => { $('#page-grid').className = 'page-grid size-' + event.target.value; });
  function addFiles(files) {
    const selected = Array.from(files).filter(file => /\.pdf$/i.test(file.name) && file.name.length <= 200 && file.size <= 100 * 1024 * 1024);
    if (!selected.length) { info('Pilih file PDF', 'Prototipe menerima metadata file berekstensi .pdf hingga 100 MB per file. Isi file tidak dibaca, divalidasi, atau diunggah.'); return; }
    let next = state.sources.some(s => s.example) ? empty() : clone(state);
    let added = 0;
    for (const file of selected) {
      if (next.sources.length >= 50 || next.nextSource >= 50) break;
      if (next.sources.some(s => s.name === file.name && s.size === file.size)) continue;
      const index = next.nextSource++, id = sourceId(index);
      next.sources.push({ id, name: file.name, size: file.size, pages: null, example: false, color: index % 5 });
      next.items.push({ id: id + '-file', source: id, originalPage: null, rotation: 0 }); added++;
    }
    if (!added) { toast('Tidak ada file baru. Duplikat diabaikan; batas 50 dokumen per sesi.'); return; }
    change(next, added + ' metadata PDF ditambahkan. Isi tidak dibaca atau diunggah.' + (selected.length !== files.length ? ' File tidak sesuai diabaikan.' : ''));
  }
  $$('[data-pick]').forEach(button => button.addEventListener('click', () => $('#file-input').click()));
  $('#file-input').addEventListener('change', event => { addFiles(event.target.files); event.target.value = ''; });
  document.addEventListener('paste', event => { if (!$('dialog[open]') && event.clipboardData?.files.length) { event.preventDefault(); addFiles(event.clipboardData.files); } });
  document.addEventListener('dragover', event => { if (event.dataTransfer.types.includes('Files')) { event.preventDefault(); if (!$('dialog[open]')) $('.document-panel').classList.add('drop-target'); } });
  document.addEventListener('dragleave', event => { if (!event.relatedTarget) $('.document-panel').classList.remove('drop-target'); });
  document.addEventListener('drop', event => {
    $('.document-panel').classList.remove('drop-target');
    if (event.dataTransfer.files.length) { event.preventDefault(); if (!$('dialog[open]')) addFiles(event.dataTransfer.files); }
  });
  document.addEventListener('keydown', event => {
    if (!(event.ctrlKey || event.metaKey) || event.altKey || $('dialog[open]')) return;
    if (event.target.closest('input,textarea,select,[contenteditable="true"]')) return;
    if (event.key.toLowerCase() === 'o') { event.preventDefault(); $('#file-input').click(); }
    if (event.key.toLowerCase() === 'z') { event.preventDefault(); history(event.shiftKey ? 'redo' : 'undo'); }
  });
  $('#close-documents').addEventListener('click', () => { $('#close-dialog .dialog-biblo').src = '/assets/' + workspace.dataset.biblo; $('#close-dialog').showModal(); });
  $('#close-confirm').addEventListener('click', () => {
    state = empty(); undo = []; redo = []; persist(); render(); $('#close-dialog').close(); $('[data-pick]').focus(); toast('Ruang kerja dikosongkan. File asli tidak diubah.');
  });
  $('#execute').addEventListener('click', () => {
    if (state.sources.some(s => !s.example)) { info('Pengolahan PDF belum tersedia', 'Nama file sudah masuk ke pratinjau, tetapi isi PDF belum dibaca atau diproses. Tidak ada file hasil yang dibuat. Gunakan contoh dokumen untuk mencoba tampilan hasil.'); return; }
    let name = $('#output-name').value.trim();
    if (!name || /[<>:"/\\|?*\x00-\x1F]/.test(name)) { info('Periksa nama file', 'Gunakan nama file tanpa karakter khusus seperti /, \\, :, atau *.'); return; }
    if (!/\.pdf$/i.test(name)) name += '.pdf';
    $('#result-name').textContent = name; $('#result-biblo').src = '/assets/' + workspace.dataset.biblo; $('#result-dialog').showModal();
  });
  $$('a[href^="/"]').forEach(link => link.addEventListener('click', event => {
    if (!state.sources.length || event.ctrlKey || event.metaKey || event.shiftKey || event.button !== 0 || link.pathname === location.pathname) return;
    event.preventDefault(); targetUrl = link.pathname;
    const title = targetUrl === '/alat/merge' ? 'Pindah ke Gabungkan PDF?' : targetUrl === '/alat/organize' ? 'Pindah ke Atur halaman PDF?' : 'Tinggalkan ruang kerja?';
    $('#leave-title').textContent = title; $('#leave-message').textContent = state.sources.length + ' file ada dalam daftar pratinjau. Nama file dan susunan tetap tersedia selama tab ini terbuka. Riwayat urungkan tidak dibawa ke halaman lain.'; $('#leave-dialog').showModal();
  }));
  $('#leave-confirm').addEventListener('click', () => { persist(); location.assign(targetUrl); });
  render();
})();
