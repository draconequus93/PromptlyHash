param(
    [string]$OutputPath = (Join-Path $PSScriptRoot "..\assets\PromptlyHash.ico")
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$size = 256
$bitmap = [System.Drawing.Bitmap]::new($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)

$path = $null
$navyBrush = $null
$cyanBrush = $null
$hashPen = $null
$checkPen = $null

try {
    $navy = [System.Drawing.Color]::FromArgb(23, 54, 93)
    $cyan = [System.Drawing.Color]::FromArgb(39, 194, 208)
    $white = [System.Drawing.Color]::White

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $navyBrush = [System.Drawing.SolidBrush]::new($navy)
    $cyanBrush = [System.Drawing.SolidBrush]::new($cyan)
    $hashPen = [System.Drawing.Pen]::new($white, 22)
    $checkPen = [System.Drawing.Pen]::new($navy, 12)

    # ICO files can contain PNG data. This produces a modern 256 px, 32-bit icon
    # without requiring an external image-conversion dependency.
    $cornerRadius = 58
    $diameter = $cornerRadius * 2
    $path.AddArc(0, 0, $diameter, $diameter, 180, 90)
    $path.AddArc($size - $diameter, 0, $diameter, $diameter, 270, 90)
    $path.AddArc($size - $diameter, $size - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc(0, $size - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    $graphics.FillPath($navyBrush, $path)

    $hashPen.StartCap = $hashPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLine($hashPen, 92, 63, 79, 193)
    $graphics.DrawLine($hashPen, 158, 63, 145, 193)
    $graphics.DrawLine($hashPen, 59, 108, 199, 108)
    $graphics.DrawLine($hashPen, 54, 158, 194, 158)

    $graphics.FillEllipse($cyanBrush, 150, 150, 82, 82)
    $checkPen.StartCap = $checkPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $checkPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $graphics.DrawLines($checkPen, [System.Drawing.Point[]]@(
        [System.Drawing.Point]::new(170, 191),
        [System.Drawing.Point]::new(184, 205),
        [System.Drawing.Point]::new(213, 174)
    ))

    $pngStream = [System.IO.MemoryStream]::new()
    try {
        $bitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
        $pngBytes = $pngStream.ToArray()
    }
    finally {
        $pngStream.Dispose()
    }

    $resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
    $outputDirectory = [System.IO.Path]::GetDirectoryName($resolvedOutput)
    [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null

    $fileStream = [System.IO.File]::Create($resolvedOutput)
    $writer = [System.IO.BinaryWriter]::new($fileStream)
    try {
        $writer.Write([uint16]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]1)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$pngBytes.Length)
        $writer.Write([uint32]22)
        $writer.Write($pngBytes)
    }
    finally {
        $writer.Dispose()
    }

}
finally {
    @($path, $navyBrush, $cyanBrush, $hashPen, $checkPen) |
        Where-Object { $null -ne $_ } |
        ForEach-Object { $_.Dispose() }
    $graphics.Dispose()
    $bitmap.Dispose()
}

Write-Host "Generated $resolvedOutput"
