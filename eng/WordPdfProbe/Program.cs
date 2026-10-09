using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
internal static class Program {
 [DllImport("user32.dll")] static extern bool EnumWindows(Callback cb,nint arg);
 [DllImport("user32.dll")] static extern bool EnumChildWindows(nint p, Callback cb, nint arg);
 [DllImport("user32.dll")] static extern int GetClassName(nint w, StringBuilder n, int cap);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(nint w, out uint pid);
 [DllImport("oleacc.dll")] static extern int AccessibleObjectFromWindow(nint w, uint id, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object result);
 delegate bool Callback(nint w, nint arg);
 [STAThread] static int Main() {
 var prior=Process.GetProcessesByName("WINWORD").Select(p=>p.Id).ToHashSet();
 using var owned=Process.Start(new ProcessStartInfo {FileName=@"C:\Program Files\Microsoft Office\Root\Office16\WINWORD.EXE",Arguments="/w /a /q",UseShellExecute=false,WindowStyle=ProcessWindowStyle.Hidden});
 if(owned is null || prior.Contains(owned.Id)) return 2;
 try {
 for(var attempt=0;attempt<100;attempt++) {
 owned.Refresh(); if(owned.HasExited){Console.WriteLine("OWNED_PROCESS_EXITED");return 3;}
 nint hwnd=0; EnumWindows((w,_)=>{GetWindowThreadProcessId(w,out var p);var n=new StringBuilder(100);GetClassName(w,n,100);if(p==owned.Id && n.ToString()=="OpusApp"){hwnd=w;return false;}return true;},0);
 if(hwnd!=0){GetWindowThreadProcessId(hwnd,out var pid);if(pid!=owned.Id)return 4;
 object? native=null;
 EnumChildWindows(hwnd,(child,_)=>{var name=new StringBuilder(100);GetClassName(child,name,100);if(name.ToString()!="_WwG")return true;GetWindowThreadProcessId(child,out var childPid);if(childPid!=owned.Id)return true;var iid=new Guid("00020400-0000-0000-C000-000000000046");if(AccessibleObjectFromWindow(child,0xfffffff0,ref iid,out var found)==0)native=found;return native is null;},0);
 if(native is not null){Console.WriteLine("NATIVE_TYPE "+native.GetType().Name);dynamic window=native;dynamic app=window.Application;Console.WriteLine("APPLICATION_READY");GetWindowThreadProcessId((nint)(int)window.Hwnd,out var appPid);if(appPid!=owned.Id)return 5;Console.WriteLine("OWNED_NATIVE_OBJECT_READY "+app.Version);Marshal.FinalReleaseComObject(app);Marshal.FinalReleaseComObject(native);return 0;}}
 Thread.Sleep(100);
 }Console.WriteLine("NO_OWNED_DOCUMENT_NATIVE_OBJECT");return 6;
 }catch(Exception e){Console.WriteLine("PROBE_FAILED "+e.GetType().Name+" "+e.Message);return 7;}
 finally{try{if(!owned.HasExited){owned.Kill(false);owned.WaitForExit(3000);}}catch(InvalidOperationException){}}
 }
}




