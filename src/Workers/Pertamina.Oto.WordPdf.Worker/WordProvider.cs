using System.Security.Cryptography;
using Microsoft.Win32;
namespace Pertamina.Oto.LocalPdf;
internal static class WordProvider
{
 internal const string AdmittedHash="A02EC8E28E1210A13F1C8D1874C9F10D6ED987A18365CBAAE72D8403BA2D906D";
 internal static bool MatchesCandidate(string path){if(!File.Exists(path))return false;using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream))==AdmittedHash;}
 internal static string? Find()
 {
  if(!OperatingSystem.IsWindows())return null;
  foreach(var view in new[]{RegistryView.Registry64,RegistryView.Registry32})
  {
   using var baseKey=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,view);
   using var key=baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WINWORD.EXE");
   var path=key?.GetValue(null) as string;
   if(path is null || !File.Exists(path))continue;
   if(MatchesCandidate(path))return path;
  }
  return null;
 }
}


