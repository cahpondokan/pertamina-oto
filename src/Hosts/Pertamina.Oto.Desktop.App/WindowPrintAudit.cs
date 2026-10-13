#if DEBUG
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Pertamina.Oto.Desktop.App;
internal sealed partial class MainWindow
{
    private async Task AuditWindowPrintStatesAsync(string source, Action<bool,string> check, Func<string,ContentDialog,Task> observe)
    {
        static void Invoke(Button button) => ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        static async Task<bool> Until(Func<bool> condition)
        { for (var i=0;i<100;i++) { if (condition()) return true; await Task.Delay(100); } return false; }
        var printer = PrintDevice.Names().FirstOrDefault(n => n.Equals("Microsoft Print to PDF",StringComparison.OrdinalIgnoreCase));
        check(printer is not null,"existing virtual driver available for dialog-only simulated states");
        if (printer is null) return;
        try
        {
            auditPrintPrinters = () => [printer];
            // Installed driver is read for preview/settings; submission is always replaced.
            var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var calls = 0;
            auditPrintWorker = (_,progress,_) => { calls++; progress(1); return completion.Task; };
            auditPrintDialog = async dialog =>
            {
                await Task.Delay(800);
                var submit = Descendants(dialog).OfType<Button>().First(b => AutomationProperties.GetName(b)=="Cetak dokumen");
                var cancel = Descendants(dialog).OfType<Button>().First(b => Equals(b.Content,"Batal"));
                var status = Descendants(dialog).OfType<TextBlock>().First(t => AutomationProperties.GetName(t)=="Status pengiriman cetak");
                bool Visible(FrameworkElement control)
                {
                    if (control.Visibility != Visibility.Visible || control.ActualWidth <= 0 || control.ActualHeight <= 0) return false;
                    var bounds = control.TransformToVisual((UIElement)Content).TransformBounds(new Rect(0,0,control.ActualWidth,control.ActualHeight));
                    for (DependencyObject? p=VisualTreeHelper.GetParent(control);p is not null;p=VisualTreeHelper.GetParent(p))
                        if (p is FrameworkElement frame && frame.ActualWidth>0 && frame.ActualHeight>0)
                        {
                            var view = frame.TransformToVisual((UIElement)Content).TransformBounds(new Rect(0,0,frame.ActualWidth,frame.ActualHeight));
                            if (bounds.Left<view.Left-1 || bounds.Top<view.Top-1 || bounds.Right>view.Right+1 || bounds.Bottom>view.Bottom+1) return false;
                        }
                    return true;
                }
                async Task State(string label)
                {
                    dialog.UpdateLayout(); check(Visible(submit)&&Visible(cancel)&&(status.Text.Length==0||Visible(status)),"Print footer fully visible through ancestors " + label);
                    await observe("print-simulated-"+label,dialog);
                }
                check(await Until(() => submit.IsEnabled),"Print preparation before simulated submission");
                if (!submit.IsEnabled) { dialog.Hide(); return; }
                await State("ready"); Invoke(submit);
                check(await Until(() => status.Text.StartsWith("Mengirim",StringComparison.Ordinal)),"Print sending visible");
                await State("sending"); check(calls==1 && !submit.IsEnabled && cancel.IsEnabled,"Print no duplicate submission while sending");
                completion.SetResult(42);
                check(await Until(() => status.Text.StartsWith("Diterima antrean Windows",StringComparison.Ordinal)&&submit.IsEnabled),"Print simulated queue accepted");
                await State("queued");
                auditPrintWorker = (_,_,_) => { calls++; throw new InvalidOperationException("SYNTHETIC_PRIVATE_VALUE"); };
                Invoke(submit); check(await Until(() => status.Text.StartsWith("Gagal:",StringComparison.Ordinal)&&submit.IsEnabled),"Print simulated failure and retry available");
                check(!status.Text.Contains("SYNTHETIC_PRIVATE_VALUE",StringComparison.Ordinal),"Print simulated error safe text"); await State("error");
                auditPrintWorker = async (_,_,token) => { calls++; await Task.Delay(Timeout.Infinite,token); return 0; };
                Invoke(submit); check(await Until(() => !submit.IsEnabled),"Print simulated cancellable submission"); Invoke(cancel);
                check(await Until(() => status.Text.StartsWith("Dibatalkan",StringComparison.Ordinal)&&submit.IsEnabled),"Print simulated cancel visible");
                await State("cancel"); check(calls==3,"Print no automatic retry after cancel");
                auditPrintStatus=PrintDevice.WindowsStatus(0x80);
                Descendants(dialog).OfType<NumberBox>().First().Value=2; await Task.Delay(400);
                check(!submit.IsEnabled,"Print simulated offline blocks submission"); await State("offline");
                dialog.Hide();
            };
            await ShowPrintAsync(source,"SIMULASI audit ukuran · Tidak mengirim job");
        }
        finally { auditPrintPrinters=null; auditPrintWorker=null; auditPrintDialog=null; auditPrintStatus=null; }
    }
}
#endif
