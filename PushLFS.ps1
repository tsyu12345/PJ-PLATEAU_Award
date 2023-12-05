param(
    [string[]]$FilesToZip,
    [string]$DestinationDirectory
)

# ZIPファイルの名前を生成（現在のタイムスタンプを使用）
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$zipFileName = "Archive_$timestamp.zip"

# 指定されたファイルをZIP形式で圧縮
Compress-Archive -Path $FilesToZip -DestinationPath $zipFileName

# ZIPファイルを指定されたディレクトリに転送
Move-Item -Path $zipFileName -Destination $DestinationDirectory
