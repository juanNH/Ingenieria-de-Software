param(
    [Parameter(Mandatory=$true)][string]$DestinoSinExtension,
    [int]$LadoMayor = 6000
)
# Requiere Copy Diagram to Clipboard ejecutado en SSMS. Solo conserva el EMF
# nativo del portapapeles y lo rasteriza proporcionalmente; no dibuja un DER.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
if(-not ('G07ClipboardExport' -as [type])){
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class G07ClipboardExport {
 [DllImport("user32.dll",SetLastError=true)] public static extern bool OpenClipboard(IntPtr owner);
 [DllImport("user32.dll",SetLastError=true)] public static extern bool CloseClipboard();
 [DllImport("user32.dll",SetLastError=true)] public static extern IntPtr GetClipboardData(uint format);
 [DllImport("gdi32.dll",EntryPoint="CopyEnhMetaFileW",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr CopyEnhMetaFile(IntPtr source,string filename);
 [DllImport("gdi32.dll",SetLastError=true)] public static extern bool DeleteEnhMetaFile(IntPtr handle);
}
'@
}
$taskBase=[IO.Path]::GetFullPath($DestinoSinExtension)
[void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($taskBase))
$taskEmf=$taskBase+'.emf';$taskPng=$taskBase+'.png'
if(-not [G07ClipboardExport]::OpenClipboard([IntPtr]::Zero)){throw 'No se pudo leer el EMF del portapapeles.'}
try{
    $taskHandle=[G07ClipboardExport]::GetClipboardData(14)
    if($taskHandle -eq [IntPtr]::Zero){throw 'SSMS no dejó un CF_ENHMETAFILE en el portapapeles.'}
    $taskCopy=[G07ClipboardExport]::CopyEnhMetaFile($taskHandle,$taskEmf)
    if($taskCopy -eq [IntPtr]::Zero){throw 'No se pudo conservar el EMF nativo.'}
    [void][G07ClipboardExport]::DeleteEnhMetaFile($taskCopy)
}finally{[void][G07ClipboardExport]::CloseClipboard()}
$taskMeta=New-Object Drawing.Imaging.Metafile $taskEmf
try{
    $taskScale=[Math]::Min($LadoMayor/[Math]::Max($taskMeta.Width,$taskMeta.Height),[Math]::Sqrt(24000000.0/($taskMeta.Width*$taskMeta.Height)))
    $taskWidth=[Math]::Max(1,[int][Math]::Round($taskMeta.Width*$taskScale))
    $taskHeight=[Math]::Max(1,[int][Math]::Round($taskMeta.Height*$taskScale))
    $taskBitmap=New-Object Drawing.Bitmap $taskWidth,$taskHeight
    try{
        $taskG= [Drawing.Graphics]::FromImage($taskBitmap)
        try{
            $taskG.Clear([Drawing.Color]::White)
            $taskG.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::HighQuality
            $taskG.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $taskG.TextRenderingHint=[Drawing.Text.TextRenderingHint]::ClearTypeGridFit
            $taskG.DrawImage($taskMeta,0,0,$taskWidth,$taskHeight)
        }finally{$taskG.Dispose()}
        $taskBitmap.Save($taskPng,[Drawing.Imaging.ImageFormat]::Png)
    }finally{$taskBitmap.Dispose()}
    $taskRecord=[ordered]@{emfPath=$taskEmf;pngPath=$taskPng;nativeWidth=$taskMeta.Width;nativeHeight=$taskMeta.Height;widthPx=$taskWidth;heightPx=$taskHeight;aspectPreserved=$true;origin='SSMS Copy Diagram to Clipboard, CF_ENHMETAFILE';nativeEmfSha256=(Get-FileHash -LiteralPath $taskEmf -Algorithm SHA256).Hash;pngSha256=(Get-FileHash -LiteralPath $taskPng -Algorithm SHA256).Hash}
}finally{$taskMeta.Dispose()}
$taskRecord|ConvertTo-Json|Set-Content -LiteralPath ($taskBase+'.export.json') -Encoding UTF8
$taskRecord|ConvertTo-Json -Compress
