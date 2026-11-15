# Lector y escritura de una COPIA LOCAL del stream XML de un diagrama SSMS.
# No conecta a SQL Server ni crea controles del diseñador.
if (-not ('G07NativeStorage' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
[ComImport,Guid("0000000B-0000-0000-C000-000000000046"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IG07Storage {
 void CreateStream([MarshalAs(UnmanagedType.LPWStr)] string name,uint mode,uint r1,uint r2,out IStream stream);
 void OpenStream([MarshalAs(UnmanagedType.LPWStr)] string name,IntPtr r1,uint mode,uint r2,out IStream stream);
}
public static class G07NativeStorage {
 [DllImport("ole32.dll",CharSet=CharSet.Unicode)] static extern int StgOpenStorage(string file,IntPtr priority,uint mode,IntPtr exclude,uint reserved,out IG07Storage storage);
 public static byte[] Read(string file,string name) {
  IG07Storage storage=null; IStream stream=null; IntPtr count=Marshal.AllocHGlobal(4);
  try {
   Marshal.ThrowExceptionForHR(StgOpenStorage(file,IntPtr.Zero,0x20,IntPtr.Zero,0,out storage));
   storage.OpenStream(name,IntPtr.Zero,0x10,0,out stream);
   STATSTG stat;stream.Stat(out stat,1);byte[] result=new byte[checked((int)stat.cbSize)];
   int offset=0;
   while(offset<result.Length){byte[] part=new byte[result.Length-offset];stream.Read(part,part.Length,count);int read=Marshal.ReadInt32(count);if(read<=0)throw new EndOfStreamException();Buffer.BlockCopy(part,0,result,offset,read);offset+=read;}
   return result;
  }finally{Marshal.FreeHGlobal(count);if(stream!=null)Marshal.ReleaseComObject(stream);if(storage!=null)Marshal.ReleaseComObject(storage);}
 }
 public static void WriteExistingStream(string file,string name,byte[] bytes) {
  IG07Storage storage=null;IStream stream=null;IntPtr count=Marshal.AllocHGlobal(4);
  try {
   Marshal.ThrowExceptionForHR(StgOpenStorage(file,IntPtr.Zero,0x12,IntPtr.Zero,0,out storage));
   storage.OpenStream(name,IntPtr.Zero,0x12,0,out stream);stream.SetSize(bytes.Length);stream.Seek(0,0,IntPtr.Zero);
   stream.Write(bytes,bytes.Length,count);if(Marshal.ReadInt32(count)!=bytes.Length)throw new IOException("Incomplete native stream write");stream.Commit(0);
  }finally{Marshal.FreeHGlobal(count);if(stream!=null)Marshal.ReleaseComObject(stream);if(storage!=null)Marshal.ReleaseComObject(storage);}
 }
}
'@
}

function Get-SsmsNativeXml([string]$Path) {
    [Text.Encoding]::UTF8.GetString([G07NativeStorage]::Read([IO.Path]::GetFullPath($Path),'DdsXMLStream'))
}

function Copy-SsmsNativeWithXml([string]$Source,[string]$Destination,[string]$Xml) {
    $taskSource=[IO.Path]::GetFullPath($Source)
    $taskDestination=[IO.Path]::GetFullPath($Destination)
    if($taskSource -eq $taskDestination){throw 'La transformación requiere una copia local distinta del original.'}
    [void][xml]$Xml
    $taskUnchangedStreams=@('Schema UDV Default','DSREF-SCHEMA-CONTENTS','Schema UDV Default Post V6')
    $taskOriginalFile=[IO.File]::ReadAllBytes($taskSource)
    if($taskOriginalFile.Length % 512 -ne 4 -or [BitConverter]::ToString($taskOriginalFile,$taskOriginalFile.Length-4,4) -ne '62-88-52-14'){
        throw 'Falta la marca final nativa DDS 62-88-52-14; no se transforma un formato desconocido.'
    }
    $taskOriginalStreams=@{}
    foreach($taskName in $taskUnchangedStreams){$taskOriginalStreams[$taskName]=[G07NativeStorage]::Read($taskSource,$taskName)}
    [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($taskDestination))
    [IO.File]::Copy($taskSource,$taskDestination,$true)
    $taskXmlBytes=(New-Object Text.UTF8Encoding $false).GetBytes($Xml)
    [G07NativeStorage]::WriteExistingStream($taskDestination,'DdsXMLStream',$taskXmlBytes)
    # OLE conserva sus streams, pero SSMS también exige este DWORD después del
    # archivo compuesto. No es parte de un stream ni un checksum de contenido:
    # la marca coincide en todos los guardados reales inspeccionados.
    $taskOleFile=[IO.File]::ReadAllBytes($taskDestination)
    if($taskOleFile.Length % 512 -ne 0){throw 'El contenedor OLE reescrito no termina en un límite de sector esperado.'}
    $taskFinalFile=New-Object byte[] ($taskOleFile.Length+4)
    [Buffer]::BlockCopy($taskOleFile,0,$taskFinalFile,0,$taskOleFile.Length)
    [Buffer]::BlockCopy($taskOriginalFile,$taskOriginalFile.Length-4,$taskFinalFile,$taskOleFile.Length,4)
    [IO.File]::WriteAllBytes($taskDestination,$taskFinalFile)
    if([Convert]::ToBase64String([G07NativeStorage]::Read($taskDestination,'DdsXMLStream')) -ne [Convert]::ToBase64String($taskXmlBytes)){throw 'El stream XML leído no coincide con la copia escrita.'}
    foreach($taskName in $taskUnchangedStreams){
        if([Convert]::ToBase64String([G07NativeStorage]::Read($taskDestination,$taskName)) -ne [Convert]::ToBase64String($taskOriginalStreams[$taskName])){throw ('Cambió un stream nativo ajeno al XML: '+$taskName)}
    }
    $taskDestination
}
