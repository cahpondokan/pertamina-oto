using System.Security.Cryptography;
using Microsoft.Win32;
namespace Pertamina.Oto.LocalPdf;
internal static class WordProvider
{
 internal const string AdmittedHash="0F96D6172DD6CC9EDA5DDB786FEE5D8F9C8B4BEE96F56DB2DAB6F5888F3BE882";
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


