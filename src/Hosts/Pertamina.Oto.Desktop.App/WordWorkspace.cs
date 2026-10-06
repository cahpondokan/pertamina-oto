using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Pertamina.Oto.LocalPdf;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
 private StackPanel? wordSources;
 private void BuildWordWorkspace(StackPanel inputArea)
 {
  sourcePreviewHost=null;sourceChoice=null;inputArea.Children.Clear();
  inputArea.Children.Add(Copy("Satu DOCX lokal · A4 portrait",14));
  var toolbar=new StackPanel{Orientation=Orientation.Horizontal,Spacing=8};
  toolbar.Children.Add(ActionButton("Pilih DOCX",async()=>await PickFilesAsync()));toolbar.Children.Add(ActionButton("Tempel file",async()=>await PasteWordAsync()));inputArea.Children.Add(toolbar);
  wordSources=new StackPanel{Spacing=12};inputArea.Children.Add(wordSources);RefreshWordSource();
  var drop=new Border{Child=Copy("Tarik satu file DOCX ke sini",14),Padding=new Thickness(24),Background=Brush(237,243,251),CornerRadius=new CornerRadius(12),AllowDrop=true};
  AutomationProperties.SetName(drop,"Area drop satu dokumen Word DOCX");
  drop.DragOver+=(_,e)=>{e.AcceptedOperation=active is null&&e.DataView.Contains(StandardDataFormats.StorageItems)?DataPackageOperation.Copy:DataPackageOperation.None;};
  drop.Drop+=async(_,e)=>{try{if(e.DataView.Contains(StandardDataFormats.StorageItems))await AcceptWordInputsAsync(await e.DataView.GetStorageItemsAsync());}catch(Exception){status.Text="File drop tidak dapat dibaca.";}};inputArea.Children.Add(drop);
  inputArea.Children.Add(Copy("Pratinjau PDF tersedia setelah konversi berhasil. Dokumen Word dibuka read-only dalam proses milik aplikasi; file asli tetap utuh.",13));
  toolOptions.Children.Insert(0,Copy("Memerlukan Microsoft Word lokal terpasang, berlisensi, dan versi yang telah diverifikasi. Tidak ada upload dokumen. Font/layout dapat berbeda; periksa hasil. A3/Letter/landscape, DOC/DOCM, macro, link eksternal, objek tertanam dan field aktif belum didukung.",13));
  run.Content="Konversi ke PDF";run.IsEnabled=WordReady();
  if(!run.IsEnabled)status.Text="Provider Word yang disetujui tidak tersedia atau versinya berubah. Aplikasi tidak menginstal Word; konversi belum dapat dijalankan.";
 }
 private static bool WordReady(){try{return WordProvider.Find() is not null;}catch(Exception){return false;}}
 private void RefreshWordSource()
 {
  if(selected?.Id!="word-pdf"||wordSources is null)return;wordSources.Children.Clear();
  if(files.Count==0){wordSources.Children.Add(Copy("Belum ada DOCX dipilih."));return;}
  wordSources.Children.Add(Copy(Path.GetFileName(files[0]),17));
  wordSources.Children.Add(ActionButton("Keluarkan file",()=>{files.Clear();RefreshWordSource();}));
 }
 private async Task AcceptWordInputsAsync(IEnumerable<IStorageItem> items)
 {
  if(active is not null||selected?.Id!="word-pdf")return;
  var input=items.Take(2).ToArray();
  if(input.Length!=1||input[0] is not StorageFile file||!file.FileType.Equals(".docx",StringComparison.OrdinalIgnoreCase)||string.IsNullOrEmpty(file.Path))
  {status.Text="Pilih tepat satu DOCX lokal. DOC/DOCM belum didukung.";return;}
  if(!WordPdfClient.IsLocalPlainPath(file.Path)||(await file.GetBasicPropertiesAsync()).Size>100*1024*1024){status.Text="Gunakan DOCX lokal maksimal 100 MB tanpa tautan file atau folder jaringan.";return;}
  files.Clear();files.Add(file.Path);RefreshWordSource();status.Text="DOCX dipilih. Struktur aman diperiksa sebelum Word dibuka.";
 }
 private async Task PasteWordAsync()
 {
  try{var data=Clipboard.GetContent();if(!data.Contains(StandardDataFormats.StorageItems)){status.Text="Salin satu DOCX dari File Explorer, bukan teks.";return;}await AcceptWordInputsAsync(await data.GetStorageItemsAsync());}
  catch(Exception){status.Text="Clipboard tidak dapat dibaca.";}
 }
}

